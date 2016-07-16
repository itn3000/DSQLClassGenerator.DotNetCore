using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Test
{
    using Xunit;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Text;
    using Microsoft.CodeAnalysis.Formatting;
    using System.IO;
    using Xunit.Abstractions;
    public class TestGenerator
    {
        ITestOutputHelper m_Outputter;
        public TestGenerator(ITestOutputHelper outputter)
        {
            m_Outputter = outputter;
        }
        [Fact]
        public void TestGenerateMultiTable()
        {
            var tables = Enumerable.Range(0, 2)
                .Select(x => new TableInfo()
                {
                    Schema = "hogehoge"
                    ,
                    Name = $"table{x}"
                    ,
                    Columns = new ColumnInfo[]
                    {
                        new ColumnInfo()
                        {
                            Name = "col1",
                            CSType = typeof(string),
                            IsNullable = true,
                            IsPrimary = true,
                            NumericPrecision = null,
                            NumericPrecisionRadix = null,
                            NumericScale = null,
                            RawSqlDataType = "varchar",
                            Sequence = null,
                            Size = 100,
                        }
                        ,
                        new ColumnInfo()
                        {
                            Name = "col2",
                            CSType = typeof(int),
                            IsNullable = true,
                            IsPrimary = false,
                            NumericPrecision = null,
                            NumericPrecisionRadix = null,
                            NumericScale = null,
                            RawSqlDataType = "int",
                            Sequence = null,
                        }
                        ,
                        new ColumnInfo()
                        {
                            Name = "col3",
                            CSType = typeof(byte[]),
                            IsNullable = true,
                            IsPrimary = false,
                            NumericPrecision = null,
                            NumericPrecisionRadix = null,
                            NumericScale = null,
                            RawSqlDataType = "bytea",
                            Sequence = null,
                        }
                    }
                })
                ;
            var gen = new DSQLClassGenerator.Generator.ClassGeneratorTemplate(tables, "testns", false);
            var str = gen.TransformText();
            // check syntax
            var diags = CSharpSyntaxTree.ParseText(str).GetDiagnostics();
            Assert.Empty(diags);
        }
        [Fact]
        public void TestGenerate()
        {
            var table = new TableInfo()
            {
                Schema = "hogehoge",
                Name = "abc",
                Columns = new ColumnInfo[]
                {
                    new ColumnInfo()
                    {
                        Name = "col1",
                        CSType = typeof(string),
                        IsNullable = true,
                        IsPrimary = true,
                        NumericPrecision = null,
                        NumericPrecisionRadix = null,
                        NumericScale = null,
                        RawSqlDataType = "varchar",
                        Sequence = null,
                        Size = 100,
                    }
                    ,
                    new ColumnInfo()
                    {
                        Name = "col2",
                        CSType = typeof(int),
                        IsNullable = true,
                        IsPrimary = false,
                        NumericPrecision = null,
                        NumericPrecisionRadix = null,
                        NumericScale = null,
                        RawSqlDataType = "int",
                        Sequence = null,
                    }
                }
                ,
            };
            var gen = new DSQLClassGenerator.Generator.ClassGeneratorTemplate(new TableInfo[] { table }, "TestNamespace", true);
            var result = gen.TransformText();
            Assert.False(string.IsNullOrEmpty(result));
            // check syntax
            var diag = CSharpSyntaxTree.ParseText(result).GetDiagnostics();
            Assert.Empty(diag);
        }
    }
}
