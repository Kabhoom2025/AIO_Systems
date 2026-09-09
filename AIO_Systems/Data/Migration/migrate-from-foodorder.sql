-- One-time data migration: FoodOrderDB -> SuperAdminDB
--
-- Copies the existing identity/tenant/platform-module reference data that FoodOrder.API
-- used to own into AIO_Systems' own database, preserving primary key values exactly
-- (JWTs, FKs, and the organizationId route/claim convention all depend on these IDs
-- staying stable across the cutover).
--
-- Run this ONCE, with both FoodOrder.API and AIO_Systems stopped (no concurrent writers),
-- after AIO_Systems' EF migrations have already been applied to SuperAdminDB (schema exists,
-- tables are empty). Both databases live on the same Postgres instance (localhost:5432),
-- so dblink is used to query FoodOrderDB from within SuperAdminDB.
--
-- Usage:
--   & 'C:\Program Files\PostgreSQL\17\bin\psql.exe' -h localhost -U postgres -d SuperAdminDB -f migrate-from-foodorder.sql

\set ON_ERROR_STOP on

CREATE EXTENSION IF NOT EXISTS dblink;

BEGIN;

-- ── Roles (explicit IDs: 1=SuperAdmin, 2=Admin, 3=Cashier, 4=Waiter, 5=InventoryManager) ──
INSERT INTO "Roles" ("Id", "RoleName")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "RoleName" FROM "Roles" ORDER BY "Id"')
    AS src("Id" integer, "RoleName" varchar(50));

-- ── RolePermissions ──────────────────────────────────────────────────────────
INSERT INTO "RolePermissions" ("Id", "RoleId", "Feature")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "RoleId", "Feature" FROM "RolePermissions" ORDER BY "Id"')
    AS src("Id" integer, "RoleId" integer, "Feature" varchar(100));

-- ── PlatformModules (explicit IDs: 1=restaurant, 2=pharmacy, 3=hr_management, 4=retail, 5=finance) ──
INSERT INTO "PlatformModules" ("Id", "Name", "Key", "Description", "Icon", "Color", "IsActive", "SortOrder", "CreatedDate")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "Name", "Key", "Description", "Icon", "Color", "IsActive", "SortOrder", "CreatedDate" FROM "PlatformModules" ORDER BY "Id"')
    AS src("Id" integer, "Name" varchar(100), "Key" varchar(50), "Description" varchar(500), "Icon" varchar(100), "Color" varchar(20), "IsActive" boolean, "SortOrder" integer, "CreatedDate" timestamptz);

-- ── Organizations (+ generate TenantKey, which has no source column) ─────────
INSERT INTO "Organizations" ("Id", "Name", "Address", "Phone", "Email", "LogoUrl", "IsActive", "Timezone", "Currency", "CreatedDate", "TenantKey")
SELECT "Id", "Name", "Address", "Phone", "Email", "LogoUrl", "IsActive", "Timezone", "Currency", "CreatedDate",
       lower(regexp_replace(regexp_replace("Name", '[^a-zA-Z0-9]+', '-', 'g'), '(^-+|-+$)', '', 'g')) || '-' || "Id" AS "TenantKey"
FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "Name", "Address", "Phone", "Email", "LogoUrl", "IsActive", "Timezone", "Currency", "CreatedDate" FROM "Organizations" ORDER BY "Id"')
    AS src("Id" integer, "Name" varchar(200), "Address" varchar(500), "Phone" varchar(30), "Email" varchar(200), "LogoUrl" varchar(500), "IsActive" boolean, "Timezone" varchar(50), "Currency" varchar(10), "CreatedDate" timestamptz);

-- ── Users ──────────────────────────────────────────────────────────────────
INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "RoleId", "IsActive", "CreatedDate", "OrganizationId", "PasswordResetToken", "PasswordResetTokenExpiry", "ProfileImage")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "Name", "Email", "PasswordHash", "RoleId", "IsActive", "CreatedDate", "OrganizationId", "PasswordResetToken", "PasswordResetTokenExpiry", "ProfileImage" FROM "Users" ORDER BY "Id"')
    AS src("Id" integer, "Name" varchar(100), "Email" varchar(150), "PasswordHash" text, "RoleId" integer, "IsActive" boolean, "CreatedDate" timestamptz, "OrganizationId" integer, "PasswordResetToken" text, "PasswordResetTokenExpiry" timestamptz, "ProfileImage" text);

-- ── OrganizationModules (composite key, no own Id) ────────────────────────────
INSERT INTO "OrganizationModules" ("OrganizationId", "PlatformModuleId", "IsEnabled")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "OrganizationId", "PlatformModuleId", "IsEnabled" FROM "OrganizationModules"')
    AS src("OrganizationId" integer, "PlatformModuleId" integer, "IsEnabled" boolean);

-- ── Licenses ───────────────────────────────────────────────────────────────
INSERT INTO "Licenses" ("Id", "OrganizationId", "Plan", "Status", "ExpiryDate", "MaxUsers", "Notes", "CreatedDate")
SELECT * FROM dblink('host=localhost port=5432 dbname=FoodOrderDB user=postgres password=1234',
    'SELECT "Id", "OrganizationId", "Plan", "Status", "ExpiryDate", "MaxUsers", "Notes", "CreatedDate" FROM "Licenses" ORDER BY "Id"')
    AS src("Id" integer, "OrganizationId" integer, "Plan" text, "Status" text, "ExpiryDate" timestamptz, "MaxUsers" integer, "Notes" text, "CreatedDate" timestamptz);

-- ── Re-sync identity sequences so future EF-generated inserts don't collide ───
SELECT setval(pg_get_serial_sequence('"Roles"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Roles"), 1));
SELECT setval(pg_get_serial_sequence('"RolePermissions"', 'Id'), COALESCE((SELECT MAX("Id") FROM "RolePermissions"), 1));
SELECT setval(pg_get_serial_sequence('"PlatformModules"', 'Id'), COALESCE((SELECT MAX("Id") FROM "PlatformModules"), 1));
SELECT setval(pg_get_serial_sequence('"Organizations"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Organizations"), 1));
SELECT setval(pg_get_serial_sequence('"Users"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Users"), 1));
SELECT setval(pg_get_serial_sequence('"Licenses"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Licenses"), 1));

COMMIT;

-- ── Verification: row counts (compare against the same queries run on FoodOrderDB) ──
SELECT 'Roles' AS table_name, count(*) FROM "Roles"
UNION ALL SELECT 'RolePermissions', count(*) FROM "RolePermissions"
UNION ALL SELECT 'PlatformModules', count(*) FROM "PlatformModules"
UNION ALL SELECT 'Organizations', count(*) FROM "Organizations"
UNION ALL SELECT 'Users', count(*) FROM "Users"
UNION ALL SELECT 'OrganizationModules', count(*) FROM "OrganizationModules"
UNION ALL SELECT 'Licenses', count(*) FROM "Licenses";
