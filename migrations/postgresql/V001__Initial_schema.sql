CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory_PostgreSQL" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory_PostgreSQL" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE TABLE "ApiKeys" (
        "Id" INTEGER NOT NULL,
        "Name" TEXT NOT NULL,
        "KeyPlaintext" TEXT NOT NULL,
        "KeyHash" TEXT NOT NULL,
        "ScopeType" INTEGER NOT NULL DEFAULT 0,
        "CreatedAt" TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" TEXT NOT NULL,
        "LastUsedAt" TEXT,
        CONSTRAINT "PK_ApiKeys" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE TABLE "LoginAttempts" (
        "Id" INTEGER NOT NULL,
        "IPAddress" TEXT NOT NULL,
        "AttemptTime" TEXT NOT NULL,
        "IsSuccessful" INTEGER NOT NULL,
        "FailureReason" TEXT,
        "UserAgent" TEXT,
        CONSTRAINT "PK_LoginAttempts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE TABLE "Websites" (
        "Id" INTEGER NOT NULL,
        "Domain" TEXT NOT NULL,
        "DisplayName" TEXT NOT NULL,
        "Tags" text,
        "CreatedAt" TEXT NOT NULL,
        "UpdatedAt" TEXT NOT NULL,
        CONSTRAINT "PK_Websites" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE TABLE "Accounts" (
        "Id" INTEGER NOT NULL,
        "WebsiteId" INTEGER NOT NULL,
        "Username" TEXT NOT NULL,
        "Password" text NOT NULL,
        "Notes" text,
        "Tags" text,
        "Status" INTEGER NOT NULL,
        "ExtendedData" text NOT NULL,
        "IsDeleted" INTEGER NOT NULL DEFAULT 0,
        "DeletedAt" TEXT,
        "CreatedAt" TEXT NOT NULL,
        "UpdatedAt" TEXT NOT NULL,
        CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Accounts_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE TABLE "ApiKeyWebsiteScopes" (
        "ApiKeyId" INTEGER NOT NULL,
        "WebsiteId" INTEGER NOT NULL,
        CONSTRAINT "PK_ApiKeyWebsiteScopes" PRIMARY KEY ("ApiKeyId", "WebsiteId"),
        CONSTRAINT "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId" FOREIGN KEY ("ApiKeyId") REFERENCES "ApiKeys" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ApiKeyWebsiteScopes_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_CreatedAt" ON "Accounts" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_DeletedAt" ON "Accounts" ("DeletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_IsDeleted" ON "Accounts" ("IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_Username" ON "Accounts" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_WebsiteId" ON "Accounts" ("WebsiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_WebsiteId_IsDeleted" ON "Accounts" ("WebsiteId", "IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ApiKeys_KeyPlaintext" ON "ApiKeys" ("KeyPlaintext");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_ApiKeyWebsiteScopes_WebsiteId" ON "ApiKeyWebsiteScopes" ("WebsiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_LoginAttempts_AttemptTime" ON "LoginAttempts" ("AttemptTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_LoginAttempts_IPAddress_AttemptTime" ON "LoginAttempts" ("IPAddress", "AttemptTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE INDEX "IX_Websites_DisplayName" ON "Websites" ("DisplayName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Websites_Domain" ON "Websites" ("Domain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020051833_Sqlite_InitialCreate') THEN
    INSERT INTO public."__EFMigrationsHistory_PostgreSQL" ("MigrationId", "ProductVersion")
    VALUES ('20251020051833_Sqlite_InitialCreate', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE TABLE "ApiKeys" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Name" character varying(100) NOT NULL,
        "KeyPlaintext" character varying(50) NOT NULL,
        "KeyHash" character varying(200) NOT NULL,
        "ScopeType" integer NOT NULL DEFAULT 0,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        "UpdatedAt" timestamp with time zone NOT NULL,
        "LastUsedAt" timestamp with time zone,
        CONSTRAINT "PK_ApiKeys" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE TABLE "LoginAttempts" (
        "Id" bigint GENERATED BY DEFAULT AS IDENTITY,
        "IPAddress" character varying(45) NOT NULL,
        "AttemptTime" timestamp with time zone NOT NULL,
        "IsSuccessful" boolean NOT NULL,
        "FailureReason" character varying(200),
        "UserAgent" character varying(500),
        CONSTRAINT "PK_LoginAttempts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE TABLE "Websites" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "Domain" character varying(255) NOT NULL,
        "DisplayName" character varying(255) NOT NULL,
        "Tags" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Websites" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE TABLE "Accounts" (
        "Id" integer GENERATED BY DEFAULT AS IDENTITY,
        "WebsiteId" integer NOT NULL,
        "Username" character varying(255) NOT NULL,
        "Password" text NOT NULL,
        "Notes" text,
        "Tags" text,
        "Status" integer NOT NULL,
        "ExtendedData" text NOT NULL,
        "IsDeleted" boolean NOT NULL DEFAULT FALSE,
        "DeletedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Accounts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Accounts_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE TABLE "ApiKeyWebsiteScopes" (
        "ApiKeyId" integer NOT NULL,
        "WebsiteId" integer NOT NULL,
        CONSTRAINT "PK_ApiKeyWebsiteScopes" PRIMARY KEY ("ApiKeyId", "WebsiteId"),
        CONSTRAINT "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId" FOREIGN KEY ("ApiKeyId") REFERENCES "ApiKeys" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_ApiKeyWebsiteScopes_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_CreatedAt" ON "Accounts" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_DeletedAt" ON "Accounts" ("DeletedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_IsDeleted" ON "Accounts" ("IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_Username" ON "Accounts" ("Username");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_WebsiteId" ON "Accounts" ("WebsiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Accounts_WebsiteId_IsDeleted" ON "Accounts" ("WebsiteId", "IsDeleted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ApiKeys_KeyPlaintext" ON "ApiKeys" ("KeyPlaintext");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_ApiKeyWebsiteScopes_WebsiteId" ON "ApiKeyWebsiteScopes" ("WebsiteId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_LoginAttempts_AttemptTime" ON "LoginAttempts" ("AttemptTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_LoginAttempts_IPAddress_AttemptTime" ON "LoginAttempts" ("IPAddress", "AttemptTime");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE INDEX "IX_Websites_DisplayName" ON "Websites" ("DisplayName");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Websites_Domain" ON "Websites" ("Domain");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052658_PostgreSQL_InitialCreate') THEN
    INSERT INTO public."__EFMigrationsHistory_PostgreSQL" ("MigrationId", "ProductVersion")
    VALUES ('20251020052658_PostgreSQL_InitialCreate', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Websites" ALTER COLUMN "UpdatedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Websites" ALTER COLUMN "Domain" TYPE varchar(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Websites" ALTER COLUMN "DisplayName" TYPE varchar(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Websites" ALTER COLUMN "CreatedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Websites" ALTER COLUMN "Id" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "LoginAttempts" ALTER COLUMN "UserAgent" TYPE varchar(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "LoginAttempts" ALTER COLUMN "IsSuccessful" TYPE tinyint(1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "LoginAttempts" ALTER COLUMN "IPAddress" TYPE varchar(45);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "LoginAttempts" ALTER COLUMN "FailureReason" TYPE varchar(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "LoginAttempts" ALTER COLUMN "AttemptTime" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeyWebsiteScopes" ALTER COLUMN "WebsiteId" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeyWebsiteScopes" ALTER COLUMN "ApiKeyId" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "UpdatedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "ScopeType" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "Name" TYPE varchar(100);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "LastUsedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "KeyPlaintext" TYPE varchar(50);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "KeyHash" TYPE varchar(200);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "CreatedAt" TYPE datetime(6);
    ALTER TABLE "ApiKeys" ALTER COLUMN "CreatedAt" DROP DEFAULT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "ApiKeys" ALTER COLUMN "Id" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "WebsiteId" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "Username" TYPE varchar(255);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "UpdatedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "Status" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "IsDeleted" TYPE tinyint(1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "DeletedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "CreatedAt" TYPE datetime(6);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    ALTER TABLE "Accounts" ALTER COLUMN "Id" TYPE int;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM public."__EFMigrationsHistory_PostgreSQL" WHERE "MigrationId" = '20251020052709_MySQL_InitialCreate') THEN
    INSERT INTO public."__EFMigrationsHistory_PostgreSQL" ("MigrationId", "ProductVersion")
    VALUES ('20251020052709_MySQL_InitialCreate', '9.0.10');
    END IF;
END $EF$;
COMMIT;

