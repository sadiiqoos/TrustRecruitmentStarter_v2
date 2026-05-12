#!/bin/bash
set -euo pipefail

# ── Variabler — anpassa dessa ──────────────────────────────────────
RESOURCE_GROUP="trustrecruitment-rg"
LOCATION="swedencentral"
ACR_NAME="trustrecruitmntacr"
ENVIRONMENT="trustrecruitment-env"
APP_NAME="trustrecruitment-app"
GITHUB_USERNAME="sadiiqoos"
GITHUB_REPO="TrustRecruitmentStarter_v2"
MONGO_CONNECTION_STRING="mongodb+srv://trustadmin:<db_password>@cluster0.myxzluc.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0"
# ──────────────────────────────────────────────────────────────────

echo "→ Skapar resursgrupp..."
az group create --name "$RESOURCE_GROUP" --location "$LOCATION"

echo "→ Skapar Azure Container Registry..."
az acr create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$ACR_NAME" \
  --sku Basic \
  --admin-enabled false

echo "→ Skapar Container Apps-miljö..."
az containerapp env create \
  --name "$ENVIRONMENT" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION"

echo "→ Skapar Container App..."
az containerapp create \
  --name "$APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --environment "$ENVIRONMENT" \
  --image "mcr.microsoft.com/dotnet/samples:aspnetapp" \
  --target-port 8080 \
  --ingress external \
  --registry-server "$ACR_NAME.azurecr.io" \
  --secrets "mongodb-connection=$MONGO_CONNECTION_STRING" \
  --env-vars \
      "ASPNETCORE_ENVIRONMENT=Production" \
      "ConnectionStrings__MongoDb=secretref:mongodb-connection"

echo "→ Skapar managed identity för GitHub Actions..."
APP_ID=$(az ad app create --display-name "trustrecruitment-github" --query appId -o tsv)
az ad sp create --id "$APP_ID"
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

echo "→ Skapar federerad credential för GitHub Actions..."
az ad app federated-credential create \
  --id "$APP_ID" \
  --parameters "{
    \"name\": \"github-oidc\",
    \"issuer\": \"https://token.actions.githubusercontent.com\",
    \"subject\": \"repo:$GITHUB_USERNAME/$GITHUB_REPO:ref:refs/heads/main\",
    \"audiences\": [\"api://AzureADTokenExchange\"]
  }"

echo "→ Tilldelar roller..."
ACR_ID=$(az acr show --name "$ACR_NAME" --query id -o tsv)
az role assignment create --assignee "$APP_ID" --role AcrPush --scope "$ACR_ID"
az role assignment create --assignee "$APP_ID" \
  --role Contributor \
  --scope "/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP"

echo ""
echo "✓ Klart! Lägg till dessa GitHub Secrets:"
echo "  AZURE_CLIENT_ID       = $APP_ID"
echo "  AZURE_TENANT_ID       = $(az account show --query tenantId -o tsv)"
echo "  AZURE_SUBSCRIPTION_ID = $SUBSCRIPTION_ID"
echo "  AZURE_RESOURCE_GROUP  = $RESOURCE_GROUP"
echo "  ACR_NAME              = $ACR_NAME"
echo "  ACR_LOGIN_SERVER      = $ACR_NAME.azurecr.io"