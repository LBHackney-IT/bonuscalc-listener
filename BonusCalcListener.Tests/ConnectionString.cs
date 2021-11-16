using System;

namespace BonusCalcListener.Tests
{
    public static class ConnectionString
    {
        public static string TestDatabase()
        {
            // Purposefully matching port spec in BC Api

            return $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1"};" +
                   $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5435"};" +
                   $"Username={Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres"};" +
                   $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "mypassword"};" +
                   $"Database={Environment.GetEnvironmentVariable("DB_DATABASE") ?? "testdb"}";
        }
    }
}
