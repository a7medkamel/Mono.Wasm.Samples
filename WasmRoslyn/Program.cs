using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using WebAssembly;

namespace WasmRoslyn
{
    public class Program
    {
        static CompileService service;

        public static async void Main(JSObject compileLog, JSObject outputLog, JSObject csharpCode)
        {
            Console.WriteLine("Rolsyn test");

            var source = await GetSourceCode("RunClass.txt");
            csharpCode.SetObjectProperty("value", source);

            service = new CompileService(httpClient);

            return;
        }

        public static void Run(JSObject compileLog, JSObject outputLog, JSObject csharpCode)
        {

            var taCode = csharpCode.GetObjectProperty("value").ToString();


            Task.Run(() => CompileAndRun(taCode, compileLog, outputLog));
        }

        public static void CompileOnly(JSObject compileLog, JSObject outputLog, JSObject csharpCode)
        {

            var taCode = csharpCode.GetObjectProperty("value").ToString();


            Task.Run(() => Compile(taCode, compileLog, outputLog));
        }

        public static async Task Compile(string Code, JSObject compileLog, JSObject outputLog)
        {

            try
            {
                service.CompileLog = new List<string>();
                var type = await service.CompileSourceCode(Code);
                if (type != null)
                {
                    service.CompileLog.Add($"Exported type returned from assembly: {type}");
                }
                else
                {
                    service.CompileLog.Add("No exported types were found.");
                }
            }
            catch (Exception e)
            {
                service.CompileLog.Add(e.Message);
                service.CompileLog.Add(e.StackTrace);
                throw;
            }
            finally
            {
                compileLog.SetObjectProperty("innerText", string.Join("\r\n", service.CompileLog));
            }
        }


        public static async Task CompileAndRun(string Code, JSObject compileLog, JSObject outputLog)
        {

            try
            {
                service.CompileLog = new List<string>();
                var result = await service.CompileAndRun(Code);
                outputLog.SetObjectProperty("innerText", string.Join("\r\n", result));
            }
            catch (Exception e)
            {
                service.CompileLog.Add(e.Message);
                service.CompileLog.Add(e.StackTrace);
                throw;
            }
            finally
            {
                compileLog.SetObjectProperty("innerText", string.Join("\r\n", service.CompileLog));

            }
        }

        static HttpClient httpClient;
        static string BaseApiUrl = string.Empty;
        static string PathName = string.Empty;
        static void CheckHttpClient()
        {
            if (httpClient == null)
            {
                Console.WriteLine("Create  HttpClient");
                using (var window = (JSObject)WebAssembly.Runtime.GetGlobalObject("window"))
                using (var location = (JSObject)window.GetObjectProperty("location"))
                {
                    BaseApiUrl = (string)location.GetObjectProperty("origin");
                    PathName = (string)location.GetObjectProperty("pathname");
                    Console.WriteLine($"Base: {BaseApiUrl} ReferencePath: {PathName}");
                }
                httpClient = new HttpClient() { BaseAddress = new Uri(new Uri(BaseApiUrl), PathName) };
            }

        }

        private static async Task<String> GetSourceCode(string name)
        {
            Console.WriteLine($"Fetching: {name}");
            CheckHttpClient();

            var source = await httpClient.GetStringAsync(name).ConfigureAwait(false);
            return source;

        }


    }
}