# https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide
resource "azurerm_linux_function_app" "github_crawler" {
  name                       = "func-${var.application_name}-${var.environment_name}-github-crawler"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  service_plan_id            = azurerm_service_plan.consumption.id
  storage_account_name       = azurerm_storage_account.function.name
  storage_account_access_key = azurerm_storage_account.function.primary_access_key

  site_config {
    application_stack {
      dotnet_version = "6.0"
    }
    cors {
      allowed_origins     = ["https://portal.azure.com"]
      support_credentials = true
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE"       = 1
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.main.instrumentation_key
    "GITHUB_PAT_TOKEN"               = var.github_pat_token
    "STORAGE_CONNECTION_STRING"      = azurerm_storage_account.github_crawler.primary_connection_string
    "QUEUE_CONNECTION_STRING"        = azurerm_storage_account.queue.primary_connection_string
  }

  identity {
    type         = "SystemAssigned, UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.function.id]
  }
}

resource "azurerm_storage_account" "github_crawler" {
  name                     = "st${var.application_name}${var.environment_name}ghc"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_container" "github_crawler_pull_requests" {
  name                  = "pull-requests"
  storage_account_name  = azurerm_storage_account.github_crawler.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "github_crawler_users" {
  name                  = "users"
  storage_account_name  = azurerm_storage_account.github_crawler.name
  container_access_type = "private"
}

resource "azurerm_storage_container" "github_crawler_companies" {
  name                  = "companies"
  storage_account_name  = azurerm_storage_account.github_crawler.name
  container_access_type = "private"
}

resource "azurerm_storage_blob" "spacelift" {
  name                   = "spacelift.txt"
  storage_account_name   = azurerm_storage_account.github_crawler.name
  storage_container_name = azurerm_storage_container.github_crawler_companies.name
  type                   = "Block"
  source                 = "./files/spacelift.txt"
}