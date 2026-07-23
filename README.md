# service-tester

Build to deploy on linux with: `dotnet publish -c Release -r linux-x64 --self-contained
`

Usage: `service-tester <sql | mongo | redis | appconfig | keyvault | blob-storage | kafka-rest | kafka-broker> [options] "<connection-string> | <url> | <broker>"`

## Supported Services

* **SQL Server**
* **MongoDB**
* **Redis**
* **Azure App Configuration**
* **Azure Key Vault**
* **Azure Blob Storage / Storage Account**
* **Kafka REST Proxy**
* **Kafka Broker**

## Commands

* `sql`: Test SQL Server connection
* `mongo`: Test MongoDB connection
* `redis`: Test Redis connection
* `appconfig`: Test Azure App Configuration connection
* `keyvault`: Test Azure Key Vault connection
* `blob-storage`: Test Azure Storage account connectivity (blob container, optional resource listing)
* `kafka-rest`: Test Kafka REST API connection
* `kafka-broker`: Test Kafka Broker connection

## Options

### General Options (SQL, Mongo, Redis, AppConfig, KeyVault, Blob Storage)
* `-m, --managed-identity`: Use Azure Managed Identity for authentication.

### Blob Storage Options
* `-c, --container`: Specific blob container to test connectivity for.
* `-f, --file-share`: Specific file share to test connectivity for.
* `-l, --list`: List accessible blob containers, file shares, and queues.

#### Notes

* With `--managed-identity`, provide a Blob service URI like: `https://<account-name>.blob.core.windows.net`
* Without `--managed-identity`, use an Azure Storage connection string in key/value format (not just `https://...`), for example: `service-tester blob-storage "DefaultEndpointsProtocol=https;AccountName=XXX;AccountKey=YYY;EndpointSuffix=core.windows.net" --list`

### Kafka REST API Options
* `-u, --username`: Username for Basic Authentication.
* `-p, --password`: Password for Basic Authentication.

### Kafka Broker Options
* `-s, --security-protocol`: Security protocol (Plaintext, Ssl, SaslPlaintext, SaslSsl).
* `-m, --sasl-mechanism`: SASL mechanism (Plain, ScramSha256, ScramSha512, Gssapi, OAuthBearer).
* `-u, --username`: SASL username.
* `-p, --password`: SASL password.

## Notes

* Command "appconfig" = Azure App Configuration
* Connection string / URL / Broker address should be enclosed in double-quotes if it contains special characters.
* Key vault connection string should be in the format: `https://<keyvault-name>.vault.azure.net`
* When not using Managed Identity, Key Vault and SQL Server may prompt for interactive login or require specific authentication details in the connection string.

## ☕ Support My Work

Find this useful and want to show appreciation??  

[![Buy Me a Coffee](https://img.shields.io/badge/Buy%20Me%20a%20Coffee-Support%20My%20Work-orange?style=flat&logo=buy-me-a-coffee)](https://buymeacoffee.com/seanmcilvenna)

Your support helps keep this project alive and motivates me to continue improving it! 🚀