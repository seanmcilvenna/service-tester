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
                Console.WriteLine("Usage: SqlConnectivityTester <database_type> <connection_string>");
                Console.WriteLine("Database types: sql, mongo, redis");
                return;
            }

            string databaseType = args[0].ToLower();
            string connectionString = args[1];

            switch (databaseType)
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

                default:
                    Console.WriteLine("Unsupported database type. Please use 'sql' or 'mongo'.");
                    break;
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
