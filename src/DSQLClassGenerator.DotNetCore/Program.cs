using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;

namespace DSQLClassGenerator
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.CommandLineUtils;
    using TableConvertFunc = Func<string, string>;
    using DSQLClassGenerator.Generator;
    using System.Data.Common;
    using System.Data;
    using Util;
    using System.IO;
    class Program
    {
        enum Providers
        {
            None,
            SqlServer,
            Postgres,
        }
        static string OutputPath = "out";
        static string OutputFilePath = "TableDefinitions.cs";
        static string Namespace = "Example";
        static bool OutputSchema = true;
        static string ExternalAppConfig = null;
        static bool IsAmalgamation = false;
        static Providers Provider = Providers.None;
        static CommandLineApplication SetupApplication()
        {
            var app = new CommandLineApplication(false);
            app.Option("-a|--amalgamation", "Creating classes into one .cs file", CommandOptionType.NoValue, opt =>
             {
                 IsAmalgamation = true;
             });
            app.Option("-d|--outputdir <OUTPUTDIR>", "Output directory(default: 'out',ignored when amalgamation)", CommandOptionType.SingleValue);
            app.Option("-f|--outputfile <OUTPUTFILE>", "Output file path(default: 'TablesDefinitions.cs',ignored when no amalgamation)", CommandOptionType.SingleValue);
            app.Option("-n|--namespace <NAMESPACE>", "Namespace(default: 'Example')", CommandOptionType.SingleValue);
            app.Option("-s|--noschema", "generate class without Schema attribute(default: output with schema attribute)", CommandOptionType.NoValue);
            app.Option("-c|--config <CONFIG>", "Specify external configuration file path(configuration name must be 'Target')", CommandOptionType.SingleValue);
            app.Option("-p|--provider <PROVIDER>", "Specify database provider,possible values are 'postgres' and 'sqlserver'", CommandOptionType.SingleValue);
            app.HelpOption("-h|--help");
            var ver = new AssemblyName(Assembly.GetEntryAssembly().FullName).Version.ToString();
            app.VersionOption("-v|--version", ver, ver);
            return app;
        }
        static void WriteCsFile(TableInfo tableInfo)
        {
            var generator = new ClassGeneratorTemplate(new[] { tableInfo }, Namespace, OutputSchema);
            var filePath = Path.Combine(OutputPath != null ? OutputPath : "", $"{tableInfo.Name}.cs");
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            File.WriteAllText(filePath, generator.TransformText(), Encoding.UTF8);
        }
        static void WriteAmalgamatedCsFile(IEnumerable<TableInfo> tableInfo)
        {
            var filePath = OutputFilePath;
            var dirPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            var generator = new ClassGeneratorTemplate(tableInfo, Namespace, OutputSchema);
            File.WriteAllText(filePath, generator.TransformText(), Encoding.UTF8);
        }
        static IConfigurationRoot ReadConfiguration(string fileName)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(fileName)
                .Build();
            return config;
        }
        static DbConnection CreateConnection()
        {
            var config = ReadConfiguration(ExternalAppConfig);
            var connectionString = config["ConnectionString"];
            var providerString = config["Provider"].ToLower();
            switch (providerString)
            {
                case "sqlserver":
                    Provider = Providers.SqlServer;
                    break;
                case "postgres":
                    Provider = Providers.Postgres;
                    break;
                default:
                    throw new ArgumentException($"unknown provider name:{providerString}");
            }
            DbProviderFactory factory;
            switch(Provider)
            {
                case Providers.SqlServer:
                    factory = System.Data.SqlClient.SqlClientFactory.Instance;
                    break;
                case Providers.Postgres:
                    factory = Npgsql.NpgsqlFactory.Instance;
                    break;
                default:
                    throw new Exception("provider not specified");
            }
            var con = factory.CreateConnection();
            con.ConnectionString = connectionString;
            return con;
        }
        static void ReadOptions(IList<CommandOption> opts)
        {
            foreach (var opt in opts)
            {
                switch (opt.ShortName)
                {
                    case "c":
                        ExternalAppConfig = opt.HasValue() ? opt.Value() : null;
                        break;
                    case "a":
                        IsAmalgamation = opt.HasValue();
                        break;
                    case "d":
                        OutputPath = opt.HasValue() ? opt.Value() : "out";
                        break;
                    case "s":
                        OutputSchema = !opt.HasValue();
                        break;
                    case "n":
                        Namespace = opt.HasValue() ? opt.Value() : "Example";
                        break;
                    default:
                        break;
                }
            }
        }
        static void Main(string[] args)
        {
            try
            {
                var app = SetupApplication();
                app.OnExecute(() =>
                {
                    ReadOptions(app.Options);
                    using (var con = CreateConnection())
                    {
                        con.Open();
                        var schemaGetter = SchemaGetterFactory.Create(con);
                        var schemas = schemaGetter.Get(null, con);
                        if (IsAmalgamation)
                        {
                            WriteAmalgamatedCsFile(schemas);
                        }
                        else
                        {
                            foreach (var mappingInfo in schemaGetter.Get(null, con))
                            {
                                WriteCsFile(mappingInfo);
                            }
                        }
                    }
                    return 0;
                });
                app.Execute(args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"error: {e}");
                Environment.Exit(-1);
            }
        }
    }
}
