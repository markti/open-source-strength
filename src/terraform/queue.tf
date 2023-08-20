
resource "azurerm_storage_account" "queue" {
  name                     = "st${var.application_name}${var.environment_name}q"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_storage_queue" "github_repo_pull_request" {
  name                 = "pull-request-page"
  storage_account_name = azurerm_storage_account.queue.name
}

resource "azurerm_storage_queue" "github_user" {
  name                 = "github-user"
  storage_account_name = azurerm_storage_account.queue.name
}

resource "azurerm_storage_queue" "github_user_provider" {
  name                 = "github-user-provider"
  storage_account_name = azurerm_storage_account.queue.name
}