using DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DataAccessLayer
{
    public static class MatrixIncDbInitializer
    {
        public static void Initialize(MatrixIncDbContext context)
        {
            EnsureProductStockColumn(context);
            EnsureOrderWorkflowColumns(context);
            NormalizeSeedProductCategories(context);

            if (context.Customers.Any())
            {
                return;
            }

            var customers = new Customer[]
            {
                new Customer { Name = "Neo", Address = "123 Elm St", Active = true },
                new Customer { Name = "Morpheus", Address = "456 Oak St", Active = true },
                new Customer { Name = "Trinity", Address = "789 Pine St", Active = true }
            };
            context.Customers.AddRange(customers);

            var orders = new Order[]
            {
                new Order { Customer = customers[0], OrderDate = DateTime.Parse("2021-01-01"), Status = "Afgerond", Source = "Webshop" },
                new Order { Customer = customers[0], OrderDate = DateTime.Parse("2021-02-01"), Status = "Nieuw", Source = "Webshop", ExternalReference = "WEB-1002" },
                new Order { Customer = customers[1], OrderDate = DateTime.Parse("2021-02-01"), Status = "In behandeling", Source = "Webshop" },
                new Order { Customer = customers[2], OrderDate = DateTime.Parse("2021-03-01"), Status = "Doorgegeven aan bezorger", Source = "Webshop", DeliveryPerson = "Switch", SentToDeliveryAt = DateTime.Parse("2021-03-01 10:30") }
            };
            context.Orders.AddRange(orders);

            var products = new Product[]
            {
                new Product { Name = "Nebuchadnezzar", Description = "Het schip waarop Neo voor het eerst de echte wereld leert kennen", Category = "Schepen", Price = 10000.00m, Stock = 3 },
                new Product { Name = "Jack-in Chair", Description = "Stoel met een rugsteun en metalen armen waarin mensen zitten om ingeplugd te worden in de Matrix via een kabel in de nekpoort", Category = "Hardware", Price = 500.50m, Stock = 12 },
                new Product { Name = "EMP (Electro-Magnetic Pulse) Device", Description = "Wapentuig op de schepen van Zion", Category = "Verdediging", Price = 129.99m, Stock = 4 }
            };
            context.Products.AddRange(products);

            var parts = new Part[]
            {
                new Part { Name = "Tandwiel", Description = "Overdracht van rotatie in bijvoorbeeld de motor of luikmechanismen" },
                new Part { Name = "M5 Boutje", Description = "Bevestiging van panelen, buizen of interne modules" },
                new Part { Name = "Hydraulische cilinder", Description = "Openen/sluiten van zware luchtsluizen of bewegende onderdelen" },
                new Part { Name = "Koelvloeistofpomp", Description = "Koeling van de motor of elektronische systemen." }
            };
            context.Parts.AddRange(parts);

            orders[0].Products.Add(products[0]);
            orders[1].Products.Add(products[1]);
            orders[1].Products.Add(products[2]);
            orders[2].Products.Add(products[2]);
            orders[3].Products.Add(products[0]);
            orders[3].Products.Add(products[1]);

            context.SaveChanges();
        }

        private static void EnsureProductStockColumn(MatrixIncDbContext context)
        {
            EnsureColumn(context, "Products", "Stock", "INTEGER NOT NULL DEFAULT 0");
            EnsureColumn(context, "Products", "Category", "TEXT NOT NULL DEFAULT 'Algemeen'");
        }

        private static void EnsureOrderWorkflowColumns(MatrixIncDbContext context)
        {
            EnsureColumn(context, "Orders", "Status", "TEXT NOT NULL DEFAULT 'Nieuw'");
            EnsureColumn(context, "Orders", "Source", "TEXT NOT NULL DEFAULT 'Webshop'");
            EnsureColumn(context, "Orders", "ExternalReference", "TEXT NULL");
            EnsureColumn(context, "Orders", "DeliveryPerson", "TEXT NULL");
            EnsureColumn(context, "Orders", "SentToDeliveryAt", "TEXT NULL");
        }

        private static void NormalizeSeedProductCategories(MatrixIncDbContext context)
        {
            context.Database.ExecuteSqlRaw("UPDATE Products SET Category = 'Schepen' WHERE Name = 'Nebuchadnezzar' AND Category = 'Algemeen'");
            context.Database.ExecuteSqlRaw("UPDATE Products SET Category = 'Hardware' WHERE Name = 'Jack-in Chair' AND Category = 'Algemeen'");
            context.Database.ExecuteSqlRaw("UPDATE Products SET Category = 'Verdediging' WHERE Name = 'EMP (Electro-Magnetic Pulse) Device' AND Category = 'Algemeen'");
        }

        private static void EnsureColumn(MatrixIncDbContext context, string tableName, string columnName, string definition)
        {
            var connection = context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State == ConnectionState.Closed;

            if (shouldCloseConnection)
            {
                connection.Open();
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = $"PRAGMA table_info('{tableName}')";

                var hasColumn = false;

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                        {
                            hasColumn = true;
                            break;
                        }
                    }
                }

                if (!hasColumn)
                {
                    using var alterCommand = connection.CreateCommand();
                    alterCommand.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition}";
                    alterCommand.ExecuteNonQuery();
                }
            }
            finally
            {
                if (shouldCloseConnection)
                {
                    connection.Close();
                }
            }
        }
    }
}
