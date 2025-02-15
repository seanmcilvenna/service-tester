# service-tester

Build to deploy on linux with: `dotnet publish -c Release -r linux-x64 --self-contained
`

Usage: `service-tester <sql | mongo | redis | appconfig | keyvault> "<connection-string>"`

## Notes

* Command "appconfig" = Azure App Configuration
* Connection string should be enclosed in double-quotes
* Key vault connection string should be in the format: "https://<keyvault-name>.vault.azure.net"
* Key vault will prompt you to login with URL in a separate browser to Azure so that it can authenticate your user against vault

## ☕ Support My Work

Find this useful and want to show appreciation??  

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20My%20Work-orange?style=flat&logo=buy-me-a-coffee)](https://buymeacoffee.com/seanmcilvenna)

Your support helps keep this project alive and motivates me to continue improving it! 🚀