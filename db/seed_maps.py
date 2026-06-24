"""Seed / sync the `maps` table from the canonical maps.json.

Idempotent: upserts by map id, so it is safe to re-run after editing maps.json.

Usage (PowerShell):
    $env:DATABASE_URL="postgresql://...";  python db/seed_maps.py
Usage (bash):
    DATABASE_URL="postgresql://..." python db/seed_maps.py
"""
import json
import os
import sys

import psycopg2
import psycopg2.extras

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MAPS_JSON = os.path.join(ROOT, "tayduky-client", "Assets", "Resources", "Maps", "maps.json")


def main():
    dsn = os.environ.get("DATABASE_URL", "").strip()
    if not dsn:
        print("ERROR: DATABASE_URL is not set. Export it first (see .env.example).")
        sys.exit(1)

    if not os.path.exists(MAPS_JSON):
        print(f"ERROR: maps.json not found at {MAPS_JSON}")
        sys.exit(1)

    with open(MAPS_JSON, "r", encoding="utf-8") as f:
        maps = json.load(f)

    conn = psycopg2.connect(dsn)
    try:
        with conn.cursor() as cur:
            for m in maps:
                cur.execute(
                    """
                    INSERT INTO maps (id, name, data)
                    VALUES (%s, %s, %s)
                    ON CONFLICT (id) DO UPDATE
                        SET name = EXCLUDED.name,
                            data = EXCLUDED.data
                    """,
                    (m["id"], m.get("name", ""), psycopg2.extras.Json(m)),
                )
        conn.commit()
        print(f"[OK] Seeded {len(maps)} map(s) into the 'maps' table: {[m['id'] for m in maps]}")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
