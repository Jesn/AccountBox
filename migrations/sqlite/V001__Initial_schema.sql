CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory_Sqlite" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory_Sqlite" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "ApiKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_ApiKeys" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "KeyPlaintext" TEXT NOT NULL,
    "KeyHash" TEXT NOT NULL,
    "ScopeType" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" TEXT NOT NULL,
    "LastUsedAt" TEXT NULL
);

CREATE TABLE "LoginAttempts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_LoginAttempts" PRIMARY KEY AUTOINCREMENT,
    "IPAddress" TEXT NOT NULL,
    "AttemptTime" TEXT NOT NULL,
    "IsSuccessful" INTEGER NOT NULL,
    "FailureReason" TEXT NULL,
    "UserAgent" TEXT NULL
);

CREATE TABLE "Websites" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Websites" PRIMARY KEY AUTOINCREMENT,
    "Domain" TEXT NOT NULL,
    "DisplayName" TEXT NOT NULL,
    "Tags" text NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL
);

CREATE TABLE "Accounts" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY AUTOINCREMENT,
    "WebsiteId" INTEGER NOT NULL,
    "Username" TEXT NOT NULL,
    "Password" text NOT NULL,
    "Notes" text NULL,
    "Tags" text NULL,
    "Status" INTEGER NOT NULL,
    "ExtendedData" text NOT NULL,
    "IsDeleted" INTEGER NOT NULL DEFAULT 0,
    "DeletedAt" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Accounts_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApiKeyWebsiteScopes" (
    "ApiKeyId" INTEGER NOT NULL,
    "WebsiteId" INTEGER NOT NULL,
    CONSTRAINT "PK_ApiKeyWebsiteScopes" PRIMARY KEY ("ApiKeyId", "WebsiteId"),
    CONSTRAINT "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId" FOREIGN KEY ("ApiKeyId") REFERENCES "ApiKeys" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApiKeyWebsiteScopes_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Accounts_CreatedAt" ON "Accounts" ("CreatedAt");

CREATE INDEX "IX_Accounts_DeletedAt" ON "Accounts" ("DeletedAt");

CREATE INDEX "IX_Accounts_IsDeleted" ON "Accounts" ("IsDeleted");

CREATE INDEX "IX_Accounts_Username" ON "Accounts" ("Username");

CREATE INDEX "IX_Accounts_WebsiteId" ON "Accounts" ("WebsiteId");

CREATE INDEX "IX_Accounts_WebsiteId_IsDeleted" ON "Accounts" ("WebsiteId", "IsDeleted");

CREATE UNIQUE INDEX "IX_ApiKeys_KeyPlaintext" ON "ApiKeys" ("KeyPlaintext");

CREATE INDEX "IX_ApiKeyWebsiteScopes_WebsiteId" ON "ApiKeyWebsiteScopes" ("WebsiteId");

CREATE INDEX "IX_LoginAttempts_AttemptTime" ON "LoginAttempts" ("AttemptTime");

CREATE INDEX "IX_LoginAttempts_IPAddress_AttemptTime" ON "LoginAttempts" ("IPAddress", "AttemptTime");

CREATE INDEX "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");

CREATE INDEX "IX_Websites_DisplayName" ON "Websites" ("DisplayName");

CREATE UNIQUE INDEX "IX_Websites_Domain" ON "Websites" ("Domain");

INSERT INTO "__EFMigrationsHistory_Sqlite" ("MigrationId", "ProductVersion")
VALUES ('20251020051833_Sqlite_InitialCreate', '9.0.10');

CREATE TABLE "ApiKeys" (
    "Id" integer NOT NULL CONSTRAINT "PK_ApiKeys" PRIMARY KEY,
    "Name" character varying(100) NOT NULL,
    "KeyPlaintext" character varying(50) NOT NULL,
    "KeyHash" character varying(200) NOT NULL,
    "ScopeType" integer NOT NULL DEFAULT 0,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "UpdatedAt" timestamp with time zone NOT NULL,
    "LastUsedAt" timestamp with time zone NULL
);

CREATE TABLE "LoginAttempts" (
    "Id" bigint NOT NULL CONSTRAINT "PK_LoginAttempts" PRIMARY KEY,
    "IPAddress" character varying(45) NOT NULL,
    "AttemptTime" timestamp with time zone NOT NULL,
    "IsSuccessful" boolean NOT NULL,
    "FailureReason" character varying(200) NULL,
    "UserAgent" character varying(500) NULL
);

CREATE TABLE "Websites" (
    "Id" integer NOT NULL CONSTRAINT "PK_Websites" PRIMARY KEY,
    "Domain" character varying(255) NOT NULL,
    "DisplayName" character varying(255) NOT NULL,
    "Tags" text NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL
);

CREATE TABLE "Accounts" (
    "Id" integer NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY,
    "WebsiteId" integer NOT NULL,
    "Username" character varying(255) NOT NULL,
    "Password" text NOT NULL,
    "Notes" text NULL,
    "Tags" text NULL,
    "Status" integer NOT NULL,
    "ExtendedData" text NOT NULL,
    "IsDeleted" boolean NOT NULL DEFAULT 0,
    "DeletedAt" timestamp with time zone NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "FK_Accounts_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE CASCADE
);

CREATE TABLE "ApiKeyWebsiteScopes" (
    "ApiKeyId" integer NOT NULL,
    "WebsiteId" integer NOT NULL,
    CONSTRAINT "PK_ApiKeyWebsiteScopes" PRIMARY KEY ("ApiKeyId", "WebsiteId"),
    CONSTRAINT "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId" FOREIGN KEY ("ApiKeyId") REFERENCES "ApiKeys" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApiKeyWebsiteScopes_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Accounts_CreatedAt" ON "Accounts" ("CreatedAt");

CREATE INDEX "IX_Accounts_DeletedAt" ON "Accounts" ("DeletedAt");

CREATE INDEX "IX_Accounts_IsDeleted" ON "Accounts" ("IsDeleted");

CREATE INDEX "IX_Accounts_Username" ON "Accounts" ("Username");

CREATE INDEX "IX_Accounts_WebsiteId" ON "Accounts" ("WebsiteId");

CREATE INDEX "IX_Accounts_WebsiteId_IsDeleted" ON "Accounts" ("WebsiteId", "IsDeleted");

CREATE UNIQUE INDEX "IX_ApiKeys_KeyPlaintext" ON "ApiKeys" ("KeyPlaintext");

CREATE INDEX "IX_ApiKeyWebsiteScopes_WebsiteId" ON "ApiKeyWebsiteScopes" ("WebsiteId");

CREATE INDEX "IX_LoginAttempts_AttemptTime" ON "LoginAttempts" ("AttemptTime");

CREATE INDEX "IX_LoginAttempts_IPAddress_AttemptTime" ON "LoginAttempts" ("IPAddress", "AttemptTime");

CREATE INDEX "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");

CREATE INDEX "IX_Websites_DisplayName" ON "Websites" ("DisplayName");

CREATE UNIQUE INDEX "IX_Websites_Domain" ON "Websites" ("Domain");

INSERT INTO "__EFMigrationsHistory_Sqlite" ("MigrationId", "ProductVersion")
VALUES ('20251020052658_PostgreSQL_InitialCreate', '9.0.10');

CREATE TABLE "ef_temp_Websites" (
    "Id" int NOT NULL CONSTRAINT "PK_Websites" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" datetime(6) NOT NULL,
    "DisplayName" varchar(255) NOT NULL,
    "Domain" varchar(255) NOT NULL,
    "Tags" text NULL,
    "UpdatedAt" datetime(6) NOT NULL
);

INSERT INTO "ef_temp_Websites" ("Id", "CreatedAt", "DisplayName", "Domain", "Tags", "UpdatedAt")
SELECT "Id", "CreatedAt", "DisplayName", "Domain", "Tags", "UpdatedAt"
FROM "Websites";

CREATE TABLE "ef_temp_LoginAttempts" (
    "Id" bigint NOT NULL CONSTRAINT "PK_LoginAttempts" PRIMARY KEY AUTOINCREMENT,
    "AttemptTime" datetime(6) NOT NULL,
    "FailureReason" varchar(200) NULL,
    "IPAddress" varchar(45) NOT NULL,
    "IsSuccessful" tinyint(1) NOT NULL,
    "UserAgent" varchar(500) NULL
);

INSERT INTO "ef_temp_LoginAttempts" ("Id", "AttemptTime", "FailureReason", "IPAddress", "IsSuccessful", "UserAgent")
SELECT "Id", "AttemptTime", "FailureReason", "IPAddress", "IsSuccessful", "UserAgent"
FROM "LoginAttempts";

CREATE TABLE "ef_temp_ApiKeyWebsiteScopes" (
    "ApiKeyId" int NOT NULL,
    "WebsiteId" int NOT NULL,
    CONSTRAINT "PK_ApiKeyWebsiteScopes" PRIMARY KEY ("ApiKeyId", "WebsiteId"),
    CONSTRAINT "FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId" FOREIGN KEY ("ApiKeyId") REFERENCES "ApiKeys" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ApiKeyWebsiteScopes_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_ApiKeyWebsiteScopes" ("ApiKeyId", "WebsiteId")
SELECT "ApiKeyId", "WebsiteId"
FROM "ApiKeyWebsiteScopes";

CREATE TABLE "ef_temp_ApiKeys" (
    "Id" int NOT NULL CONSTRAINT "PK_ApiKeys" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" datetime(6) NOT NULL,
    "KeyHash" varchar(200) NOT NULL,
    "KeyPlaintext" varchar(50) NOT NULL,
    "LastUsedAt" datetime(6) NULL,
    "Name" varchar(100) NOT NULL,
    "ScopeType" int NOT NULL DEFAULT 0,
    "UpdatedAt" datetime(6) NOT NULL
);

INSERT INTO "ef_temp_ApiKeys" ("Id", "CreatedAt", "KeyHash", "KeyPlaintext", "LastUsedAt", "Name", "ScopeType", "UpdatedAt")
SELECT "Id", "CreatedAt", "KeyHash", "KeyPlaintext", "LastUsedAt", "Name", "ScopeType", "UpdatedAt"
FROM "ApiKeys";

CREATE TABLE "ef_temp_Accounts" (
    "Id" int NOT NULL CONSTRAINT "PK_Accounts" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" datetime(6) NOT NULL,
    "DeletedAt" datetime(6) NULL,
    "ExtendedData" text NOT NULL,
    "IsDeleted" tinyint(1) NOT NULL DEFAULT 0,
    "Notes" text NULL,
    "Password" text NOT NULL,
    "Status" int NOT NULL,
    "Tags" text NULL,
    "UpdatedAt" datetime(6) NOT NULL,
    "Username" varchar(255) NOT NULL,
    "WebsiteId" int NOT NULL,
    CONSTRAINT "FK_Accounts_Websites_WebsiteId" FOREIGN KEY ("WebsiteId") REFERENCES "Websites" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Accounts" ("Id", "CreatedAt", "DeletedAt", "ExtendedData", "IsDeleted", "Notes", "Password", "Status", "Tags", "UpdatedAt", "Username", "WebsiteId")
SELECT "Id", "CreatedAt", "DeletedAt", "ExtendedData", "IsDeleted", "Notes", "Password", "Status", "Tags", "UpdatedAt", "Username", "WebsiteId"
FROM "Accounts";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Websites";

ALTER TABLE "ef_temp_Websites" RENAME TO "Websites";

DROP TABLE "LoginAttempts";

ALTER TABLE "ef_temp_LoginAttempts" RENAME TO "LoginAttempts";

DROP TABLE "ApiKeyWebsiteScopes";

ALTER TABLE "ef_temp_ApiKeyWebsiteScopes" RENAME TO "ApiKeyWebsiteScopes";

DROP TABLE "ApiKeys";

ALTER TABLE "ef_temp_ApiKeys" RENAME TO "ApiKeys";

DROP TABLE "Accounts";

ALTER TABLE "ef_temp_Accounts" RENAME TO "Accounts";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Websites_CreatedAt" ON "Websites" ("CreatedAt");

CREATE INDEX "IX_Websites_DisplayName" ON "Websites" ("DisplayName");

CREATE UNIQUE INDEX "IX_Websites_Domain" ON "Websites" ("Domain");

CREATE INDEX "IX_LoginAttempts_AttemptTime" ON "LoginAttempts" ("AttemptTime");

CREATE INDEX "IX_LoginAttempts_IPAddress_AttemptTime" ON "LoginAttempts" ("IPAddress", "AttemptTime");

CREATE INDEX "IX_ApiKeyWebsiteScopes_WebsiteId" ON "ApiKeyWebsiteScopes" ("WebsiteId");

CREATE UNIQUE INDEX "IX_ApiKeys_KeyPlaintext" ON "ApiKeys" ("KeyPlaintext");

CREATE INDEX "IX_Accounts_CreatedAt" ON "Accounts" ("CreatedAt");

CREATE INDEX "IX_Accounts_DeletedAt" ON "Accounts" ("DeletedAt");

CREATE INDEX "IX_Accounts_IsDeleted" ON "Accounts" ("IsDeleted");

CREATE INDEX "IX_Accounts_Username" ON "Accounts" ("Username");

CREATE INDEX "IX_Accounts_WebsiteId" ON "Accounts" ("WebsiteId");

CREATE INDEX "IX_Accounts_WebsiteId_IsDeleted" ON "Accounts" ("WebsiteId", "IsDeleted");

COMMIT;

INSERT INTO "__EFMigrationsHistory_Sqlite" ("MigrationId", "ProductVersion")
VALUES ('20251020052709_MySQL_InitialCreate', '9.0.10');

