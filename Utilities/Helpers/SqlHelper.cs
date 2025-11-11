using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Temperance.Utilities.Helpers
{
	public class SqlHelper : ISqlHelper
	{
		private readonly string _connectionString;
		private readonly ConcurrentDictionary<string, SemaphoreSlim> _tableLocks = new();

        public SqlHelper(string connectionString)
		{
            _connectionString = connectionString;
		}

        public async Task<bool> TableExists(string tableName)
        {
            using var connection = new SqlConnection(_connectionString);
            var cleanTableName = tableName.Replace("[", "").Replace("]", "").Split('.').Last();
            return await connection.ExecuteScalarAsync<bool>(
                @"SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'Prices' 
                        AND TABLE_NAME = @TableName
                ) THEN 1 ELSE 0 END",
                new { TableName = cleanTableName });
        }

        public string SanitizeTableName(string symbol, string interval)
		{
			var cleanSymbol = Regex.Replace(symbol, @"[^a-zA-Z0-9]", "_");
			var cleanInterval = Regex.Replace(interval, @"[^a-zA-Z0-9]", "_");
			return $"[Prices].[{cleanSymbol}_{cleanInterval}]";
		}

        public async Task EnsureTableExists(string tableName)
        {
            var lockKey = tableName.ToLower();
            var tableLock = _tableLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

            await tableLock.WaitAsync();
            try
            {
                using var connection = new SqlConnection(_connectionString);

                var cleanTableName = tableName.Replace("[", "").Replace("]", "").Split('.').Last();

                var tableExists = await connection.ExecuteScalarAsync<bool>(
                    @"SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM INFORMATION_SCHEMA.TABLES 
                        WHERE TABLE_SCHEMA = 'Prices' 
                        AND TABLE_NAME = @TableName
                    ) THEN 1 ELSE 0 END",
                    new { TableName = cleanTableName });

                if (!tableExists)
                {
                    var columnDefinitions = new List<string>
                        {
                            "SecurityID INT NOT NULL",
                            "Symbol NVARCHAR(50) NOT NULL",
                            "Timestamp DATETIME2 NOT NULL",
                            "OpenPrice DECIMAL(18,4) NOT NULL",
                            "HighPrice DECIMAL(18, 4) NOT NULL",
                            "LowPrice DECIMAL(18, 4) NOT NULL",
                            "ClosePrice DECIMAL(18, 4) NOT NULL"
                        };

                    var isOneDay = tableName.Contains("_1d", StringComparison.OrdinalIgnoreCase);
                    if (isOneDay)
                    {
                        columnDefinitions.Add("AdjustedClosePrice DECIMAL(18,4) NOT NULL");
                        columnDefinitions.Add("Volume BIGINT NOT NULL"); // Volume always present
                        columnDefinitions.Add("DividendAmount DECIMAL(18,4) NOT NULL");
                        columnDefinitions.Add("SplitCoefficient DECIMAL(18,4) NOT NULL");
                        columnDefinitions.Add("TimeInterval NVARCHAR(50) NOT NULL");
                    }
                    else
                    {

                        columnDefinitions.Add("Volume BIGINT NOT NULL");
                        // Volume always present
                        columnDefinitions.Add("TimeInterval NVARCHAR(50) NOT NULL");
                    }

                    var createTablePortion = string.Join(",\n                        ", columnDefinitions);

                    var query = $@"CREATE TABLE {tableName} (
                    {createTablePortion},
                    CONSTRAINT PK_{cleanTableName} PRIMARY KEY (Timestamp)
                );";

                    await connection.ExecuteAsync(query);
                }
            }
            finally
            {
                tableLock.Release();
            }
        }
    }
}