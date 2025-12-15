-- Database schema for Check_IT (PostgreSQL)

-- Database cleanup script: drop all tables and recreate users empty

-- Drop everything in public schema
DO $$
DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END
$$;

-- Recreate empty users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    hashed_password TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Index for faster lookup
CREATE INDEX IF NOT EXISTS idx_users_email ON users (email);

-- No seed data inserted
