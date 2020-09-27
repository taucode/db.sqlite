using System;
using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace TauCode.Db.SQLite.Tests
{
    internal class TestMigrator
    {
        internal TestMigrator(string connectionString, Assembly migrationsAssembly)
        {
            this.ConnectionString = connectionString;
            this.MigrationsAssembly = migrationsAssembly;
        }

        internal string ConnectionString { get; }

        internal Assembly MigrationsAssembly { get; }

        internal void Migrate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString))
            {
                throw new InvalidOperationException("Connection string must not be empty.");
            }

            if (this.MigrationsAssembly == null)
            {
                throw new InvalidOperationException("'MigrationsAssembly' must not be null.");
            }

            var serviceCollection = new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore();

            var serviceProvider = serviceCollection
                .ConfigureRunner(rb =>
                {
                    rb.AddSQLite();

                    rb
                        // Set the connection string
                        .WithGlobalConnectionString(this.ConnectionString)
                        // Define the assembly containing the migrations
                        .ScanIn(this.MigrationsAssembly).For.Migrations();
                })
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                // Build the service provider
                .BuildServiceProvider(false);

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (serviceProvider.CreateScope())
            {
                // Instantiate the runner
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                // Execute the migrations
                runner.MigrateUp();
            }
        }
    }
}
