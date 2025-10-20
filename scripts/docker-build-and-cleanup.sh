#!/bin/bash

# ============================================
# Docker Build with Automatic Cleanup
# ============================================
# This script builds Docker images using docker-compose
# and automatically cleans up build artifacts afterward.
#
# Usage: ./scripts/docker-build-and-cleanup.sh [options]
# Options:
#   --no-cache    Build without using cache
#   --help        Show this help message
# ============================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Parse arguments
BUILD_ARGS=""
if [[ "$1" == "--no-cache" ]]; then
    BUILD_ARGS="--no-cache"
    echo -e "${BLUE}Building without cache...${NC}"
elif [[ "$1" == "--help" ]]; then
    grep "^#" "$0" | grep -v "^#!/bin/bash" | sed 's/^# //'
    exit 0
fi

# Change to project root
cd "$PROJECT_ROOT"

echo -e "${BLUE}🐳 Building Docker images...${NC}"
if docker-compose build $BUILD_ARGS; then
    echo -e "${GREEN}✓ Docker build completed successfully${NC}"
else
    echo -e "${RED}✗ Docker build failed${NC}"
    exit 1
fi

echo ""
echo -e "${BLUE}🧹 Running cleanup...${NC}"
if bash "$SCRIPT_DIR/cleanup-docker-artifacts.sh"; then
    echo -e "${GREEN}✓ All done!${NC}"
else
    echo -e "${RED}✗ Cleanup failed${NC}"
    exit 1
fi

exit 0

