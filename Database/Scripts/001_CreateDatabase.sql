-- AssetGuard Database Creation Script
-- Run against your SQL Server instance

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AssetGuardDb')
BEGIN
    CREATE DATABASE AssetGuardDb;
END
GO

USE AssetGuardDb;
GO
