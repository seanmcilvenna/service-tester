using Azure.Data.AppConfiguration;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;
using MongoDB.Driver;
using StackExchange.Redis;

namespace ServiceTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: service-tester <service_type> <connection_string>");
                Console.WriteLine("Service types: sql, mongo, redis, appconfig, keyvault");
                return;
            }

            string serviceType = args[0].ToLower();
            string connectionString = args[1];

            switch (serviceType)
            {
                case "sql":
                    TestSqlServerConnection(connectionString);
                    break;

                case "mongo":
                    TestMongoDbConnection(connectionString);
                    break;
                
                case "redis":
                    TestRedisConnection(connectionString);
                    break;
                
                case "appconfig":
                    TestAzureAppConfig(connectionString);
                    break;
                
                case "keyvault":
                    TestAzureKeyVault(connectionString);
                    break;

                default:
                    Console.WriteLine("Unsupported database type " + serviceType);
                    break;
            }
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
