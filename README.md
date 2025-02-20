# service-tester

Build to deploy on linux with: `dotnet publish -c Release -r linux-x64 --self-contained
`

Usage: `Usage: service-tester <service_type> <connection_string> [--key-filter <key_filter>] [--label-filter <label_filter>]`

## Notes

* Command "appconfig" = Azure App Configuration
* `--key-filter` and `--label-filter` are optional, and used only for Azure App Configuration tests. When specified, returns/outputs the values of the app configs that were found.
* Connection string should be enclosed in double-quotes
* Key vault connection string should be in the format: "https://<keyvault-name>.vault.azure.net"
* Key vault will prompt you to login with URL in a separate browser to Azure so that it can authenticate your user against vault