using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSQLClassGenerator.SqlServer
{
    using System.Data;
    using DSQLClassGenerator.Util;
    public class SqlServerSchemaGetter : ISchemaGetter
    {
        IEnumerable<Tuple<string, string>> GetTables(IEnumerable<string> targetSchemas, DbConnection con)
        {
            var query = string.Format("select TABLE_SCHEMA,TABLE_NAME from INFORMATION_SCHEMA.TABLES");
            var isAny = targetSchemas == null || !targetSchemas.Any();
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = query;
                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        var tname = reader.GetString(0);
                        if (isAny || targetSchemas.Any(x => x == tname))
                        {
                            yield return Tuple.Create(reader.GetString(0), reader.GetString(1));
                        }
                    }
                }
            }
        }
        IDictionary<string, string> GetColumnSqlTypes(string schema, string tableName, IDbConnection con)
        {
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = string.Format("select COLUMN_NAME,DATA_TYPE from INFORMATION_SCHEMA.COLUMNS where TABLE_SCHEMA=@TABLE_SCHEMA and TABLE_NAME=@TABLE_NAME");
                cmd.AddParameter("TABLE_SCHEMA", schema);
                cmd.AddParameter("TABLE_NAME", tableName);
                using (var reader = cmd.ExecuteReader())
                {
                    var ret = new Dictionary<string, string>();
                    while (reader.Read())
                    {
                        ret[reader.GetString(0)] = reader.GetString(1);
                    }
                    return ret;
                }
            }
        }
        DbType SqlTypeToDbType(SqlDbType t)
        {
            switch(t)
            {
                case SqlDbType.BigInt:
                    return DbType.Int64;
                case SqlDbType.Binary:
                    return DbType.Binary;
                case SqlDbType.Bit:
                    return DbType.Boolean;
                case SqlDbType.Char:
                    return DbType.AnsiStringFixedLength;
                case SqlDbType.Date:
                    return DbType.Date;
                case SqlDbType.DateTime:
                    return DbType.DateTime;
                case SqlDbType.DateTime2:
                    return DbType.DateTime2;
                case SqlDbType.DateTimeOffset:
                    return DbType.DateTimeOffset;
                case SqlDbType.Decimal:
                    return DbType.Decimal;
                case SqlDbType.Float:
                    return DbType.Double;
                case SqlDbType.Image:
                    return DbType.Binary;
                case SqlDbType.Int:
                    return DbType.Int32;
                case SqlDbType.Money:
                    return DbType.VarNumeric;
                case SqlDbType.NChar:
                    return DbType.StringFixedLength;
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                    return DbType.String;
                case SqlDbType.Real:
                    return DbType.Double;
                case SqlDbType.SmallDateTime:
                    return DbType.DateTime;
                case SqlDbType.SmallInt:
                    return DbType.Int16;
                case SqlDbType.SmallMoney:
                    return DbType.VarNumeric;
                case SqlDbType.Text:
                    return DbType.AnsiString;
                case SqlDbType.Time:
                    return DbType.Time;
                case SqlDbType.Timestamp:
                    return DbType.DateTimeOffset;
                case SqlDbType.TinyInt:
                    return DbType.SByte;
                case SqlDbType.UniqueIdentifier:
                    return DbType.Guid;
                case SqlDbType.VarBinary:
                    return DbType.Binary;
                case SqlDbType.VarChar:
                    return DbType.AnsiString;
                default:
                    return DbType.String;
            }
        }
        IReadOnlyDictionary<string, DbColumn> GetFieldTypes(string schema, string tableName, DbConnection con)
        {
            using (var cmd = con.CreateCommand())
            {
                // all I want is only column information,not row info
                cmd.CommandText = $"select * from {schema}.{tableName} where 1 = 2";
                var ret = new Dictionary<string, Type>();
                using (var reader = cmd.ExecuteReader(CommandBehavior.KeyInfo))
                {
                    return reader.GetColumnSchema().ToDictionary(x=>x.ColumnName);
                }
            }
        }
        IEnumerable<ColumnInfo> GetColumns(string schema, string tableName, DbConnection con)
        {
            //var schemaTables = GetColmnsFromSchemaTable(schema, tableName, con);
            var typeNames = GetColumnSqlTypes(schema, tableName, con);
            var typeMap = GetFieldTypes(schema, tableName, con);
            foreach (var column in typeMap)
            {
                var ci = new ColumnInfo();
                ci.CSType = column.Value.DataType;
                ci.RawSqlDataType = typeNames[column.Key];
                ci.IsPrimary = column.Value.IsKey.GetValueOrDefault(false);
                ci.IsNullable = column.Value.AllowDBNull.GetValueOrDefault(false);
                if (column.Value.IsAutoIncrement.GetValueOrDefault(false))
                {
                    ci.Sequence = new SequenceInfo();
                }
                //ci.DatabaseType = SqlTypeToDbType((SqlDbType)column.Value["NonVersionedProviderType"]);
                ci.Name = column.Key;
                if (column.Value.NumericPrecision.HasValue)
                {
                    var v = column.Value.NumericPrecision.Value;
                    ci.NumericPrecision = v == 255 ? -1 : v;
                }
                if (column.Value.NumericScale.HasValue)
                {
                    var v = column.Value.NumericPrecision.Value;
                    ci.NumericScale = v == 255 ? -1 : v;
                }
                if (column.Value.ColumnSize.HasValue)
                {
                    ci.Size = column.Value.ColumnSize.Value;
                }
                yield return ci;
            }
        }
        public IEnumerable<TableInfo> Get(IEnumerable<string> targetSchemas, DbConnection con)
        {
            foreach (var table in GetTables(targetSchemas, con).ToArray())
            {
                var ti = new TableInfo();
                ti.Schema = table.Item1;
                ti.Name = table.Item2;
                ti.Columns = GetColumns(table.Item1, table.Item2, con).ToArray();
                yield return ti;
            }
        }
    }
}
