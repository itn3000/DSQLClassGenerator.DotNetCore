using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DSQLClassGenerator.Generator
{
    using System.Text;
    using DSQLClassGenerator.Util;
    public class ClassGeneratorTemplate
    {
        TableInfo[] m_TableInfoList;
        string m_Namespace;
        bool m_IsOutputSchema;
        public ClassGeneratorTemplate(IEnumerable<TableInfo> tableInfoList, string ns, bool isOutputSchemaAttribute)
        {
            m_TableInfoList = tableInfoList.ToArray();
            m_Namespace = ns;
            m_IsOutputSchema = isOutputSchemaAttribute;
        }
        public string TransformText()
        {
            var sb = new StringBuilder();
            sb.Append(@"using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DeclarativeSql.Annotations;
");
            sb.Append($@"
namespace {m_Namespace} {{");
            foreach (var ti in m_TableInfoList)
            {
                var schemaAttr = !string.IsNullOrEmpty(ti.Schema) && m_IsOutputSchema ? $@", Schema = ""{ti.Schema}""" : "";
                sb.Append($@"
    [Table(""{ti.Name}""{schemaAttr})]
    public class {ti.Name}
    {{");
                foreach (var ci in ti.Columns)
                {
                    if(ci.IsAutoIncrement)
                    {
                        sb.Append($@"
        [AutoIncrement]");
                    }
                    if(ci.IsPrimary)
                    {
                        sb.Append($@"
        [Key]");
                    }
                    sb.Append($@"
        public {ci.CSType} {ci.Name} {{ get; set; }}");
                }
                sb.Append(@"
    }");
            }
            sb.AppendLine(@"
}");
            return sb.ToString();
        }
    }
}
