using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using CommandLine;
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
    }

    [Verb("mongo", HelpText = "Test MongoDB connection")]
    public class MongoOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for MongoDB")]
        public string ConnectionString { get; set; } = string.Empty;
    }

    [Verb("redis", HelpText = "Test Redis connection")]
    public class RedisOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Redis")]
        public string ConnectionString { get; set; } = string.Empty;
    }

    [Verb("appconfig", HelpText = "Test Azure App Configuration connection")]
    public class AppConfigOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure App Configuration")]
        public string ConnectionString { get; set; } = string.Empty;
    }

    [Verb("keyvault", HelpText = "Test Azure Key Vault connection")]
    public class KeyVaultOptions
    {
        [Value(0, Required = true, MetaName = "connectionString", HelpText = "Connection string for Azure Key Vault")]
        public string ConnectionString { get; set; } = string.Empty;
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SqlOptions, MongoOptions, RedisOptions, AppConfigOptions, KeyVaultOptions>(args)
                .WithParsed<SqlOptions>(opts => TestSqlServerConnection(opts.ConnectionString))
                .WithParsed<MongoOptions>(opts => TestMongoDbConnection(opts.ConnectionString))
                .WithParsed<RedisOptions>(opts => TestRedisConnection(opts.ConnectionString))
                .WithParsed<AppConfigOptions>(opts => TestAzureAppConfig(opts.ConnectionString))
                .WithParsed<KeyVaultOptions>(opts => TestAzureKeyVault(opts.ConnectionString))
                .WithNotParsed(HandleParseError);
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            // The library handles outputting help/errors by default if we don't do much here.
        }

        private static void TestAzureKeyVault(string connectionString)
        {
            try
            {
                DeviceCodeCredentialOptions options = new DeviceCodeCredentialOptions();
                var credential = new DeviceCodeCredential(options);
                var client = new SecretClient(new Uri(connectionString), credential);
                var properties = client.GetPropertiesOfSecrets();
                Console.WriteLine("Connection to Azure Key Vault established successfully. Found " + properties.Count() + " properties of secrets.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Azure Key Vault: {ex.Message}");
            }
        }

        private static void TestAzureAppConfig(string connectionString)
        {
            try
            {
                var client = new ConfigurationClient(connectionString);
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

        static void TestSqlServerConnection(string connectionString)
        {
            try
            {
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

        static void TestMongoDbConnection(string connectionString)
        {
            try
            {
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
        
        static void TestRedisConnection(string connectionString)
        {
            try
            {
                using (var connection = ConnectionMultiplexer.Connect(connectionString))
                {
                    Console.WriteLine("Connection to Redis established successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while connecting to Redis: {ex.Message}");
            }
        }
    }
}
