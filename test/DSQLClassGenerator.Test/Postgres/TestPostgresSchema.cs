using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DSQLClassGenerator.Test.Postgres
{
    using Npgsql;
    using System.Data.Common;
    public class TestPostgresSchema
    {
        void CreateDatabase()
        {
            var sb = new NpgsqlConnectionStringBuilder();
            sb.Host = "localhost";
            sb.Username = "postgres";
            using (var con = new NpgsqlConnection(sb))
            {
                con.Open();
                bool isExist = true;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = $"select datname from pg_database where datname='testdsql'";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            isExist = true;
                        }
                        else
                        {
                            isExist = false;
                        }
                    }
                }
                if (!isExist)
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = @"create database testdsql;";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
        DbConnection GetConnection()
        {
            var sb = new NpgsqlConnectionStringBuilder();
            sb.Host = "localhost";
            sb.Username = "postgres";
            sb.Database = "testdsql";
            return new NpgsqlConnection(sb);
        }
        void CreateTable(DbConnection con, string tableName, IEnumerable<string> columns, string primaryKey)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = $"drop table if exists {tableName}";
                cmd.ExecuteNonQuery();
                var str = string.Join(",", columns);
                cmd.CommandText = $"create table {tableName}({str},PRIMARY KEY({primaryKey}))";
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// test for getting schema information of postgresql
        /// </summary>
        /// <remarks>you must be able to login local postgres server as 'postgres' without pass</remarks>
        [Fact]
        public void TestGet()
        {
            CreateDatabase();
            using (var con = GetConnection())
            {
                con.Open();
                var tableName = "testdsql";
                var columns = new string[]
                {
                    "a varchar(100)",
                    "b int",
                    "c bytea"
                };
                CreateTable(con, tableName, columns, "a");
                var getter = new DSQLClassGenerator.Postgres.NpgsqlSchemaGetter();
                var tables = getter.Get(null, con);
                Assert.NotEmpty(tables);
                Assert.Contains(tableName, tables.Select(x => x.Name));
                Assert.Equal(columns.Count(), tables.First(x => x.Name == tableName).Columns.Count());
            }
        }
    }
}
