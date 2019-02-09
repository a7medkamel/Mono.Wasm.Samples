using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace WasmRoslyn
{
    public class CompileService
    {
        private readonly HttpClient _http;
        public List<string> CompileLog { get; set; }
        private List<MetadataReference> references { get; set; }


        public CompileService(HttpClient http)
        {
            _http = http;
        }

        public async Task Init()
        {
            if (references == null)
            {
                references = new List<MetadataReference>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    references.Add(await GetFileStream($"managed/{assembly.GetName().Name}.dll"));
                }
            }
        }


        public async Task<Type> CompileSourceCode(string code)
        {

            Console.WriteLine("Compile assembly");
            var assembly = await Compile(code);
            if (assembly != null)
            {
                CompileLog.Add("Searching for first exported type.");
                return assembly.GetExportedTypes().FirstOrDefault();
            }
            return null;

        }

        public async Task<Assembly> Compile(string code)
        {
            await Init();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(LanguageVersion.Latest));
            foreach (var diagnostic in syntaxTree.GetDiagnostics())
            {
                CompileLog.Add(diagnostic.ToString());
            }

            if (syntaxTree.GetDiagnostics().Any(i => i.Severity == DiagnosticSeverity.Error))
            {
                CompileLog.Add("Parse SyntaxTree Error!");
                return null;
            }

            CompileLog.Add("Parse SyntaxTree Success");

            CSharpCompilation compilation = CSharpCompilation.Create("WasmRoslyn.Demo", new[] { syntaxTree },
                references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (MemoryStream stream = new MemoryStream())
            {
                EmitResult result = compilation.Emit(stream);

                foreach (var diagnostic in result.Diagnostics)
                {
                    CompileLog.Add(diagnostic.ToString());
                }

                if (!result.Success)
                {
                    CompileLog.Add("Compilation error");
                    return null;
                }

                CompileLog.Add("Compilation success!");
                Assembly assemby = AppDomain.CurrentDomain.Load(stream.ToArray());
                return assemby;
            }

        }


        public async Task<string> CompileAndRun(string code)
        {
            await Init();

            var assemby = await this.Compile(code);
            if (assemby != null)
            {
                var type = assemby.GetExportedTypes().FirstOrDefault();
                var methodInfo = type.GetMethod("Run");
                var instance = Activator.CreateInstance(type);
                return (string)methodInfo.Invoke(instance, new object[] { new object[] { "Hic sunt dracones", 12 } });
            }

            return null;
        }

        private async Task<MetadataReference> GetFileStream(string name)
        {
            Console.WriteLine($"Fetching: {name}");

            MetadataReference mr = null;
            using (var stream = await _http.GetStreamAsync(name).ConfigureAwait(false))
            using (var outputStream = new MemoryStream())
            {
                await stream.CopyToAsync(outputStream).ConfigureAwait(false);
                // Make sure to set the position to 0
                outputStream.Position = 0;
                mr = MetadataReference.CreateFromStream(outputStream);
            }
            return mr;

        }

    }
}