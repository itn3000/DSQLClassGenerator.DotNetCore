using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DSQLClassGenerator.Test.SqlServer
{
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;
    using Xunit.Abstractions;
    public class TestSqlServerSchema
    {
        ITestOutputHelper m_Outputter;
        public TestSqlServerSchema(ITestOutputHelper outputter)
        {
            m_Outputter = outputter;
        }
        DbConnection GetConnection()
        {
            var sb = new SqlConnectionStringBuilder();
            sb.ApplicationName = "testdsql";
            sb.InitialCatalog = "testdsql";
            sb.DataSource = @"(localdb)\testdsql";
            sb.IntegratedSecurity = true;
            //sb.AttachDBFilename = Path.Combine(Directory.GetCurrentDirectory(), "tmp", "testsqlserver.mdf");
            //if(!Directory.Exists(Path.GetDirectoryName(sb.AttachDBFilename)))
            //{
            //    Directory.CreateDirectory(Path.GetDirectoryName(sb.AttachDBFilename));
            //}
            return new SqlConnection(sb.ToString());
        }
        void CreateDatabase()
        {
            var sb = new SqlConnectionStringBuilder();
            sb.ApplicationName = "testdsql";
            sb.DataSource = @"(localdb)\testdsql";
            sb.IntegratedSecurity = true;
            using (var con = new SqlConnection(sb.ToString()))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
if not exists (select * from sys.databases where name = 'testdsql')
begin
    create database testdsql
end
";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        void CreateTestTable(DbConnection con, string tableName, IEnumerable<string> columns, string primaryKey)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = $@"
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME='{tableName}')
begin
  drop table {tableName}
end
create table {tableName}({string.Join(",",columns)},constraint PK_{tableName} primary key clustered({primaryKey}))
";
                cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// test for getting schema information for sqlserver
        /// </summary>
        /// <remarks>you must be able to login sqlserver (localdb)\testdsql with integrated security</remarks>
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
                        "a nvarchar(100) not null",
                        "b bigint",
                        "c varbinary(MAX)"
                    };
                CreateTestTable(con, tableName, columns, "a");
                var getter = new DSQLClassGenerator.SqlServer.SqlServerSchemaGetter();
                var tables = getter.Get(null, con);
                Assert.Contains(tableName, tables.Select(x => x.Name));
                Assert.Equal(columns.Count(), tables.First(x => x.Name == tableName).Columns.Count());
                foreach (var table in tables)
                {
                    m_Outputter.WriteLine($"tablename = {table.Schema}.{table.Name}");
                    var str = string.Join(",", table.Columns.Select(x => $"{x.CSType},{x.Name}"));
                    m_Outputter.WriteLine($"column = {str}");
                }
            }
        }

    }
}
