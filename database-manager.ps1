# BlazorCrudDemo Database Management Script (PowerShell)
# This script provides common database operations for the BlazorCrudDemo application

param(
    [string]$Command = "menu",
    [string]$MigrationName = ""
)

# Colors for output
$Red = "Red"
$Green = "Green"
$Yellow = "Yellow"
$Blue = "Blue"
$NC = "White"

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor $Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor $Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor $Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor $Red
}

# Function to check if we're in the right directory
function Test-Directory {
    if (-not (Test-Path "BlazorCrudDemo.Web")) {
        Write-Error "Please run this script from the BlazorCrudDemo root directory"
        exit 1
    }
}

# Function to create initial migration
function New-InitialMigration {
    Write-Info "Creating initial migration..."
    Set-Location "BlazorCrudDemo.Web"
    dotnet ef migrations add InitialCreate --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    Set-Location ".."
    Write-Success "Initial migration created"
}

# Function to update database
function Update-Database {
    Write-Info "Applying migrations to database..."
    Set-Location "BlazorCrudDemo.Web"
    dotnet ef database update --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    Set-Location ".."
    Write-Success "Database updated"
}

# Function to create new migration
function New-Migration {
    param([string]$Name)

    if ([string]::IsNullOrEmpty($Name)) {
        Write-Error "Please provide a migration name"
        Write-Host "Usage: .\database-manager.ps1 migrate <migration_name>"
        exit 1
    }

    Write-Info "Creating migration: $Name"
    Set-Location "BlazorCrudDemo.Web"
    dotnet ef migrations add $Name --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    Set-Location ".."
    Write-Success "Migration '$Name' created"
}

# Function to remove last migration
function Remove-Migration {
    $response = Read-Host "This will remove the last migration. Continue? (y/N)"
    if ($response -match "^([yY][eE][sS]|[yY]|[sS][iI]|[oO][uU][iI])$") {
        Write-Info "Removing last migration..."
        Set-Location "BlazorCrudDemo.Web"
        dotnet ef migrations remove --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
        Set-Location ".."
        Write-Success "Last migration removed"
    } else {
        Write-Info "Operation cancelled"
    }
}

# Function to list migrations
function Get-Migrations {
    Write-Info "Listing applied migrations..."
    Set-Location "BlazorCrudDemo.Web"
    dotnet ef migrations list --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    Set-Location ".."
}

# Function to generate SQL script
function New-SqlScript {
    Write-Info "Generating SQL script..."
    Set-Location "BlazorCrudDemo.Web"
    dotnet ef migrations script --project "../BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj" --output "migration-script.sql"
    Set-Location ".."
    Write-Success "SQL script generated: migration-script.sql"
}

# Function to reset database
function Reset-Database {
    $response = Read-Host "This will delete the database and recreate it. Continue? (y/N)"
    if ($response -match "^([yY][eE][sS]|[yY]|[sS][iI]|[oO][uU][iI])$") {
        Write-Info "Resetting database..."

        # Delete database file if it exists
        $dbPath = "BlazorCrudDemo.Web/blazorcrud.db"
        if (Test-Path $dbPath) {
            Remove-Item $dbPath -Force
            Write-Info "Database file deleted"
        }

        # Update database to recreate
        Update-Database
    } else {
        Write-Info "Operation cancelled"
    }
}

# Function to show database info
function Get-DatabaseInfo {
    Write-Info "Database Information:"
    $dbPath = "BlazorCrudDemo.Web/blazorcrud.db"
    Write-Host "  Database File: $dbPath"
    Write-Host "  Connection String: Data Source=blazorcrud.db"

    if (Test-Path $dbPath) {
        $fileInfo = Get-Item $dbPath
        Write-Host "  File Size: $($fileInfo.Length) bytes"
        Write-Host "  Last Modified: $($fileInfo.LastWriteTime)"
    }
}

# Function to build projects
function Build-Projects {
    Write-Info "Building projects..."
    dotnet build
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Projects built successfully"
    } else {
        Write-Error "Build failed"
        exit 1
    }
}

# Function to show menu
function Show-Menu {
    Write-Host "======================================" -ForegroundColor $Blue
    Write-Host "  BlazorCrudDemo Database Manager" -ForegroundColor $Blue
    Write-Host "======================================" -ForegroundColor $Blue
    Write-Host ""
    Write-Host "1. Build Projects"
    Write-Host "2. Create Initial Migration"
    Write-Host "3. Update Database"
    Write-Host "4. Create New Migration"
    Write-Host "5. Remove Last Migration"
    Write-Host "6. List Migrations"
    Write-Host "7. Generate SQL Script"
    Write-Host "8. Reset Database"
    Write-Host "9. Show Database Info"
    Write-Host "0. Exit"
    Write-Host ""
    $choice = Read-Host "Enter your choice"
    return $choice
}

# Main script logic
function Main {
    Test-Directory

    switch ($Command) {
        "menu" {
            $choice = Show-Menu
            switch ($choice) {
                "1" { Build-Projects }
                "2" { New-InitialMigration }
                "3" { Update-Database }
                "4" {
                    if ([string]::IsNullOrEmpty($MigrationName)) {
                        $MigrationName = Read-Host "Enter migration name"
                    }
                    New-Migration $MigrationName
                }
                "5" { Remove-Migration }
                "6" { Get-Migrations }
                "7" { New-SqlScript }
                "8" { Reset-Database }
                "9" { Get-DatabaseInfo }
                "0" {
                    Write-Info "Goodbye!"
                    exit 0
                }
                default {
                    Write-Error "Invalid option. Please try again."
                }
            }
        }
        "build" { Build-Projects }
        "init" { New-InitialMigration }
        "update" { Update-Database }
        "migrate" {
            if ([string]::IsNullOrEmpty($MigrationName)) {
                Write-Error "Please provide a migration name"
                Write-Host "Usage: .\database-manager.ps1 migrate <migration_name>"
                exit 1
            }
            New-Migration $MigrationName
        }
        "remove" { Remove-Migration }
        "list" { Get-Migrations }
        "script" { New-SqlScript }
        "reset" { Reset-Database }
        "info" { Get-DatabaseInfo }
        default {
            Write-Error "Unknown command: $Command"
            Write-Host "Use 'menu' to see available options or run with specific command"
            exit 1
        }
    }
}

# Run the main function
Main
