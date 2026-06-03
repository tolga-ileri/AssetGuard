USE AssetGuardDb;
GO

-- Users
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        UserName    NVARCHAR(100)  NOT NULL UNIQUE,
        Email       NVARCHAR(200)  NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        FullName    NVARCHAR(200)  NOT NULL,
        Role        NVARCHAR(50)   NOT NULL DEFAULT 'User',
        IsActive    BIT            NOT NULL DEFAULT 1,
        CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2      NULL
    );
END
GO

-- Employees
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
BEGIN
    CREATE TABLE Employees (
        Id         INT IDENTITY(1,1) PRIMARY KEY,
        FullName   NVARCHAR(200) NOT NULL,
        Department NVARCHAR(100) NOT NULL,
        Position   NVARCHAR(100) NOT NULL,
        Email      NVARCHAR(200) NOT NULL,
        Phone      NVARCHAR(50)  NULL,
        IsActive   BIT           NOT NULL DEFAULT 1,
        CreatedAt  DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Devices
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Devices')
BEGIN
    CREATE TABLE Devices (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        AssetTag        NVARCHAR(50)  NOT NULL UNIQUE,
        SerialNumber    NVARCHAR(100) NOT NULL,
        DeviceName      NVARCHAR(200) NOT NULL,
        DeviceType      NVARCHAR(50)  NOT NULL,
        Brand           NVARCHAR(100) NOT NULL,
        Model           NVARCHAR(100) NOT NULL,
        PurchaseDate    DATE          NOT NULL,
        WarrantyEndDate DATE          NULL,
        Status          NVARCHAR(50)  NOT NULL DEFAULT 'Available',
        Location        NVARCHAR(200) NULL,
        Notes           NVARCHAR(MAX) NULL,
        CreatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt       DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Assignments
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assignments')
BEGIN
    CREATE TABLE Assignments (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId        INT           NOT NULL,
        EmployeeId      INT           NOT NULL,
        AssignedDate    DATE          NOT NULL,
        ReturnDate      DATE          NULL,
        Status          NVARCHAR(50)  NOT NULL DEFAULT 'Active',
        AssignmentNote  NVARCHAR(500) NULL,
        ReturnNote      NVARCHAR(500) NULL,
        CONSTRAINT FK_Assignments_Devices   FOREIGN KEY (DeviceId)   REFERENCES Devices(Id),
        CONSTRAINT FK_Assignments_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
    );
END
GO

-- MaintenanceRecords
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRecords')
BEGIN
    CREATE TABLE MaintenanceRecords (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId            INT            NOT NULL,
        MaintenanceDate     DATE           NOT NULL,
        MaintenanceType     NVARCHAR(100)  NOT NULL,
        Description         NVARCHAR(MAX)  NOT NULL,
        Cost                DECIMAL(18,2)  NOT NULL DEFAULT 0,
        PerformedBy         NVARCHAR(200)  NOT NULL,
        NextMaintenanceDate DATE           NULL,
        CONSTRAINT FK_MaintenanceRecords_Devices FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
    );
END
GO

-- Warranties
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Warranties')
BEGIN
    CREATE TABLE Warranties (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId        INT           NOT NULL,
        Provider        NVARCHAR(200) NOT NULL,
        StartDate       DATE          NOT NULL,
        EndDate         DATE          NOT NULL,
        CoverageDetails NVARCHAR(MAX) NULL,
        IsActive        BIT           NOT NULL DEFAULT 1,
        CONSTRAINT FK_Warranties_Devices FOREIGN KEY (DeviceId) REFERENCES Devices(Id)
    );
END
GO

-- AuditLogs
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
        UserName    NVARCHAR(100) NOT NULL,
        ActionType  NVARCHAR(50)  NOT NULL,
        EntityName  NVARCHAR(100) NOT NULL,
        EntityId    INT           NULL,
        Description NVARCHAR(500) NOT NULL,
        CreatedAt   DATETIME2     NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Devices_Status')
    CREATE INDEX IX_Devices_Status ON Devices(Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Devices_DeviceType')
    CREATE INDEX IX_Devices_DeviceType ON Devices(DeviceType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Assignments_Status')
    CREATE INDEX IX_Assignments_Status ON Assignments(Status);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLogs_CreatedAt')
    CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);
GO
