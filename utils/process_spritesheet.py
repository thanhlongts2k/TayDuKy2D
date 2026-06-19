import os
import sys
import argparse
import math
import warnings
warnings.filterwarnings("ignore", category=DeprecationWarning)
from PIL import Image, ImageOps, ImageChops

def parse_color(color_str):
    if not color_str:
        return None
    color_str = color_str.strip()
    if color_str.startswith('#'):
        color_str = color_str.lstrip('#')
        return tuple(int(color_str[i:i+2], 16) for i in (0, 2, 4))
    elif ',' in color_str:
        return tuple(int(c.strip()) for c in color_str.split(','))
    return None

def color_distance(c1, c2):
    return math.sqrt(sum((a - b) ** 2 for a, b in zip(c1[:3], c2[:3])))

def remove_background(img, bg_color=None, tolerance=30, detect_bg=False):
    """
    Removes the background color and returns an RGBA image with transparency.
    """
    img = img.convert("RGBA")
    width, height = img.size
    
    if detect_bg or bg_color is None:
        # Sample corners to find the most common background color
        corners = [img.getpixel((0, 0)), img.getpixel((width - 1, 0)), 
                   img.getpixel((0, height - 1)), img.getpixel((width - 1, height - 1))]
        # Use the first one or voting. Let's use the top-left corner as standard.
        detected = corners[0]
        if bg_color is None:
            bg_color = detected[:3]
            print(f"Auto-detected background color: {bg_color}")
            
    data = img.getdata()
    new_data = []
    
    for item in data:
        # Check distance to background color
        if color_distance(item[:3], bg_color) < tolerance:
            # Make it fully transparent
            new_data.append((0, 0, 0, 0))
        else:
            new_data.append(item)
            
    img.putdata(new_data)
    return img

def clean_fringe_pixels(img, alpha_threshold=50):
    """
    Thresholds the alpha channel to remove semi-transparent fringe pixels.
    This creates clean, sharp pixel outlines characteristic of 16-bit retro games.
    """
    img = img.convert("RGBA")
    data = img.getdata()
    new_data = []
    for item in data:
        r, g, b, a = item
        if a < alpha_threshold:
            new_data.append((0, 0, 0, 0))
        else:
            new_data.append((r, g, b, 255))
    img.putdata(new_data)
    return img

def add_pixel_outline(sprite_img, outline_color=(0, 0, 0, 255)):
    """
    Adds a 1-pixel outline around the non-transparent parts of a single sprite.
    """
    width, height = sprite_img.size
    # Create an expanded canvas or work in place if sprite has padding
    outline_img = sprite_img.copy()
    
    # We check 4-connectivity neighbors for transparency
    pixels = sprite_img.load()
    out_pixels = outline_img.load()
    
    for y in range(height):
        for x in range(width):
            # If current pixel is transparent
            if pixels[x, y][3] == 0:
                # Check neighbors
                has_solid_neighbor = False
                for dx, dy in [(-1, 0), (1, 0), (0, -1), (0, 1)]:
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < width and 0 <= ny < height:
                        if pixels[nx, ny][3] > 0:
                            has_solid_neighbor = True
                            break
                if has_solid_neighbor:
                    out_pixels[x, y] = outline_color
                    
    return outline_img

def process_spritesheet(input_path, output_path, cols=4, rows=4, cell_size=256, 
                        bg_color=None, tolerance=30, detect_bg=False,
                        align_y='bottom', bottom_margin=16, top_margin=16,
                        alpha_threshold=50, clean_fringe=True, add_outline=False,
                        outline_color=(0, 0, 0, 255)):
    
    if not os.path.exists(input_path):
        print(f"Error: Input file '{input_path}' does not exist.")
        return False
        
    print(f"Loading image: {input_path}")
    img = Image.open(input_path)
    
    # Remove background first
    img = remove_background(img, bg_color=bg_color, tolerance=tolerance, detect_bg=detect_bg)
    
    if clean_fringe:
        img = clean_fringe_pixels(img, alpha_threshold=alpha_threshold)
        
    width, height = img.size
    
    # Get original cell sizes
    src_cell_w = width // cols
    src_cell_h = height // rows
    
    print(f"Original image size: {width}x{height}. Cell size: {src_cell_w}x{src_cell_h}")
    print(f"Target grid: {cols}x{rows} cells of size {cell_size}x{cell_size}")
    
    # Create target image
    target_w = cols * cell_size
    target_h = rows * cell_size
    target_img = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
    
    for r in range(rows):
        for c in range(cols):
            # Crop source cell
            left = c * src_cell_w
            top = r * src_cell_h
            right = left + src_cell_w
            bottom = top + src_cell_h
            
            cell_img = img.crop((left, top, right, bottom))
            
            # Find bounding box of non-transparent content
            # bbox is (left, top, right, bottom)
            bbox = cell_img.getbbox()
            
            new_cell = Image.new("RGBA", (cell_size, cell_size), (0, 0, 0, 0))
            
            if bbox:
                # Crop the actual sprite content
                sprite_crop = cell_img.crop(bbox)
                
                if add_outline:
                    sprite_crop = add_pixel_outline(sprite_crop, outline_color)
                    # Recompute bbox in case outline expanded it
                    # (Though since we worked within the cell, outline is within cell coordinates)
                
                sprite_w = bbox[2] - bbox[0]
                sprite_h = bbox[3] - bbox[1]
                
                # If the sprite is larger than the target cell size, scale it down proportionally
                if sprite_w > cell_size or sprite_h > cell_size:
                    scale = min(cell_size / sprite_w, cell_size / sprite_h)
                    new_w = int(sprite_w * scale)
                    new_h = int(sprite_h * scale)
                    sprite_crop = sprite_crop.resize((new_w, new_h), Image.Resampling.NEAREST)
                    sprite_w, sprite_h = new_w, new_h
                
                # Compute new placement inside the 256x256 target cell
                # Horizontally centered
                new_x = (cell_size - sprite_w) // 2
                
                # Vertically aligned
                if align_y == 'center':
                    new_y = (cell_size - sprite_h) // 2
                elif align_y == 'bottom':
                    new_y = cell_size - sprite_h - bottom_margin
                    # Make sure it doesn't go negative
                    if new_y < 0:
                        new_y = 0
                elif align_y == 'top':
                    new_y = top_margin
                else:
                    # Default center
                    new_y = (cell_size - sprite_h) // 2
                
                new_cell.paste(sprite_crop, (new_x, new_y), sprite_crop)
            else:
                # Cell is empty
                print(f"Warning: Cell at row {r}, col {c} is empty.")
                
            # Paste into target sheet
            target_x = c * cell_size
            target_y = r * cell_size
            target_img.paste(new_cell, (target_x, target_y))
            
    # Save output
    os.makedirs(os.path.dirname(os.path.abspath(output_path)), exist_ok=True)
    target_img.save(output_path, "PNG")
    print(f"Successfully processed and saved sprite sheet to: {output_path}")
    return True

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Clean, auto-center, and standardize AI character sprite sheets.")
    parser.add_argument("--input", required=True, help="Path to input raw sprite sheet image.")
    parser.add_argument("--output", required=True, help="Path to save the processed sprite sheet.")
    parser.add_argument("--cols", type=int, default=4, help="Number of columns in grid (default: 4)")
    parser.add_argument("--rows", type=int, default=4, help="Number of rows in grid (default: 4)")
    parser.add_argument("--cell-size", type=int, default=256, help="Target cell size in pixels (default: 256)")
    parser.add_argument("--bg-color", help="Background color to remove, either hex '#ffffff' or R,G,B format '255,255,255'")
    parser.add_argument("--tolerance", type=int, default=30, help="Chroma key color distance tolerance (default: 30)")
    parser.add_argument("--detect-bg", action="store_true", help="Auto-detect background color from top-left pixel")
    parser.add_argument("--align-y", choices=["center", "bottom", "top"], default="bottom", help="Vertical alignment (default: bottom)")
    parser.add_argument("--bottom-margin", type=int, default=24, help="Bottom margin in pixels when align-y is 'bottom' (default: 24)")
    parser.add_argument("--top-margin", type=int, default=16, help="Top margin in pixels when align-y is 'top' (default: 16)")
    parser.add_argument("--alpha-threshold", type=int, default=50, help="Alpha threshold to clean fringe pixels (default: 50)")
    parser.add_argument("--no-clean-fringe", action="store_false", dest="clean_fringe", help="Disable alpha thresholding fringe clean-up")
    parser.add_argument("--add-outline", action="store_true", help="Add a 1px solid black outline around the sprites")
    parser.add_argument("--outline-color", default="0,0,0,255", help="Color of the outline in R,G,B,A format")
    
    args = parser.parse_args()
    
    bg_col = parse_color(args.bg_color)
    out_col = tuple(int(c.strip()) for c in args.outline_color.split(',')) if args.outline_color else (0,0,0,255)
    
    process_spritesheet(
        input_path=args.input,
        output_path=args.output,
        cols=args.cols,
        rows=args.rows,
        cell_size=args.cell_size,
        bg_color=bg_col,
        tolerance=args.tolerance,
        detect_bg=args.detect_bg or (bg_col is None),
        align_y=args.align_y,
        bottom_margin=args.bottom_margin,
        top_margin=args.top_margin,
        alpha_threshold=args.alpha_threshold,
        clean_fringe=args.clean_fringe,
        add_outline=args.add_outline,
        outline_color=out_col
    )
