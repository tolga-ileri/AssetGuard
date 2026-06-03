# AssetGuard LocalDB Setup Script
# Run from repo root: .\Database\Setup-LocalDb.ps1

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$SqlScript = Join-Path $RepoRoot "Database\Scripts\AssetGuard_Complete.sql"
$Server = "(localdb)\MSSQLLocalDB"
$Database = "AssetGuardDb"

Write-Host "=== AssetGuard LocalDB Setup ===" -ForegroundColor Cyan

# 1. LocalDB instance
Write-Host "`n[1/4] Checking LocalDB..."
$instances = sqllocaldb info 2>&1
if ($instances -notmatch "MSSQLLocalDB") {
    Write-Host "Creating MSSQLLocalDB instance..."
    sqllocaldb create MSSQLLocalDB | Out-Null
}
sqllocaldb start MSSQLLocalDB | Out-Null
$info = sqllocaldb info MSSQLLocalDB
if ($info -match "State:\s+Running") {
    Write-Host "  LocalDB running: OK" -ForegroundColor Green
} else {
    throw "LocalDB failed to start."
}

# 2. Execute SQL script
Write-Host "`n[2/4] Running database script..."
$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if ($sqlcmd) {
    & sqlcmd -S $Server -I -i $SqlScript -b
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed with exit code $LASTEXITCODE" }
} else {
    throw "sqlcmd not found. Install SQL Server Command Line Utilities, or run Database\Scripts\AssetGuard_Complete.sql in SSMS connected to (localdb)\MSSQLLocalDB."
}

# 3. Verify
Write-Host "`n[3/4] Verifying database..."
$verifyQuery = @"
SELECT CASE WHEN DB_ID('$Database') IS NOT NULL THEN 1 ELSE 0 END AS DbExists;
SELECT COUNT(*) AS TableCount FROM $Database.sys.tables;
SELECT CASE WHEN EXISTS(SELECT 1 FROM $Database.dbo.Users WHERE UserName='admin') THEN 1 ELSE 0 END AS AdminExists;
"@
$result = & sqlcmd -S $Server -Q $verifyQuery -W -h -1 2>&1
$lines = ($result | Where-Object { $_ -match '\S' })
$dbExists = $lines[0].Trim() -eq '1'
$tableCount = [int]$lines[1].Trim()
$adminExists = $lines[2].Trim() -eq '1'

Write-Host "  Database created: $(if ($dbExists) { 'OK' } else { 'FAILED' })" -ForegroundColor $(if ($dbExists) { 'Green' } else { 'Red' })
Write-Host "  Tables count: $tableCount"
Write-Host "  Admin user exists: $(if ($adminExists) { 'OK' } else { 'FAILED' })" -ForegroundColor $(if ($adminExists) { 'Green' } else { 'Red' })

if (-not ($dbExists -and $adminExists -and $tableCount -ge 7)) {
    throw "Verification failed."
}

Write-Host "`n[4/4] Setup complete." -ForegroundColor Green
Write-Host "Login: admin / Admin@123"
