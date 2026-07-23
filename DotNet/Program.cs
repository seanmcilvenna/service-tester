using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using CommandLine;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using StackExchange.Redis;

namespace ServiceTester
{
    public interface IConnectionStringOptions
    {
        string ConnectionString { get; set; }
        IEnumerable<string> RemainingArguments { get; set; }
    }

    [Verb("sql", HelpText = "Test SQL Server connection")]
    public class SqlOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for SQL Server (i.e. \"Server=tcp:XXX,1433;Initial Catalog=YYY;User Id=objectId;Password=secret;Authentication='Active Directory Service Principal'\")")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("mongo", HelpText = "Test MongoDB connection")]
    public class MongoOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for MongoDB")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("redis", HelpText = "Test Redis connection")]
    public class RedisOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Redis")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("appconfig", HelpText = "Test Azure App Configuration connection")]
    public class AppConfigOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure App Configuration (i.e. \"https://resource.azconfig.io\")")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("keyvault", HelpText = "Test Azure Key Vault connection")]
    public class KeyVaultOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure Key Vault")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("blob-storage", HelpText = "Test Azure Storage account connection")]
    public class BlobStorageOptions : IConnectionStringOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure Storage, or Blob service URI when using --managed-identity (e.g. https://myaccount.blob.core.windows.net)")]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }

        [Option('c', "container", HelpText = "Specific blob container to test")]
        public string? Container { get; set; }

        [Option('f', "file-share", HelpText = "Specific file share to test")]
        public string? FileShare { get; set; }

        [Option('l', "list", HelpText = "List accessible blob containers, file shares, and queues")]
        public bool List { get; set; }
    }

    public interface IUrlOptions
    {
        string Url { get; set; }
        IEnumerable<string> RemainingArguments { get; set; }
    }

    [Verb("kafka-rest", HelpText = "Test Kafka REST API connection")]
    public class KafkaRestOptions : IUrlOptions
    {
        [Value(0, Required = true, MetaName = "url", HelpText = "URL for Kafka REST API (e.g., http://localhost:8082)")]
        public string Url { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('u', "username", HelpText = "Username for Basic Authentication")]
        public string? Username { get; set; }

        [Option('p', "password", HelpText = "Password for Basic Authentication")]
        public string? Password { get; set; }
    }

    public interface IBrokerOptions
    {
        string Broker { get; set; }
        IEnumerable<string> RemainingArguments { get; set; }
    }

    [Verb("kafka-broker", HelpText = "Test Kafka Broker connection")]
    public class KafkaBrokerOptions : IBrokerOptions
    {
        [Value(0, Required = true, MetaName = "broker", HelpText = "Kafka broker address (e.g., localhost:9092)")]
        public string Broker { get; set; } = string.Empty;

        [Value(1, MetaName = "remainingArgs", HelpText = "Remaining arguments (should be empty)")]
        public IEnumerable<string> RemainingArguments { get; set; } = Enumerable.Empty<string>();

        [Option('s', "security-protocol", HelpText = "Security protocol (Plaintext, Ssl, SaslPlaintext, SaslSsl)")]
        public SecurityProtocol? SecurityProtocol { get; set; }

        [Option('m', "sasl-mechanism", HelpText = "SASL mechanism (Plain, ScramSha256, ScramSha512, Gssapi, OAuthBearer)")]
        public SaslMechanism? SaslMechanism { get; set; }

        [Option('u', "username", HelpText = "SASL username")]
        public string? Username { get; set; }

        [Option('p', "password", HelpText = "SASL password")]
        public string? Password { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SqlOptions, MongoOptions, RedisOptions, AppConfigOptions, KeyVaultOptions, BlobStorageOptions, KafkaRestOptions, KafkaBrokerOptions>(args)
                .WithParsed<SqlOptions>(opts => RunIfValid(opts, TestSqlServerConnection))
                .WithParsed<MongoOptions>(opts => RunIfValid(opts, TestMongoDbConnection))
                .WithParsed<RedisOptions>(opts => RunIfValid(opts, TestRedisConnection))
                .WithParsed<AppConfigOptions>(opts => RunIfValid(opts, TestAzureAppConfig))
                .WithParsed<KeyVaultOptions>(opts => RunIfValid(opts, TestAzureKeyVault))
                .WithParsed<BlobStorageOptions>(opts => RunIfValid(opts, TestAzureBlobStorage))
                .WithParsed<KafkaRestOptions>(opts => RunIfValid(opts, TestKafkaRestConnection))
                .WithParsed<KafkaBrokerOptions>(opts => RunIfValid(opts, TestKafkaBrokerConnection))
                .WithNotParsed(HandleParseError);
        }

        static void RunIfValid<T>(T opts, Action<T> action) where T : notnull
        {
            var remainingArgs = GetRemainingArgs(opts);
            if (remainingArgs.Any())
            {
                Console.WriteLine("Error: Multiple positional arguments detected. If your connection string contains spaces, ensure it is correctly quoted.");
                Console.WriteLine("Detected extra parts: " + string.Join(" ", remainingArgs));
                Environment.Exit(1);
            }
            action(opts);
        }

        static IEnumerable<string> GetRemainingArgs(object opts)
        {
            return opts switch
            {
                IConnectionStringOptions c => c.RemainingArguments,
                IUrlOptions u => u.RemainingArguments,
                IBrokerOptions b => b.RemainingArguments,
                _ => Enumerable.Empty<string>()
            };
        }

        static void HandleParseError(IEnumerable<CommandLine.Error> errs)
        {
            // The library handles outputting help/errors by default if we don't do much here.
        }

        private static void TestAzureKeyVault(KeyVaultOptions opts)
        {
            try
            {
                TokenCredential credential;
                if (opts.UseManagedIdentity)
                {
                    credential = new DefaultAzureCredential();
                }
                else
                {
                    DeviceCodeCredentialOptions options = new DeviceCodeCredentialOptions();
                    credential = new DeviceCodeCredential(options);
                }
                
                var client = new SecretClient(new Uri(opts.ConnectionString), credential);
                var properties = client.GetPropertiesOfSecrets();
                Console.WriteLine("Connection to Azure Key Vault established successfully. Found " + properties.Count() + " properties of secrets.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Azure Key Vault: {ex.Message}");
            }
        }

        private static void TestAzureAppConfig(AppConfigOptions opts)
        {
            try
            {
                ConfigurationClient client;
                if (opts.UseManagedIdentity)
                {
                    client = new ConfigurationClient(new Uri(opts.ConnectionString), new DefaultAzureCredential());
                }
                else
                {
                    client = new ConfigurationClient(opts.ConnectionString);
                }
                
                var selector = new SettingSelector { KeyFilter = "*", LabelFilter = "*" };
                var settings = client.GetConfigurationSettings(selector);

                List<string> labels = settings.Select(s => string.IsNullOrEmpty(s.Label) ? "<EMPTY>" : s.Label).Distinct().ToList();
                Console.WriteLine("Connection to Azure App Configuration established successfully. Found " + settings.Count() + " config settings and the following labels: " + string.Join(", ", labels));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Azure App Configuration: {ex.Message}");
            }
        }

        static void TestSqlServerConnection(SqlOptions opts)
        {
            try
            {
                string connectionString = opts.ConnectionString;
                if (opts.UseManagedIdentity && !connectionString.Contains("Authentication", StringComparison.OrdinalIgnoreCase))
                {
                    if (connectionString.EndsWith(";"))
                        connectionString += "Authentication=Active Directory Default;";
                    else
                        connectionString += ";Authentication=Active Directory Default;";
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Connection to SQL Server established successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to SQL Server: {ex.Message}");
            }
        }

        static void TestMongoDbConnection(MongoOptions opts)
        {
            try
            {
                string connectionString = opts.ConnectionString;
                if (opts.UseManagedIdentity && !connectionString.Contains("authMechanism", StringComparison.OrdinalIgnoreCase))
                {
                    if (connectionString.Contains("?"))
                        connectionString += "&authMechanism=MONGODB-AZURE-MSI";
                    else
                        connectionString += "?authMechanism=MONGODB-AZURE-MSI";
                }

                var client = new MongoClient(connectionString);
                var databaseList = client.ListDatabases().ToList();
                Console.WriteLine("Connection to MongoDB established successfully. Databases:");
                foreach (var db in databaseList)
                {
                    Console.WriteLine($" - {db["name"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to MongoDB: {ex.Message}");
            }
        }
        
        static void TestRedisConnection(RedisOptions opts)
        {
            try
            {
                ConnectionMultiplexer connection;
                if (opts.UseManagedIdentity)
                {
                    var configOptions = ConfigurationOptions.Parse(opts.ConnectionString);
                    configOptions.ConfigureForAzureWithTokenCredentialAsync(new DefaultAzureCredential()).GetAwaiter().GetResult();
                    connection = ConnectionMultiplexer.Connect(configOptions);
                }
                else
                {
                    connection = ConnectionMultiplexer.Connect(opts.ConnectionString);
                }

                using (connection)
                {
                    Console.WriteLine("Connection to Redis established successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Redis: {ex.Message}");
            }
        }

        static void TestAzureBlobStorage(BlobStorageOptions opts)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(opts.Container) && !string.IsNullOrWhiteSpace(opts.FileShare))
                {
                    Console.WriteLine("Please specify only one of --container or --file-share per request.");
                    return;
                }

                if (opts.UseManagedIdentity)
                {
                    var credential = new DefaultAzureCredential();
                    var blobServiceClient = new BlobServiceClient(new Uri(opts.ConnectionString), credential);
                    var accountBaseUri = GetStorageAccountBaseUri(blobServiceClient.Uri);

                    if (!string.IsNullOrWhiteSpace(opts.Container))
                    {
                        var containerClient = blobServiceClient.GetBlobContainerClient(opts.Container);
                        ReportContainerAccess(containerClient, opts.Container);
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(opts.FileShare))
                    {
                        var shareServiceClient = new ShareServiceClient(new Uri($"{accountBaseUri}.file.core.windows.net"), credential);
                        var shareClient = shareServiceClient.GetShareClient(opts.FileShare);
                        ReportFileShareAccess(shareClient, opts.FileShare);
                        return;
                    }

                    if (opts.List)
                    {
                        ListStorageResourcesByEndpoint(blobServiceClient, credential);
                        return;
                    }

                    blobServiceClient.GetProperties();
                    Console.WriteLine("Connection to Azure Blob Storage established successfully.");
                    return;
                }

                var blobServiceFromConnectionString = new BlobServiceClient(opts.ConnectionString);
                var shareServiceFromConnectionString = new ShareServiceClient(opts.ConnectionString);

                if (!string.IsNullOrWhiteSpace(opts.Container))
                {
                    var containerClient = blobServiceFromConnectionString.GetBlobContainerClient(opts.Container);
                    ReportContainerAccess(containerClient, opts.Container);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(opts.FileShare))
                {
                    var shareClient = shareServiceFromConnectionString.GetShareClient(opts.FileShare);
                    ReportFileShareAccess(shareClient, opts.FileShare);
                    return;
                }

                if (opts.List)
                {
                    ListStorageResourcesByConnectionString(opts.ConnectionString, blobServiceFromConnectionString);
                    return;
                }

                blobServiceFromConnectionString.GetProperties();
                Console.WriteLine("Connection to Azure Blob Storage established successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Azure Blob Storage: {ex.Message}");
            }
        }

        static void ReportContainerAccess(BlobContainerClient containerClient, string containerName)
        {
            bool canRead;
            string? readError = null;
            try
            {
                containerClient.GetProperties();
                canRead = true;
            }
            catch (Exception ex)
            {
                canRead = false;
                readError = ex.Message;
            }

            bool canWrite;
            string? writeError = null;
            var testBlobName = $"service-tester-access-check-{Guid.NewGuid():N}";
            var testBlobClient = containerClient.GetBlobClient(testBlobName);

            try
            {
                using var stream = new MemoryStream(Array.Empty<byte>());
                testBlobClient.Upload(stream);
                testBlobClient.DeleteIfExists();
                canWrite = true;
            }
            catch (Exception ex)
            {
                canWrite = false;
                writeError = ex.Message;
            }

            Console.WriteLine($"Connection to Azure Blob Storage established. Container '{containerName}' access:");
            Console.WriteLine($" - Read: {(canRead ? "Yes" : "No")}");
            Console.WriteLine($" - Write: {(canWrite ? "Yes" : "No")}");

            if (!canRead && !string.IsNullOrWhiteSpace(readError))
            {
                Console.WriteLine($"   Read check error: {readError}");
            }

            if (!canWrite && !string.IsNullOrWhiteSpace(writeError))
            {
                Console.WriteLine($"   Write check error: {writeError}");
            }
        }

        static void ReportFileShareAccess(ShareClient shareClient, string shareName)
        {
            bool canRead;
            string? readError = null;
            try
            {
                shareClient.GetProperties();
                canRead = true;
            }
            catch (Exception ex)
            {
                canRead = false;
                readError = ex.Message;
            }

            bool canWrite;
            string? writeError = null;
            var testFileName = $"service-tester-access-check-{Guid.NewGuid():N}";
            var rootDirectoryClient = shareClient.GetRootDirectoryClient();
            var testFileClient = rootDirectoryClient.GetFileClient(testFileName);

            try
            {
                testFileClient.Create(0);
                testFileClient.DeleteIfExists();
                canWrite = true;
            }
            catch (Exception ex)
            {
                canWrite = false;
                writeError = ex.Message;
            }

            Console.WriteLine($"Connection to Azure Storage File Share established. File share '{shareName}' access:");
            Console.WriteLine($" - Read: {(canRead ? "Yes" : "No")}");
            Console.WriteLine($" - Write: {(canWrite ? "Yes" : "No")}");

            if (!canRead && !string.IsNullOrWhiteSpace(readError))
            {
                Console.WriteLine($"   Read check error: {readError}");
            }

            if (!canWrite && !string.IsNullOrWhiteSpace(writeError))
            {
                Console.WriteLine($"   Write check error: {writeError}");
            }
        }

        static void ListStorageResourcesByEndpoint(BlobServiceClient blobServiceClient, TokenCredential credential)
        {
            Console.WriteLine("Accessible Blob Containers:");
            foreach (var container in blobServiceClient.GetBlobContainers())
            {
                Console.WriteLine($" - {container.Name}");
            }

            var accountBaseUri = GetStorageAccountBaseUri(blobServiceClient.Uri);

            var shareServiceClient = new ShareServiceClient(new Uri($"{accountBaseUri}.file.core.windows.net"), credential);
            Console.WriteLine("Accessible File Shares:");
            foreach (var share in shareServiceClient.GetShares())
            {
                Console.WriteLine($" - {share.Name}");
            }

            var queueServiceClient = new QueueServiceClient(new Uri($"{accountBaseUri}.queue.core.windows.net"), credential);
            Console.WriteLine("Accessible Queues:");
            foreach (var queue in queueServiceClient.GetQueues())
            {
                Console.WriteLine($" - {queue.Name}");
            }

            Console.WriteLine("Storage resource listing completed successfully.");
        }

        static void ListStorageResourcesByConnectionString(string connectionString, BlobServiceClient blobServiceClient)
        {
            Console.WriteLine("Accessible Blob Containers:");
            foreach (var container in blobServiceClient.GetBlobContainers())
            {
                Console.WriteLine($" - {container.Name}");
            }

            var shareServiceClient = new ShareServiceClient(connectionString);
            Console.WriteLine("Accessible File Shares:");
            foreach (var share in shareServiceClient.GetShares())
            {
                Console.WriteLine($" - {share.Name}");
            }

            var queueServiceClient = new QueueServiceClient(connectionString);
            Console.WriteLine("Accessible Queues:");
            foreach (var queue in queueServiceClient.GetQueues())
            {
                Console.WriteLine($" - {queue.Name}");
            }

            Console.WriteLine("Storage resource listing completed successfully.");
        }

        static string GetStorageAccountBaseUri(Uri blobServiceUri)
        {
            var host = blobServiceUri.Host;
            const string blobSuffix = ".blob.core.windows.net";
            if (!host.EndsWith(blobSuffix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("When using --managed-identity, provide a Blob service URI like https://<account>.blob.core.windows.net");
            }

            return $"{blobServiceUri.Scheme}://{host[..^blobSuffix.Length]}";
        }

        static void TestKafkaRestConnection(KafkaRestOptions opts)
        {
            try
            {
                using var client = new HttpClient();
                client.BaseAddress = new Uri(opts.Url);

                if (!string.IsNullOrEmpty(opts.Username))
                {
                    var authToken = System.Text.Encoding.ASCII.GetBytes($"{opts.Username}:{opts.Password}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
                }

                // Try to get cluster information (standard Kafka REST Proxy v3 API)
                var response = client.GetAsync("/v3/clusters").GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    // Basic parsing for information requested in issue
                    Console.WriteLine("Connection to Kafka REST API established successfully.");
                    Console.WriteLine($"Response: {content}");
                }
                else
                {
                    // Fallback to v2 /brokers if v3 is not available
                    var v2Response = client.GetAsync("/brokers").GetAwaiter().GetResult();
                    if (v2Response.IsSuccessStatusCode)
                    {
                        var content = v2Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        Console.WriteLine("Connection to Kafka REST API established successfully (v2).");
                        Console.WriteLine($"Brokers: {content}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to connect to Kafka REST API. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Kafka REST API: {ex.Message}");
            }
        }

        static void TestKafkaBrokerConnection(KafkaBrokerOptions opts)
        {
            try
            {
                var config = new AdminClientConfig
                {
                    BootstrapServers = opts.Broker,
                    SecurityProtocol = opts.SecurityProtocol,
                    SaslMechanism = opts.SaslMechanism,
                    SaslUsername = opts.Username,
                    SaslPassword = opts.Password
                };
                using var adminClient = new AdminClientBuilder(config).Build();
                
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                
                Console.WriteLine("Connection to Kafka Broker established successfully.");
                Console.WriteLine("Cluster ID: " + metadata.OriginatingBrokerName); // OriginatingBrokerName is often used if ClusterId is not directly accessible or different in versions
                Console.WriteLine("Broker Count: " + metadata.Brokers.Count);
                Console.WriteLine("Brokers:");
                foreach (var broker in metadata.Brokers)
                {
                    Console.WriteLine(" - " + broker.BrokerId + ": " + broker.Host + ":" + broker.Port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Kafka Broker: {ex.Message}");
            }
        }
    }
}
