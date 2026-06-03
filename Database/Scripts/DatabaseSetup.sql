/*
    AssetGuard — LocalDB Database Setup
    ====================================

    Prerequisites:
    - SQL Server LocalDB (installed with Visual Studio or SQL Server Express)
    - sqlcmd (optional, included with SQL Server tools)

    Step 1 — Start LocalDB instance
    --------------------------------
    sqllocaldb start MSSQLLocalDB
    sqllocaldb info MSSQLLocalDB

    Step 2 — Run this script
    ------------------------
    From the repo root (Asset-Guard):

    sqlcmd -S "(localdb)\MSSQLLocalDB" -I -i Database\Scripts\DatabaseSetup.sql

    Or run the automated setup script from repo root:

    .\Database\Setup-LocalDb.ps1

    Step 3 — Run the app
    --------------------
    dotnet run --project src/AssetGuard.Web

    Default login: admin / Admin@123
*/

:r Database\Scripts\AssetGuard_Complete.sql
