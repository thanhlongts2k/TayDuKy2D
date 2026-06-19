param(
    [Parameter(Mandatory=$true)]
    [string]$CharacterName, # e.g. "than_toc_sheet" or "ma_toc_sheet" or "yeu_toc_sheet"
    [Parameter(Mandatory=$true)]
    [string]$RawImagePath  # Path to the raw generated PNG
)

# 1. Setup paths
$clientSpritesDir = "d:\AgentAI\TayDuKy2D\tayduky-client\Assets\Sprites\Characters"
$outputPath = "$clientSpritesDir\$CharacterName.png"
$metaPath = "$outputPath.meta"

Write-Host "Step 1: Running Python processor for transparency and auto-centering..."
python d:\AgentAI\TayDuKy2D\utils\process_spritesheet.py --input "$RawImagePath" --output "$outputPath" --detect-bg --align-y bottom --bottom-margin 24

if ($LASTEXITCODE -ne 0) {
    Write-Error "Python sprite processing failed!"
    exit 1
}

# 2. Check if .meta file exists. If not, generate a default template with a new GUID
if (-not (Test-Path $metaPath)) {
    Write-Host "Step 2: Creating new .meta file template..."
    $guid = [guid]::NewGuid().ToString("n")
    $template = @"
fileFormatVersion: 2
guid: $guid
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: 100
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  sprites: []
  outline: []
  customData: 
  physicsShape: []
  bones: []
  secondaryTextures: []
  spriteCustomMetadata:
    entries: []
  nameFileIdTable: {}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"@
    Set-Content -Path $metaPath -Value $template
}

# 3. Generate 16 sub-sprites (4x4 Grid Standard) and replace sprites block in meta
Write-Host "Step 3: Calculating and injecting sprite slices..."
$content = Get-Content -Path $metaPath -Raw

$spritesYaml = "  sprites:`n"
$directions = @("up", "right", "left", "down") # row 0=up, row 1=right, row 2=left, row 3=down
$spriteWidth = 256
$spriteHeight = 256

$idx = 0
for ($r = 0; $r -lt 4; $r++) {
    $dir = $directions[$r]
    $y = $r * $spriteHeight
    for ($c = 0; $c -lt 4; $c++) {
        $x = $c * $spriteWidth
        
        # Generate a unique spriteID (32 hex characters)
        $spriteID = ""
        for ($i = 0; $i -lt 32; $i++) {
            $spriteID += "{0:x}" -f (Get-Random -Minimum 0 -Maximum 16)
        }
        
        $spritesYaml += "    - serializedVersion: 2`n"
        $spritesYaml += "      name: $($CharacterName)_$($dir)_$($c)`n"
        $spritesYaml += "      rect:`n"
        $spritesYaml += "        serializedVersion: 2`n"
        $spritesYaml += "        x: $x`n"
        $spritesYaml += "        y: $y`n"
        $spritesYaml += "        width: $spriteWidth`n"
        $spritesYaml += "        height: $spriteHeight`n"
        $spritesYaml += "      alignment: 0`n"
        $spritesYaml += "      pivot: {x: 0.5, y: 0.5}`n"
        $spritesYaml += "      border: {x: 0, y: 0, z: 0, w: 0}`n"
        $spritesYaml += "      outline: []`n"
        $spritesYaml += "      physicsShape: []`n"
        $spritesYaml += "      tessellationDetail: 0`n"
        $spritesYaml += "      bones: []`n"
        $spritesYaml += "      spriteID: $spriteID`n"
        $spritesYaml += "      internalID: $(21300000 + $idx)`n"
        
        $idx++
    }
}

# Replace the sprites list inside the meta file
if ($content -match "  sprites: \[\]") {
    $content = $content -replace "  sprites: \[\]", $spritesYaml.TrimEnd()
} else {
    $pattern = "(?s)  sprites:.*?(?=  outline:)"
    $content = [regex]::Replace($content, $pattern, $spritesYaml)
}

Set-Content -Path $metaPath -Value $content -NoNewline
Write-Host "Success! Character '$CharacterName' sheet created and sliced at: $outputPath"
