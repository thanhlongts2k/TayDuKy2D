-- ============================================================================
-- Tây Du Ký 2D — Map storage schema (PostgreSQL / Supabase)
-- Apply via Supabase SQL Editor, or:  psql "$DATABASE_URL" -f db/schema.sql
-- ============================================================================

CREATE TABLE IF NOT EXISTS maps (
    id          INTEGER PRIMARY KEY,            -- map id (matches maps.json id, e.g. 101)
    name        TEXT NOT NULL,                  -- human-friendly name (admin convenience)
    data        JSONB NOT NULL,                 -- full MapConfig: width/height/spawn/obstacles/npcs/portals/bg_resource_path
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Auto-touch updated_at whenever an admin edits a map (visible in Supabase Studio)
CREATE OR REPLACE FUNCTION set_maps_updated_at() RETURNS trigger AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_maps_updated_at ON maps;
CREATE TRIGGER trg_maps_updated_at
    BEFORE UPDATE ON maps
    FOR EACH ROW
    EXECUTE FUNCTION set_maps_updated_at();

-- NOTE: the game server computes each map's cache version as a SHA1 of the JSONB
-- content, so editing `data` automatically invalidates client caches — no manual
-- version bump needed.
