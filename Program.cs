using System.Text;
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
        private static string? GetArgumentValue(string[] args, int startingIndex, out int nextIndex)
        {
            if (args[startingIndex].StartsWith("--"))
            {
                nextIndex = startingIndex-1;
                return null;
            }

            if (args[startingIndex].StartsWith("\""))
            {
                StringBuilder sb = new StringBuilder();
                for (int i = startingIndex; i < args.Length; i++)
                {
                    sb.Append(args[i]);
                    if (args[i].EndsWith("\""))
                    {
                        nextIndex = i + 1;
                        return sb.ToString().Trim('"');
                    }
                    else
                    {
                        nextIndex = i + 1;
                        sb.Append(" ");
                    }
                }
            }
            
            nextIndex = startingIndex + 1;
            return args[nextIndex - 1];
        }
        
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: service-tester <service_type> <connection_string> [--key-filter <key_filter>] [--label-filter <label_filter>]");
                Console.WriteLine("Service types: sql, mongo, redis, appconfig, keyvault");
                return;
            }

            string serviceType = args[0].ToLower();
            string connectionString = args[1];
            string keyFilter = "*";
            string labelFilter = "*";

            for (int i = 2; i < args.Length; i++)
            {
                var nextIndex = i + 1;
                
                if (args[i] == "--key-filter" && i + 1 < args.Length)
                {
                    keyFilter = GetArgumentValue(args, nextIndex, out nextIndex);
                    i = nextIndex;
                }
                else if (args[i] == "--label-filter" && i + 1 < args.Length)
                {
                    labelFilter = GetArgumentValue(args, nextIndex, out nextIndex);
                    i = nextIndex;
                }
            }

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
                    TestAzureAppConfig(connectionString, keyFilter, labelFilter);
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

        private static void TestAzureAppConfig(string connectionString, string? keyFilter = "*", string? labelFilter = "*")
        {
            keyFilter ??= "*";
            labelFilter ??= "*";
            
            try
            {
                var client = new ConfigurationClient(connectionString);
                var selector = new SettingSelector { KeyFilter = keyFilter, LabelFilter = labelFilter };
                var settings = client.GetConfigurationSettings(selector);

                var labels = settings.Select(s => string.IsNullOrEmpty(s.Label) ? "<EMPTY>" : s.Label).Distinct().ToList();

                if (!settings.Any())
                    Console.WriteLine("Connection to Azure App Configuration established successfully, but found no configs");
                else
                {
                    if (keyFilter != "*" || labelFilter != "*")
                    {
                        Console.WriteLine("Connection to Azure App Configuration established successfully. Found " + settings.Count() + " config settings:");
                        
                        foreach (var setting in settings)
                        {
                            Console.WriteLine($"Key: {setting.Key}, Value: {setting.Value}, Label: {setting.Label}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Connection to Azure App Configuration established successfully. Found " +
                                          settings.Count() + " config settings and the following labels: " +
                                          string.Join(", ", labels));   
                    }
                }
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
