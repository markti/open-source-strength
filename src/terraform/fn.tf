resource "azurerm_storage_account" "function" {
  name                     = "st${var.application_name}${random_string.name.result}fn"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

resource "azurerm_user_assigned_identity" "function" {
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  name                = "mi-${var.application_name}-${random_string.name.result}-fn"
}

resource "azurerm_service_plan" "consumption" {
  name                = "asp-${var.application_name}-${random_string.name.result}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "Y1"
}