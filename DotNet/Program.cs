using Azure.Core;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CommandLine;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using StackExchange.Redis;

namespace ServiceTester
{
    [Verb("sql", HelpText = "Test SQL Server connection")]
    public class SqlOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for SQL Server")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("mongo", HelpText = "Test MongoDB connection")]
    public class MongoOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for MongoDB")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("redis", HelpText = "Test Redis connection")]
    public class RedisOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Redis")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("appconfig", HelpText = "Test Azure App Configuration connection")]
    public class AppConfigOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure App Configuration")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("keyvault", HelpText = "Test Azure Key Vault connection")]
    public class KeyVaultOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure Key Vault")]
        public string ConnectionString { get; set; } = string.Empty;

        [Option('m', "managed-identity", HelpText = "Use Azure Managed Identity for authentication")]
        public bool UseManagedIdentity { get; set; }
    }

    [Verb("kafka-rest", HelpText = "Test Kafka REST API connection")]
    public class KafkaRestOptions
    {
        [Value(0, Required = true, MetaName = "url", HelpText = "URL for Kafka REST API (e.g., http://localhost:8082)")]
        public string Url { get; set; } = string.Empty;

        [Option('u', "username", HelpText = "Username for Basic Authentication")]
        public string? Username { get; set; }

        [Option('p', "password", HelpText = "Password for Basic Authentication")]
        public string? Password { get; set; }
    }

    [Verb("kafka-broker", HelpText = "Test Kafka Broker connection")]
    public class KafkaBrokerOptions
    {
        [Value(0, Required = true, MetaName = "broker", HelpText = "Kafka broker address (e.g., localhost:9092)")]
        public string Broker { get; set; } = string.Empty;

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
            Parser.Default.ParseArguments<SqlOptions, MongoOptions, RedisOptions, AppConfigOptions, KeyVaultOptions, KafkaRestOptions, KafkaBrokerOptions>(args)
                .WithParsed<SqlOptions>(opts => TestSqlServerConnection(opts))
                .WithParsed<MongoOptions>(opts => TestMongoDbConnection(opts))
                .WithParsed<RedisOptions>(opts => TestRedisConnection(opts))
                .WithParsed<AppConfigOptions>(opts => TestAzureAppConfig(opts))
                .WithParsed<KeyVaultOptions>(opts => TestAzureKeyVault(opts))
                .WithParsed<KafkaRestOptions>(opts => TestKafkaRestConnection(opts))
                .WithParsed<KafkaBrokerOptions>(opts => TestKafkaBrokerConnection(opts))
                .WithNotParsed(HandleParseError);
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
