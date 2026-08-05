#!/usr/bin/env bash
set -Eeuo pipefail

# ============================================================
# Deploy an existing .NET application to an existing
# Azure App Service using Azure CLI ZIP Deploy.
#
# Repository structure:
#   MsftFoundryMcpApplicationAccess/
#   ├── Books.Mcp/
#   │   └── Books.Mcp.csproj
#   └── deploy-app-service.sh
#
# Usage:
#   chmod +x deploy-app-service.sh
#   ./deploy-app-service.sh
# ============================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Required Azure parameters
SUBSCRIPTION_ID="<azure-subscription-id>"
RESOURCE_GROUP="<resource-group-name>"
WEB_APP_NAME="<existing-app-service-name>"

# Project path based on the current repository structure
PROJECT_PATH="${SCRIPT_DIR}/Books.Mcp/Books.Mcp.csproj"

# Local build settings
BUILD_CONFIGURATION="Release"
DEPLOY_DIR="${SCRIPT_DIR}/.deploy"
PUBLISH_DIR="${DEPLOY_DIR}/publish"
ZIP_PATH="${DEPLOY_DIR}/app.zip"

log() {
    printf '\n\033[1;34m==> %s\033[0m\n' "$1"
}

fail() {
    printf '\n\033[1;31mERROR: %s\033[0m\n' "$1" >&2
    exit 1
}

require_command() {
    command -v "$1" >/dev/null 2>&1 ||
        fail "Command '$1' was not found."
}

require_value() {
    local name="$1"
    local value="$2"

    if [[ -z "$value" || "$value" =~ ^\<.*\>$ ]]; then
        fail "Replace the placeholder for '$name'."
    fi
}

# ============================================================
# Validation
# ============================================================

require_command az
require_command dotnet
require_command zip

require_value "SUBSCRIPTION_ID" "$SUBSCRIPTION_ID"
require_value "RESOURCE_GROUP" "$RESOURCE_GROUP"
require_value "WEB_APP_NAME" "$WEB_APP_NAME"

[[ -f "$PROJECT_PATH" ]] ||
    fail "Project file not found: $PROJECT_PATH"

# ============================================================
# Azure authentication
# ============================================================

log "Authenticating with Azure CLI"

if ! az account show >/dev/null 2>&1; then
    az login
fi

az account set --subscription "$SUBSCRIPTION_ID"

log "Validating the existing App Service"

az webapp show \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_APP_NAME" \
    --output none \
    || fail "App Service '$WEB_APP_NAME' was not found in resource group '$RESOURCE_GROUP'."

# ============================================================
# Build and package
# ============================================================

log "Publishing the .NET application"

rm -rf "$DEPLOY_DIR"
mkdir -p "$PUBLISH_DIR"

dotnet restore "$PROJECT_PATH"

dotnet publish "$PROJECT_PATH" \
    --configuration "$BUILD_CONFIGURATION" \
    --output "$PUBLISH_DIR" \
    --no-restore

log "Creating deployment package"

(
    cd "$PUBLISH_DIR"
    zip -qr "$ZIP_PATH" .
)

# ============================================================
# Deployment
# ============================================================

log "Deploying to Azure App Service"

az webapp deploy \
    --resource-group "$RESOURCE_GROUP" \
    --name "$WEB_APP_NAME" \
    --src-path "$ZIP_PATH" \
    --type zip \
    --clean true \
    --restart true \
    --output none

APP_URL="https://${WEB_APP_NAME}.azurewebsites.net"

log "Deployment completed"
echo "Application: ${APP_URL}"
echo "MCP endpoint: ${APP_URL}/mcp"