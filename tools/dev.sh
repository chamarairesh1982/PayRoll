#!/bin/bash
# Sri Lanka Payroll Platform - Development Script (Bash)
# Cross-platform development automation for Linux/macOS

set -e

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
COMMAND="${1:-help}"
TARGET="${2:-}"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Output functions
success() { echo -e "${GREEN}$1${NC}"; }
info() { echo -e "${CYAN}$1${NC}"; }
warning() { echo -e "${YELLOW}$1${NC}"; }
error() { echo -e "${RED}$1${NC}"; }

# Header
show_header() {
    echo ""
    echo -e "${CYAN}╔════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║   Sri Lanka Payroll Platform - Development Tool       ║${NC}"
    echo -e "${CYAN}╚════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

# Check prerequisites
check_prerequisites() {
    info "Checking prerequisites..."
    
    # Check .NET SDK
    if ! command -v dotnet &> /dev/null; then
        error "❌ .NET SDK not found. Please install .NET 8 SDK."
        exit 1
    fi
    DOTNET_VERSION=$(dotnet --version)
    success "✓ .NET SDK: $DOTNET_VERSION"
    
    # Check Node.js
    if ! command -v node &> /dev/null; then
        error "❌ Node.js not found. Please install Node.js 20+."
        exit 1
    fi
    NODE_VERSION=$(node --version)
    success "✓ Node.js: $NODE_VERSION"
    
    # Check npm
    if ! command -v npm &> /dev/null; then
        error "❌ npm not found. Please install npm."
        exit 1
    fi
    NPM_VERSION=$(npm --version)
    success "✓ npm: $NPM_VERSION"
    
    echo ""
}

# Bootstrap - First time setup
bootstrap() {
    info "🚀 Bootstrapping development environment..."
    echo ""
    
    check_prerequisites
    
    # Restore backend dependencies
    info "📦 Restoring .NET dependencies..."
    cd "$ROOT_DIR"
    dotnet restore PayrollSolution.sln
    success "✓ .NET dependencies restored"
    echo ""
    
    # Restore frontend dependencies
    info "📦 Installing npm packages..."
    cd "$ROOT_DIR/frontend/payroll-web"
    npm install
    success "✓ npm packages installed"
    echo ""
    
    # Restore E2E test dependencies
    info "📦 Installing Playwright..."
    cd "$ROOT_DIR/qa-automation"
    npm install
    npx playwright install
    success "✓ Playwright installed"
    echo ""
    
    # Database setup
    info "🗄️  Setting up database..."
    cd "$ROOT_DIR/backend/Payroll.Api"
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    success "✓ Database created and migrated"
    echo ""
    
    # Seed data
    info "🌱 Seeding initial data..."
    cd "$ROOT_DIR/backend/Payroll.Seeder"
    dotnet run
    success "✓ Data seeded"
    echo ""
    
    success "✅ Bootstrap complete! Run './tools/dev.sh run' to start development."
}

# Build all projects
build() {
    info "🔨 Building all projects..."
    echo ""
    
    # Build backend
    info "Building backend..."
    cd "$ROOT_DIR"
    dotnet build PayrollSolution.sln --configuration Debug
    success "✓ Backend built"
    echo ""
    
    # Build frontend
    info "Building frontend..."
    cd "$ROOT_DIR/frontend/payroll-web"
    npm run build
    success "✓ Frontend built"
    echo ""
    
    success "✅ Build complete!"
}

# Run tests
test() {
    local TEST_TARGET="${1:-all}"
    
    info "🧪 Running tests..."
    echo ""
    
    if [[ "$TEST_TARGET" == "all" || "$TEST_TARGET" == "backend" ]]; then
        info "Running backend tests..."
        cd "$ROOT_DIR/backend/Payroll.Application.Tests"
        dotnet test --logger "console;verbosity=normal"
        success "✓ Backend tests passed"
        echo ""
    fi
    
    if [[ "$TEST_TARGET" == "all" || "$TEST_TARGET" == "frontend" ]]; then
        info "Running frontend tests..."
        cd "$ROOT_DIR/frontend/payroll-web"
        npm run test:ci
        success "✓ Frontend tests passed"
        echo ""
    fi
    
    if [[ "$TEST_TARGET" == "all" || "$TEST_TARGET" == "e2e" ]]; then
        info "Running E2E tests..."
        cd "$ROOT_DIR/qa-automation"
        npx playwright test
        success "✓ E2E tests passed"
        echo ""
    fi
    
    success "✅ All tests passed!"
}

# Lint and format code
lint() {
    local CHECK_ONLY=false
    if [[ "$1" == "--check" ]]; then
        CHECK_ONLY=true
    fi
    
    info "🎨 Linting code..."
    echo ""
    
    # Lint backend
    info "Linting backend..."
    cd "$ROOT_DIR"
    if $CHECK_ONLY; then
        dotnet format PayrollSolution.sln --verify-no-changes
    else
        dotnet format PayrollSolution.sln
    fi
    success "✓ Backend linted"
    echo ""
    
    # Lint frontend
    info "Linting frontend..."
    cd "$ROOT_DIR/frontend/payroll-web"
    if $CHECK_ONLY; then
        npm run lint
    else
        npm run lint:fix
    fi
    success "✓ Frontend linted"
    echo ""
    
    success "✅ Linting complete!"
}

# Run development servers
run() {
    local RUN_TARGET="${1:-all}"
    
    info "🚀 Starting development servers..."
    echo ""
    
    if [[ "$RUN_TARGET" == "backend" ]]; then
        info "Starting backend API on http://localhost:52938..."
        cd "$ROOT_DIR/backend/Payroll.Api"
        dotnet run --urls http://localhost:52938
    elif [[ "$RUN_TARGET" == "frontend" ]]; then
        info "Starting frontend on http://localhost:4200..."
        cd "$ROOT_DIR/frontend/payroll-web"
        npm start
    else
        info "Starting both backend and frontend..."
        info "Backend: http://localhost:52938"
        info "Frontend: http://localhost:4200"
        echo ""
        warning "Note: Run backend and frontend in separate terminals:"
        echo "  Terminal 1: ./tools/dev.sh run backend"
        echo "  Terminal 2: ./tools/dev.sh run frontend"
    fi
}

# Clean build artifacts
clean() {
    info "🧹 Cleaning build artifacts..."
    echo ""
    
    # Clean backend
    info "Cleaning backend..."
    cd "$ROOT_DIR"
    dotnet clean PayrollSolution.sln
    success "✓ Backend cleaned"
    echo ""
    
    # Clean frontend
    info "Cleaning frontend..."
    cd "$ROOT_DIR/frontend/payroll-web"
    rm -rf dist .angular
    success "✓ Frontend cleaned"
    echo ""
    
    success "✅ Clean complete!"
}

# Database reset
db_reset() {
    warning "⚠️  This will DROP and recreate the database!"
    read -p "Are you sure? (yes/no): " confirm
    
    if [[ "$confirm" != "yes" ]]; then
        info "Cancelled."
        return
    fi
    
    info "🗄️  Resetting database..."
    cd "$ROOT_DIR/backend/Payroll.Api"
    
    # Drop database
    dotnet ef database drop --project ../Payroll.Infrastructure --startup-project . --force
    
    # Recreate and migrate
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    
    success "✓ Database reset"
    echo ""
    
    # Seed data
    info "🌱 Seeding data..."
    cd "$ROOT_DIR/backend/Payroll.Seeder"
    dotnet run
    success "✓ Data seeded"
    echo ""
    
    success "✅ Database reset complete!"
}

# Create database migration
db_migrate() {
    local MIGRATION_NAME="$1"
    
    if [[ -z "$MIGRATION_NAME" ]]; then
        error "❌ Migration name is required. Usage: ./tools/dev.sh db-migrate 'MigrationName'"
        exit 1
    fi
    
    info "📝 Creating migration: $MIGRATION_NAME..."
    cd "$ROOT_DIR/backend/Payroll.Api"
    dotnet ef migrations add "$MIGRATION_NAME" --project ../Payroll.Infrastructure --startup-project .
    success "✓ Migration created"
    echo ""
    
    info "To apply migration, run: ./tools/dev.sh db-update"
}

# Apply database migrations
db_update() {
    info "🗄️  Applying database migrations..."
    cd "$ROOT_DIR/backend/Payroll.Api"
    dotnet ef database update --project ../Payroll.Infrastructure --startup-project .
    success "✓ Migrations applied"
}

# Show help
show_help() {
    echo -e "${YELLOW}Usage: ./tools/dev.sh <command> [options]${NC}"
    echo ""
    echo -e "${CYAN}Commands:${NC}"
    echo "  bootstrap          First-time setup (install deps, create DB, seed data)"
    echo "  build              Build all projects"
    echo "  test [target]      Run tests (all|backend|frontend|e2e)"
    echo "  lint [--check]     Lint and format code"
    echo "  run [target]       Start dev servers (all|backend|frontend)"
    echo "  clean              Remove build artifacts"
    echo "  db-reset           Drop and recreate database"
    echo "  db-migrate <name>  Create new migration"
    echo "  db-update          Apply pending migrations"
    echo "  help               Show this help"
    echo ""
    echo -e "${CYAN}Examples:${NC}"
    echo "  ./tools/dev.sh bootstrap"
    echo "  ./tools/dev.sh build"
    echo "  ./tools/dev.sh test backend"
    echo "  ./tools/dev.sh lint"
    echo "  ./tools/dev.sh run backend"
    echo "  ./tools/dev.sh db-migrate 'AddBonusTable'"
    echo ""
}

# Main execution
show_header

case "$COMMAND" in
    bootstrap)
        bootstrap
        ;;
    build)
        build
        ;;
    test)
        test "$TARGET"
        ;;
    lint)
        lint "$TARGET"
        ;;
    run)
        run "$TARGET"
        ;;
    clean)
        clean
        ;;
    db-reset)
        db_reset
        ;;
    db-migrate)
        db_migrate "$TARGET"
        ;;
    db-update)
        db_update
        ;;
    help)
        show_help
        ;;
    *)
        error "Unknown command: $COMMAND"
        show_help
        exit 1
        ;;
esac
