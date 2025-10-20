CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory_MySQL` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory_MySQL` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE TABLE `ApiKeys` (
        `Id` INTEGER NOT NULL,
        `Name` TEXT NOT NULL,
        `KeyPlaintext` TEXT NOT NULL,
        `KeyHash` TEXT NOT NULL,
        `ScopeType` INTEGER NOT NULL DEFAULT 0,
        `CreatedAt` TEXT NOT NULL,
        `UpdatedAt` TEXT NOT NULL,
        `LastUsedAt` TEXT NULL,
        CONSTRAINT `PK_ApiKeys` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE TABLE `LoginAttempts` (
        `Id` INTEGER NOT NULL,
        `IPAddress` TEXT NOT NULL,
        `AttemptTime` TEXT NOT NULL,
        `IsSuccessful` INTEGER NOT NULL,
        `FailureReason` TEXT NULL,
        `UserAgent` TEXT NULL,
        CONSTRAINT `PK_LoginAttempts` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE TABLE `Websites` (
        `Id` INTEGER NOT NULL,
        `Domain` TEXT NOT NULL,
        `DisplayName` TEXT NOT NULL,
        `Tags` text NULL,
        `CreatedAt` TEXT NOT NULL,
        `UpdatedAt` TEXT NOT NULL,
        CONSTRAINT `PK_Websites` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE TABLE `Accounts` (
        `Id` INTEGER NOT NULL,
        `WebsiteId` INTEGER NOT NULL,
        `Username` TEXT NOT NULL,
        `Password` text NOT NULL,
        `Notes` text NULL,
        `Tags` text NULL,
        `Status` INTEGER NOT NULL,
        `ExtendedData` text NOT NULL,
        `IsDeleted` INTEGER NOT NULL DEFAULT FALSE,
        `DeletedAt` TEXT NULL,
        `CreatedAt` TEXT NOT NULL,
        `UpdatedAt` TEXT NOT NULL,
        CONSTRAINT `PK_Accounts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Accounts_Websites_WebsiteId` FOREIGN KEY (`WebsiteId`) REFERENCES `Websites` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE TABLE `ApiKeyWebsiteScopes` (
        `ApiKeyId` INTEGER NOT NULL,
        `WebsiteId` INTEGER NOT NULL,
        CONSTRAINT `PK_ApiKeyWebsiteScopes` PRIMARY KEY (`ApiKeyId`, `WebsiteId`),
        CONSTRAINT `FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId` FOREIGN KEY (`ApiKeyId`) REFERENCES `ApiKeys` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ApiKeyWebsiteScopes_Websites_WebsiteId` FOREIGN KEY (`WebsiteId`) REFERENCES `Websites` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_CreatedAt` ON `Accounts` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_DeletedAt` ON `Accounts` (`DeletedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_IsDeleted` ON `Accounts` (`IsDeleted`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_Username` ON `Accounts` (`Username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_WebsiteId` ON `Accounts` (`WebsiteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_WebsiteId_IsDeleted` ON `Accounts` (`WebsiteId`, `IsDeleted`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ApiKeys_KeyPlaintext` ON `ApiKeys` (`KeyPlaintext`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_ApiKeyWebsiteScopes_WebsiteId` ON `ApiKeyWebsiteScopes` (`WebsiteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_LoginAttempts_AttemptTime` ON `LoginAttempts` (`AttemptTime`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_LoginAttempts_IPAddress_AttemptTime` ON `LoginAttempts` (`IPAddress`, `AttemptTime`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Websites_CreatedAt` ON `Websites` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE INDEX `IX_Websites_DisplayName` ON `Websites` (`DisplayName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Websites_Domain` ON `Websites` (`Domain`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020051833_Sqlite_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory_MySQL` (`MigrationId`, `ProductVersion`)
    VALUES ('20251020051833_Sqlite_InitialCreate', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE TABLE `ApiKeys` (
        `Id` integer NOT NULL,
        `Name` character varying(100) NOT NULL,
        `KeyPlaintext` character varying(50) NOT NULL,
        `KeyHash` character varying(200) NOT NULL,
        `ScopeType` integer NOT NULL DEFAULT 0,
        `CreatedAt` timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
        `UpdatedAt` timestamp with time zone NOT NULL,
        `LastUsedAt` timestamp with time zone NULL,
        CONSTRAINT `PK_ApiKeys` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE TABLE `LoginAttempts` (
        `Id` bigint NOT NULL,
        `IPAddress` character varying(45) NOT NULL,
        `AttemptTime` timestamp with time zone NOT NULL,
        `IsSuccessful` boolean NOT NULL,
        `FailureReason` character varying(200) NULL,
        `UserAgent` character varying(500) NULL,
        CONSTRAINT `PK_LoginAttempts` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE TABLE `Websites` (
        `Id` integer NOT NULL,
        `Domain` character varying(255) NOT NULL,
        `DisplayName` character varying(255) NOT NULL,
        `Tags` text NULL,
        `CreatedAt` timestamp with time zone NOT NULL,
        `UpdatedAt` timestamp with time zone NOT NULL,
        CONSTRAINT `PK_Websites` PRIMARY KEY (`Id`)
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE TABLE `Accounts` (
        `Id` integer NOT NULL,
        `WebsiteId` integer NOT NULL,
        `Username` character varying(255) NOT NULL,
        `Password` text NOT NULL,
        `Notes` text NULL,
        `Tags` text NULL,
        `Status` integer NOT NULL,
        `ExtendedData` text NOT NULL,
        `IsDeleted` boolean NOT NULL DEFAULT FALSE,
        `DeletedAt` timestamp with time zone NULL,
        `CreatedAt` timestamp with time zone NOT NULL,
        `UpdatedAt` timestamp with time zone NOT NULL,
        CONSTRAINT `PK_Accounts` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_Accounts_Websites_WebsiteId` FOREIGN KEY (`WebsiteId`) REFERENCES `Websites` (`Id`) ON DELETE CASCADE
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE TABLE `ApiKeyWebsiteScopes` (
        `ApiKeyId` integer NOT NULL,
        `WebsiteId` integer NOT NULL,
        CONSTRAINT `PK_ApiKeyWebsiteScopes` PRIMARY KEY (`ApiKeyId`, `WebsiteId`),
        CONSTRAINT `FK_ApiKeyWebsiteScopes_ApiKeys_ApiKeyId` FOREIGN KEY (`ApiKeyId`) REFERENCES `ApiKeys` (`Id`) ON DELETE CASCADE,
        CONSTRAINT `FK_ApiKeyWebsiteScopes_Websites_WebsiteId` FOREIGN KEY (`WebsiteId`) REFERENCES `Websites` (`Id`) ON DELETE RESTRICT
    );

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_CreatedAt` ON `Accounts` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_DeletedAt` ON `Accounts` (`DeletedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_IsDeleted` ON `Accounts` (`IsDeleted`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_Username` ON `Accounts` (`Username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_WebsiteId` ON `Accounts` (`WebsiteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Accounts_WebsiteId_IsDeleted` ON `Accounts` (`WebsiteId`, `IsDeleted`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_ApiKeys_KeyPlaintext` ON `ApiKeys` (`KeyPlaintext`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_ApiKeyWebsiteScopes_WebsiteId` ON `ApiKeyWebsiteScopes` (`WebsiteId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_LoginAttempts_AttemptTime` ON `LoginAttempts` (`AttemptTime`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_LoginAttempts_IPAddress_AttemptTime` ON `LoginAttempts` (`IPAddress`, `AttemptTime`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Websites_CreatedAt` ON `Websites` (`CreatedAt`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE INDEX `IX_Websites_DisplayName` ON `Websites` (`DisplayName`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_Websites_Domain` ON `Websites` (`Domain`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052658_PostgreSQL_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory_MySQL` (`MigrationId`, `ProductVersion`)
    VALUES ('20251020052658_PostgreSQL_InitialCreate', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Websites` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Websites` MODIFY COLUMN `Domain` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Websites` MODIFY COLUMN `DisplayName` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Websites` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Websites` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `UserAgent` varchar(500) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `IsSuccessful` tinyint(1) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `IPAddress` varchar(45) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `FailureReason` varchar(200) CHARACTER SET utf8mb4 NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `AttemptTime` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `LoginAttempts` MODIFY COLUMN `Id` bigint NOT NULL AUTO_INCREMENT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeyWebsiteScopes` MODIFY COLUMN `WebsiteId` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeyWebsiteScopes` MODIFY COLUMN `ApiKeyId` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `ScopeType` int NOT NULL DEFAULT 0;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `Name` varchar(100) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `LastUsedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `KeyPlaintext` varchar(50) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `KeyHash` varchar(200) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `ApiKeys` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `WebsiteId` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `Username` varchar(255) CHARACTER SET utf8mb4 NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `UpdatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `Status` int NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `IsDeleted` tinyint(1) NOT NULL DEFAULT FALSE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `DeletedAt` datetime(6) NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `CreatedAt` datetime(6) NOT NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    ALTER TABLE `Accounts` MODIFY COLUMN `Id` int NOT NULL AUTO_INCREMENT;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory_MySQL` WHERE `MigrationId` = '20251020052709_MySQL_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory_MySQL` (`MigrationId`, `ProductVersion`)
    VALUES ('20251020052709_MySQL_InitialCreate', '9.0.10');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

