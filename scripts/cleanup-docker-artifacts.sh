#!/bin/bash

# ============================================
# Docker Build Artifacts Cleanup Script
# ============================================
# This script removes duplicate directories that may be created
# during the Docker build process.
#
# Usage: ./scripts/cleanup-docker-artifacts.sh
# ============================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo -e "${YELLOW}🧹 Cleaning up Docker build artifacts...${NC}"

# Array of directories to clean
CLEANUP_DIRS=(
    "$PROJECT_ROOT/backend/backend"
    "$PROJECT_ROOT/backend/frontend"
)

# Counter for cleaned directories
CLEANED_COUNT=0

# Clean each directory
for dir in "${CLEANUP_DIRS[@]}"; do
    if [ -d "$dir" ]; then
        echo -e "${YELLOW}  Removing: $dir${NC}"
        rm -rf "$dir"
        ((CLEANED_COUNT++))
        echo -e "${GREEN}  ✓ Removed${NC}"
    fi
done

if [ $CLEANED_COUNT -gt 0 ]; then
    echo -e "${GREEN}✓ Cleanup complete! Removed $CLEANED_COUNT artifact(s)${NC}"
else
    echo -e "${GREEN}✓ No artifacts to clean${NC}"
fi

exit 0

