#!/bin/bash

# BlazorCrudDemo Database Management Script
# This script provides common database operations for the BlazorCrudDemo application

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Project paths
WEB_PROJECT_PATH="BlazorCrudDemo.Web"
DATA_PROJECT_PATH="../BlazorCrudDemo.Data"
STARTUP_PROJECT="$WEB_PROJECT_PATH"

echo_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

echo_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

echo_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

echo_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if we're in the right directory
check_directory() {
    if [ ! -d "$WEB_PROJECT_PATH" ]; then
        echo_error "Please run this script from the BlazorCrudDemo root directory"
        exit 1
    fi
}

# Function to create initial migration
create_initial_migration() {
    echo_info "Creating initial migration..."
    cd "$WEB_PROJECT_PATH"
    dotnet ef migrations add InitialCreate --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    echo_success "Initial migration created"
}

# Function to update database
update_database() {
    echo_info "Applying migrations to database..."
    cd "$WEB_PROJECT_PATH"
    dotnet ef database update --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    echo_success "Database updated"
}

# Function to create new migration
create_migration() {
    if [ -z "$1" ]; then
        echo_error "Please provide a migration name"
        echo "Usage: $0 migrate <migration_name>"
        exit 1
    fi

    echo_info "Creating migration: $1"
    cd "$WEB_PROJECT_PATH"
    dotnet ef migrations add "$1" --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
    echo_success "Migration '$1' created"
}

# Function to remove last migration
remove_migration() {
    echo_warning "This will remove the last migration. Continue? (y/N)"
    read -r response
    if [[ "$response" =~ ^([yY][eE][sS]|[yY]|[sS][iI]|[oO][uU][iI])$ ]]; then
        echo_info "Removing last migration..."
        cd "$WEB_PROJECT_PATH"
        dotnet ef migrations remove --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
        echo_success "Last migration removed"
    else
        echo_info "Operation cancelled"
    fi
}

# Function to list migrations
list_migrations() {
    echo_info "Listing applied migrations..."
    cd "$WEB_PROJECT_PATH"
    dotnet ef migrations list --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj"
}

# Function to generate SQL script
generate_script() {
    echo_info "Generating SQL script..."
    cd "$WEB_PROJECT_PATH"
    dotnet ef migrations script --project "$DATA_PROJECT_PATH/BlazorCrudDemo.Data.csproj" --startup-project "BlazorCrudDemo.Web.csproj" --output "migration-script.sql"
    echo_success "SQL script generated: migration-script.sql"
}

# Function to reset database
reset_database() {
    echo_warning "This will delete the database and recreate it. Continue? (y/N)"
    read -r response
    if [[ "$response" =~ ^([yY][eE][sS]|[yY]|[sS][iI]|[oO][uU][iI])$ ]]; then
        echo_info "Resetting database..."

        # Delete database file if it exists
        if [ -f "$WEB_PROJECT_PATH/blazorcrud.db" ]; then
            rm "$WEB_PROJECT_PATH/blazorcrud.db"
            echo_info "Database file deleted"
        fi

        # Update database to recreate
        update_database
    else
        echo_info "Operation cancelled"
    fi
}

# Function to show database info
show_info() {
    echo_info "Database Information:"
    echo "  Database File: $WEB_PROJECT_PATH/blazorcrud.db"
    echo "  Connection String: Data Source=blazorcrud.db"

    if [ -f "$WEB_PROJECT_PATH/blazorcrud.db" ]; then
        echo "  File Size: $(du -h "$WEB_PROJECT_PATH/blazorcrud.db" | cut -f1)"
        echo "  Last Modified: $(stat -c %y "$WEB_PROJECT_PATH/blazorcrud.db" 2>/dev/null || stat -f %Sm "$WEB_PROJECT_PATH/blazorcrud.db" 2>/dev/null || echo "Unknown")"
    fi
}

# Function to build projects
build_projects() {
    echo_info "Building projects..."
    dotnet build
    echo_success "Projects built successfully"
}

# Main menu
show_menu() {
    echo -e "${BLUE}"
    echo "======================================"
    echo "  BlazorCrudDemo Database Manager"
    echo "======================================"
    echo -e "${NC}"
    echo "1. Build Projects"
    echo "2. Create Initial Migration"
    echo "3. Update Database"
    echo "4. Create New Migration"
    echo "5. Remove Last Migration"
    echo "6. List Migrations"
    echo "7. Generate SQL Script"
    echo "8. Reset Database"
    echo "9. Show Database Info"
    echo "0. Exit"
    echo ""
    echo -n "Enter your choice: "
}

# Main script logic
main() {
    check_directory

    while true; do
        show_menu
        read -r choice

        case $choice in
            1)
                build_projects
                ;;
            2)
                create_initial_migration
                ;;
            3)
                update_database
                ;;
            4)
                echo -n "Enter migration name: "
                read -r migration_name
                create_migration "$migration_name"
                ;;
            5)
                remove_migration
                ;;
            6)
                list_migrations
                ;;
            7)
                generate_script
                ;;
            8)
                reset_database
                ;;
            9)
                show_info
                ;;
            0)
                echo_info "Goodbye!"
                exit 0
                ;;
            *)
                echo_error "Invalid option. Please try again."
                ;;
        esac

        echo ""
        echo -n "Press Enter to continue..."
        read -r
        echo ""
    done
}

# Run the main function
main "$@"
