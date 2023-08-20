variable "application_name" {
  type = string
}
variable "environment_name" {
  type = string
}
variable "location" {
  type = string
}
variable "github_pat_token" {
  type      = string
  sensitive = true
}