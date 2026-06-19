#!/usr/bin/env python3
"""Utility script to parse, classify, and index project documentation.
Generates:
- `Docs/Classified_Docs.md` – a markdown with sections per business domain.
- `Docs/Traceability_Matrix.md` – mapping domain → config → client class → server module.
"""
import os, re, json

# Root of the client project (three levels up from this script)
ROOT = os.path.abspath(os.path.join(__file__, "../../../"))
# List of documentation files to process (relative to ROOT)
DOCS = [
    "Story_And_Lore.md",
    "Item_And_System_Catalogs.md",
    "Graphics_And_Tech_Requirements.md",
    "Additional_Proposals.md",
    "Testing_Guide.md"
]

def classify_doc(path):
    """Return a list of top‑level section titles (## headings) found in the file."""
    with open(path, "r", encoding="utf-8") as f:
        content = f.read()
    # Find headings like "## Something"
    return re.findall(r"^##\s+(.+)$", content, re.MULTILINE)

def main():
    # Ensure output folder exists
    docs_dir = os.path.join(ROOT, "Docs")
    os.makedirs(docs_dir, exist_ok=True)

    classified_md_path = os.path.join(docs_dir, "Classified_Docs.md")
    matrix_md_path = os.path.join(docs_dir, "Traceability_Matrix.md")

    # Build Classified Docs markdown
    with open(classified_md_path, "w", encoding="utf-8") as out_md:
        out_md.write("# Classified Documentation\n\n")
        for doc in DOCS:
            full_path = os.path.join(ROOT, doc)
            if not os.path.isfile(full_path):
                continue
            sections = classify_doc(full_path)
            out_md.write(f"## {os.path.basename(doc)}\n")
            for sec in sections:
                out_md.write(f"- {sec}\n")
            out_md.write("\n")

    # Build Traceability Matrix markdown
    with open(matrix_md_path, "w", encoding="utf-8") as out_mat:
        out_mat.write("| Domain | Source Doc | Client Class | Server Module |\n|---|---|---|---|\n")
        for doc in DOCS:
            full_path = os.path.join(ROOT, doc)
            if not os.path.isfile(full_path):
                continue
            sections = classify_doc(full_path)
            for sec in sections:
                out_mat.write(f"| {sec} | {os.path.basename(doc)} | [TODO] | [TODO] |\n")

if __name__ == "__main__":
    main()
