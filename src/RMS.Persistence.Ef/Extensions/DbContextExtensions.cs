using RMS.Core.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RMS.Persistence.Ef.Extensions
{
    /// <summary>
    /// Represents database context extensions
    /// </summary>
    public static class DbContextExtensions
    {
        private static readonly ConcurrentDictionary<string, string> _tableNames = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Get SQL commands from the script
        /// </summary>
        /// <param name="sql">SQL script</param>
        /// <returns>List of commands</returns>
        private static IList<string> GetCommandsFromScript(string sql)
        {
            var commands = new List<string>();

            //origin from the Microsoft.EntityFrameworkCore.Migrations.SqlServerMigrationsSqlGenerator.Generate method
            sql = Regex.Replace(sql, @"\\\r?\n", string.Empty);
            var batches = Regex.Split(sql, @"^\s*(GO[ \t]+[0-9]+|GO)(?:\s+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);

            for (var i = 0; i < batches.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(batches[i]) || batches[i].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                    continue;

                var count = 1;
                if (i != batches.Length - 1 && batches[i + 1].StartsWith("GO", StringComparison.OrdinalIgnoreCase))
                {
                    var match = Regex.Match(batches[i + 1], "([0-9]+)");
                    if (match.Success)
                        count = int.Parse(match.Value);
                }

                var builder = new StringBuilder();
                for (var j = 0; j < count; j++)
                {
                    builder.Append(batches[i]);
                    if (i == batches.Length - 1)
                        builder.AppendLine();
                }

                commands.Add(builder.ToString());
            }

            return commands;
        }

        /// <summary>
        /// Get table name of entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="context">Database context</param>
        /// <returns>Table name</returns>
        public static string GetTableName<TEntity>(this IDbContext context) where TEntity : BaseEntity
        {
            if (context == null)
                throw new ValidationException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext dbContext))
                throw new InvalidOperationException("Context does not support operation");

            var entityTypeFullName = typeof(TEntity).FullName;
            if (!_tableNames.ContainsKey(entityTypeFullName))
            {
                var adapter = ((IObjectContextAdapter)context).ObjectContext;
                var storageModel = (StoreItemCollection)adapter.MetadataWorkspace.GetItemCollection(DataSpace.SSpace);
                var containers = storageModel.GetItems<EntityContainer>();
                var entitySetBase = containers.SelectMany(c => c.BaseEntitySets.Where(bes => bes.Name == typeof(TEntity).Name)).First();

                // Here are variables that will hold table and schema name
                string entityTableName = entitySetBase.MetadataProperties.First(p => p.Name == "Table").Value.ToString();

                //get the name of the table to which the entity type is mapped
                _tableNames.TryAdd(entityTypeFullName, entityTableName);

                return entityTableName;
            }

            _tableNames.TryGetValue(entityTypeFullName, out var tableName);

            return tableName;
        }

        /// <summary>
        /// Get database name
        /// </summary>
        /// <param name="context">Database context</param>
        /// <returns>Database name</returns>
        public static string DbName(this IDbContext context)
        {
            if (context == null)
                throw new ValidationException(nameof(context));

            //try to get the EF database context
            if (!(context is DbContext _))
                throw new InvalidOperationException("Context does not support operation");

            if (!(((IObjectContextAdapter)context).ObjectContext.Connection is EntityConnection connection))
                return string.Empty;

            return connection.StoreConnection.Database;
        }

        /// <summary>
        /// Execute commands from the SQL script against the context database
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="sql">SQL script</param>
        public static void ExecuteSqlScript(this IDbContext context, string sql)
        {
            if (context == null)
                throw new ValidationException(nameof(context));

            var sqlCommands = GetCommandsFromScript(sql);
            foreach (var command in sqlCommands)
                context.ExecuteSqlCommand(command);
        }

        /// <summary>
        /// Execute commands from a file with SQL script against the context database
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="filePath">Path to the file</param>
        public static void ExecuteSqlScriptFromFile(this IDbContext context, string filePath)
        {
            if (context == null)
                throw new ValidationException(nameof(context));

            if (!File.Exists(filePath))
                return;

            context.ExecuteSqlScript(File.ReadAllText(filePath));
        }
    }
}
