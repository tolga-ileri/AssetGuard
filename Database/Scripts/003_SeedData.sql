USE AssetGuardDb;
GO

-- Default admin user (password: Admin@123)
-- Hash generated with BCrypt
IF NOT EXISTS (SELECT 1 FROM Users WHERE UserName = 'admin')
BEGIN
    INSERT INTO Users (UserName, Email, PasswordHash, FullName, Role, IsActive)
    VALUES (
        'admin',
        'admin@assetguard.local',
        '$2a$11$f7QHrrjBZXiFfpkE6giV.evyFTcsLl4nslWuNVtvQ0IR42y7LkpRG',
        'System Administrator',
        'Admin',
        1
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Employees)
BEGIN
    INSERT INTO Employees (FullName, Department, Position, Email, Phone, IsActive) VALUES
    ('John Smith',       'IT',           'IT Manager',        'john.smith@company.com',    '555-0101', 1),
    ('Sarah Johnson',    'Finance',      'Financial Analyst', 'sarah.j@company.com',       '555-0102', 1),
    ('Michael Brown',    'HR',           'HR Specialist',     'michael.b@company.com',     '555-0103', 1),
    ('Emily Davis',      'Marketing',    'Marketing Lead',    'emily.d@company.com',       '555-0104', 1),
    ('David Wilson',     'Engineering',  'Software Engineer', 'david.w@company.com',       '555-0105', 1),
    ('Lisa Anderson',    'Sales',        'Sales Director',    'lisa.a@company.com',        '555-0106', 1),
    ('Robert Taylor',    'Operations',   'Operations Manager','robert.t@company.com',      '555-0107', 1),
    ('Jennifer Martinez','Legal',        'Legal Counsel',     'jennifer.m@company.com',    '555-0108', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM Devices)
BEGIN
    INSERT INTO Devices (AssetTag, SerialNumber, DeviceName, DeviceType, Brand, Model, PurchaseDate, WarrantyEndDate, Status, Location, Notes) VALUES
    ('AG-LPT-001', 'SN-DELL-78432',  'Dell Latitude 5540',     'Laptop',     'Dell',    'Latitude 5540',  '2024-01-15', '2027-01-15', 'Assigned',   'HQ Floor 3',  'Primary dev laptop'),
    ('AG-LPT-002', 'SN-HP-91234',    'HP EliteBook 840',       'Laptop',     'HP',      'EliteBook 840','2024-03-20', '2027-03-20', 'Available',  'IT Storage',  NULL),
    ('AG-LPT-003', 'SN-LEN-45678',   'Lenovo ThinkPad X1',     'Laptop',     'Lenovo',  'ThinkPad X1',  '2023-11-10', '2026-11-10', 'Assigned',   'Remote',      NULL),
    ('AG-DSP-001', 'SN-DELL-M7821',  'Dell UltraSharp 27',     'Monitor',    'Dell',    'U2723QE',      '2024-02-01', '2027-02-01', 'Available',  'HQ Floor 3',  NULL),
    ('AG-DSP-002', 'SN-LG-33445',    'LG 34 UltraWide',        'Monitor',    'LG',      '34WP65C',      '2024-04-15', '2027-04-15', 'Assigned',   'HQ Floor 2',  NULL),
    ('AG-PHN-001', 'SN-IPH-99887',   'iPhone 15 Pro',          'Phone',      'Apple',   'iPhone 15 Pro','2024-06-01', '2026-06-01', 'Assigned',   'Mobile',      'Company phone'),
    ('AG-PHN-002', 'SN-SAM-77665',   'Samsung Galaxy S24',     'Phone',      'Samsung', 'Galaxy S24',   '2024-07-10', '2026-07-10', 'Available',  'IT Storage',  NULL),
    ('AG-PRT-001', 'SN-HP-P1234',    'HP LaserJet Pro',        'Printer',    'HP',      'M404dn',       '2023-08-20', '2026-08-20', 'Available',  'HQ Floor 1',  'Shared printer'),
    ('AG-SRV-001', 'SN-DELL-R740',   'Dell PowerEdge R740',    'Server',     'Dell',    'PowerEdge R740','2022-05-01','2025-05-01', 'In Maintenance','Data Center','Production server'),
    ('AG-TBL-001', 'SN-IPAD-5544',   'iPad Pro 12.9',          'Tablet',     'Apple',   'iPad Pro',     '2024-09-01', '2026-09-01', 'Available',  'IT Storage',  NULL),
    ('AG-NET-001', 'SN-CS-8877',     'Cisco Catalyst Switch',  'Network',    'Cisco',   'C9300-48P',    '2023-03-15', '2026-03-15', 'Available',  'Data Center', NULL),
    ('AG-LPT-004', 'SN-MAC-2233',    'MacBook Pro 14',         'Laptop',     'Apple',   'MacBook Pro',  '2024-10-01', '2027-10-01', 'Retired',    'Archive',     'End of life');
END
GO

IF NOT EXISTS (SELECT 1 FROM Assignments)
BEGIN
    INSERT INTO Assignments (DeviceId, EmployeeId, AssignedDate, ReturnDate, Status, AssignmentNote) VALUES
    (1, 1, '2024-02-01', NULL, 'Active', 'Assigned for IT management duties'),
    (3, 5, '2024-01-05', NULL, 'Active', 'Engineering team laptop'),
    (5, 4, '2024-05-01', NULL, 'Active', 'Marketing workstation monitor'),
    (6, 6, '2024-06-15', NULL, 'Active', 'Sales team mobile device');
END
GO

IF NOT EXISTS (SELECT 1 FROM MaintenanceRecords)
BEGIN
    INSERT INTO MaintenanceRecords (DeviceId, MaintenanceDate, MaintenanceType, Description, Cost, PerformedBy, NextMaintenanceDate) VALUES
    (1,  '2024-06-01', 'Preventive',  'Annual hardware inspection and cleaning', 75.00,  'IT Support', '2025-06-01'),
    (9,  '2025-01-10', 'Corrective',  'Replaced failed RAID controller',         450.00, 'Dell Support', '2025-07-10'),
    (8,  '2024-09-15', 'Preventive',  'Toner replacement and calibration',       120.00, 'IT Support', '2025-03-15'),
    (3,  '2024-08-20', 'Upgrade',     'RAM upgraded from 16GB to 32GB',          180.00, 'IT Support', NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM Warranties)
BEGIN
    INSERT INTO Warranties (DeviceId, Provider, StartDate, EndDate, CoverageDetails, IsActive) VALUES
    (1,  'Dell ProSupport',    '2024-01-15', '2027-01-15', 'On-site next business day', 1),
    (2,  'HP Care Pack',       '2024-03-20', '2027-03-20', '3-year hardware warranty',  1),
    (9,  'Dell ProSupport Plus','2022-05-01','2025-05-01', '24/7 critical support',     1),
    (6,  'AppleCare+',         '2024-06-01', '2026-06-01', 'Accidental damage coverage',1),
    (12, 'AppleCare',          '2024-10-01', '2027-10-01', 'Standard warranty',          0);
END
GO

IF NOT EXISTS (SELECT 1 FROM AuditLogs)
BEGIN
    INSERT INTO AuditLogs (UserName, ActionType, EntityName, EntityId, Description, CreatedAt) VALUES
    ('admin', 'Create', 'Device',     1,  'Created device AG-LPT-001',           DATEADD(DAY, -30, GETUTCDATE())),
    ('admin', 'Create', 'Employee',   1,  'Created employee John Smith',         DATEADD(DAY, -28, GETUTCDATE())),
    ('admin', 'Assign', 'Assignment', 1,  'Assigned AG-LPT-001 to John Smith',   DATEADD(DAY, -25, GETUTCDATE())),
    ('admin', 'Update', 'Device',     9,  'Updated device status to In Maintenance', DATEADD(DAY, -5, GETUTCDATE())),
    ('admin', 'Create', 'MaintenanceRecord', 2, 'Logged maintenance for server AG-SRV-001', DATEADD(DAY, -3, GETUTCDATE()));
END
GO
