variable "azure_tenant_id" {
  type      = string
  sensitive = true
}

variable "azure_client_id" {
  type      = string
  sensitive = true
}

terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">=3.7.0"
    }
  }
  backend "azurerm" {
    resource_group_name  = "terraform-backend"
    storage_account_name = "seatpickerterraformstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "resourceGroup" {
  name     = "testy"
  location = "Norway East"
}

resource "random_string" "random" {
  length  = 8
  special = false
  upper   = false
  keepers = [locals.rgName]
}

locals {
  rgName   = azurerm_resource_group.resourceGroup.name
  location = azurerm_resource_group.resourceGroup.location
  postfix  = random_string.random.result
}

resource "azurerm_log_analytics_workspace" "logAnalytics" {
  name                = "loganalytics-${local.postfix}"
  location            = local.location
  resource_group_name = local.rgName
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "appInsights" {
  name                = "appins-${local.postfix}"
  location            = local.location
  resource_group_name = local.rgName
  workspace_id        = azurerm_log_analytics_workspace.logAnalytics.id
  application_type    = "web"
}

resource "azurerm_app_service_plan" "appServicePlan" {
  name                = "appserviceplan-${local.postfix}"
  location            = local.location
  resource_group_name = local.rgName

  sku {
    tier = "Standard"
    size = "S1"
  }
}

resource "azurerm_windows_web_app" "appService" {
  name                = "appservice-${local.postfix}"
  location            = local.location
  resource_group_name = local.rgName
  service_plan_id     = azurerm_app_service_plan.appServicePlan.id

  site_config {
    http2_enabled      = true
    always_on          = true
    websockets_enabled = true
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings,
      connection_string,
      site_config.virtual_application
    ]
  }
}

resource "azurerm_windows_web_app_slot" "stagingSlot" {
  name           = "staging"
  app_service_id = azurerm_windows_web_app.appService.id

  site_config {
    http2_enabled      = true
    websockets_enabled = true
    always_on          = true

    virtual_application {
      virtual_path  = "/"
      physical_path = "site\\wwwroot"
      preload       = true
    }

    virtual_application {
      virtual_path  = "/api"
      physical_path = "site\\wwwroot\\api"
      preload       = true
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

locals {
  kv_readers = [
    azurerm_windows_web_app.appService.identity.principal_id,
    azurerm_windows_web_app_slot.staging.identity.principal_id
  ]

  kv_officers = [
    var.azure_client_id,
    "8090cb15-f778-47c0-8b4d-8a1af3d0d2be" // Erik Nystø Rahka
  ]
}

resource "azurerm_key_vault" "keyvault" {
  name                        = "kv-${local.postfix}"
  location                    = local.location
  resource_group_name         = local.rgName
  enabled_for_disk_encryption = true
  tenant_id                   = var.azure_tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  enable_rbac_authorization   = true

  sku_name = "standard"
}

resource "azurerm_role_assignment" "keyvaultReaders" {
  for_each             = local.kv_readers
  scope                = azurerm_key_vault.keyvault.id
  role_definition_name = "Key Vault Secrets User "
  principal_id         = each.key
}

resource "azurerm_role_assignment" "keyvaultSecretOfficers" {
  for_each             = local.kv_officers
  scope                = azurerm_key_vault.keyvault.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = each.key
}

resource "azurerm_role_assignment" "keyvaultCertificateOfficers" {
  for_each             = local.kv_officers
  scope                = azurerm_key_vault.keyvault.id
  role_definition_name = "Key Vault Certificates Officer"
  principal_id         = each.key
}

resource "random_password" "postgresPassword" {
  length           = 16
  special          = true
  override_special = "!#$%&*()-_=+[]{}<>:?"
}

resource "azurerm_postgresql_server" "postgres" {
  name                = "postgresql-${local.postfix}"
  location            = local.location
  resource_group_name = local.rgName

  sku_name = "B_Gen5_1"

  storage_mb                   = 32768
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false
  auto_grow_enabled            = true

  administrator_login          = "psqladmin"
  administrator_login_password = random_password.postgresPassword.result
  version                      = "15"
  ssl_enforcement_enabled      = true
}

resource "azurerm_postgresql_database" "productionDatabse" {
  name                = "production"
  resource_group_name = local.rgName
  server_name         = azurerm_postgresql_server.postgres.name
  charset             = "UTF8"
  collation           = "English_United States.1252"
}

resource "azurerm_key_vault_secret" "databasePasswordSecret" {
  name         = "DatabaseAdminPassword"
  value        = random_password.postgresPassword.result
  key_vault_id = azurerm_key_vault.keyvault.id
  depends_on   = [azurerm_role_assignment.keyvaultSecretOfficers]
}

resource "azurerm_key_vault_secret" "databasePasswordUsername" {
  name         = "DatabaseAdminUsername"
  value        = azurerm_postgresql_server.postgres.administrator_login
  key_vault_id = azurerm_key_vault.keyvault.id
  depends_on   = [azurerm_role_assignment.keyvaultSecretOfficers]
}
