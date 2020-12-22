using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;





namespace RuntimeCore

{
    internal class ModuleDispatcher : ILinkerBaseFields
    {
        internal static bool runModule = false;
        internal static readonly ModuleDispatcher.Body _body;
        private static readonly ModuleDispatcher.Initializer _initializer;
        private static readonly ModuleDispatcher.InvokeHandler _invokeHandler;
        



        static ModuleDispatcher()
        {
            _body = new Body();
            _initializer = new Initializer();
            _invokeHandler = new InvokeHandler();
            _initializer.Invoke("");
        }


        public void NewData<T>() where T : new()
        {

        }


        private class Initializer : IModuleInitializer, IInputOutputHandler
        {
            


            public void Invoke(String runArgument = null)
            {
                _body.Start();

            }
            public void Invoke() => Invoke(String.Empty);
        }

        internal class Body : IModuleBody
        {
            internal string pathToLanguages, pathToModules;
            public object JsonConvert { get; private set; }
            private static readonly Dictionary<string, string> ExecutableLanguage = new Dictionary<string, string>();


            private static readonly Dictionary<string, string> ExecutableModule = new Dictionary<string, string>();

            

            public void InputOutput(String command)
            {

                Console.WriteLine($"-----------------{command}----------------------------------------");
            }

            public void Update()
            {

            }
            private static async Task<string> RunServerAsync()
            {
                // Open the named pipe.
                var server = new NamedPipeServerStream("NPtes");

                Console.WriteLine("Waiting for connection...");
                
                await server.WaitForConnectionAsync();
                
                Console.WriteLine("Connected.");
                var br = new BinaryReader(server);
               
                string json = null;
                var a = 1;
                while (true)
                {
                    try
                    {
                        var len = (int)br.ReadUInt32();            // Read string length
                      
                        json ="{"+ '\u0022' + new string(br.ReadChars(len + 2));    // Read string
                        a++;
                        //Console.WriteLine(json);
                        

                        
                    }
                    catch (EndOfStreamException)
                    {
                        
                        break;                    // When client disconnects
                    }
                }

                Console.WriteLine("Client disconnected.");
                server.Close();
                server.Dispose();
                return json;
            }

            public void Start()
            {
                string path = @"InitializeDispatcherModule.txt";
                string json = null;
                string path2 = Directory.GetCurrentDirectory();
                
                try
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                       
                        json = sr.ReadToEnd();
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                JObject o = JObject.Parse(json);

                Console.WriteLine(o.ToString());
                pathToLanguages = Convert.ToString(o["Environments"]);
                pathToModules = Convert.ToString(o["Modules"]);
                
                string[] fileEntries1 = Directory.GetFiles(_body.pathToLanguages);//all file in directory
                string result; 
                foreach (string file in fileEntries1)
                {
                    result = Path.GetFileName(file);
                    if (result == "python.exe")
                    {
                        ExecutableLanguage.Add(result, file);
                        Console.WriteLine("File {0} successfully added", result);
                    }
                       
                }
                string[] fileEntries2 = Directory.GetDirectories(_body.pathToModules);//all file in directory
                
                foreach (string file in fileEntries2)
                {
                    result = Path.GetFileName(file);
                    string[] fileEntries3 = Directory.GetFiles(_body.pathToModules + "\\"+result);//all file in directory

                    foreach (string file1 in fileEntries3)
                    {
                        result = Path.GetFileName(file1);

                        ExecutableModule.Add(result, file1);
                        Console.WriteLine("File {0} successfully added", result);

                    }

                }
                


            }

            public String GetStr() => base.ToString();

            public void SetStr() => Console.WriteLine(base.ToString());

            internal async Task<string>  RunModuleAsync
            (
                string languageType = "python.exe",
                string moduleType = "main.py",
                string additionalArguments = "D:\\ProjArtify\\ArtifyCore\\Users\\Yura\\image2.png"
            )
            {
                Task<string> json = RunServerAsync();
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = ExecutableLanguage[languageType];
                start.Arguments = string.Format("{0} {1}", ExecutableModule[moduleType], additionalArguments);
                start.UseShellExecute = false;

                start.RedirectStandardOutput = true;
               
                Process process = Process.Start(start);
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    Console.WriteLine(line);
                    //do something with line
                }
                return await json;
            }

            internal string GetLanguage(string key)
            {
                return ExecutableLanguage[key];
            }
            internal IEnumerable<String> GetListOfLanguages()
            {
                return ExecutableLanguage.Where(x => ExecutableLanguage.Values.Contains(x.Value)).Select(x => x.Key);
            }
            internal static void SetLanguage(string languageName)
            {
                string[] fileEntries = Directory.GetFiles(_body.pathToLanguages);//all file in directory

                foreach (string file in fileEntries)
                {
                    if (file == _body.pathToLanguages + languageName)//check if the file is in a folder
                    {
                        if (!ExecutableModule.ContainsKey(languageName))//check if the file is in the dictionary
                        {
                            ExecutableLanguage.Add(languageName, file);
                            Console.WriteLine("File successfully added");
                        }
                    }
                }
            }
            //

            //Set + Get Modules
            internal static void SetModule(string languageName = "python", string fileName = "main.py")//register module in the dictionary
            {
                string[] fileEntries = Directory.GetFiles("D:\\ProjArtify\\ArtifyCore\\modules\\" + languageName);//all file in directory

                foreach (string file in fileEntries)
                {
                    if (file == _body.pathToModules + languageName + "\\" + fileName)//check if the file is in a folder
                    {
                        if (!ExecutableModule.ContainsKey(fileName))//check if the file is in the dictionary
                        {
                            ExecutableModule.Add(fileName, file);
                            Console.WriteLine("File successfully added");
                        }
                    }
                }
            }
            internal IEnumerable<String> GetListOfModules()
            {
                return ExecutableModule.Where(x => ExecutableModule.Values.Contains(x.Value)).Select(x => x.Key);
            }

        }


        private class InvokeHandler : IInvokeHandler
        {

            public Action SwitchInputAction(String command) =>
                command switch
                {
                    "GetName" => _body.SetStr,
                    "MockStart" => _body.Start,
                    _ => throw new Exception()
                };

            public Func<String> SwitchOutputAction(String command) =>
                command switch
                {
                    "GetName" => _body.GetStr,
                    _ => _body.GetStr
                };

        }


        public void InputInvoker(String command)
        {
            var action = _invokeHandler.SwitchInputAction(command);
            action();

        }

        public String OutputInvoker(String command)
        {
            var func = _invokeHandler.SwitchOutputAction(command);
            return func();
        } 
    }

}
//To run RunModuleAsync:
//Product product = new Product();
//product.NumberOfImages = 2;
//product.Image.Add("D:/ProjArtify/ArtifyCore/Users/Yura/image1.png");
//product.Image.Add("D:/ProjArtify/ArtifyCore/Users/Yura/image2.png");
//product.PathToModule = "enemy";
//string json = JsonConvert.SerializeObject(product);
//Console.WriteLine(json.Replace(@"""", @"\"""));
//var res = await ModuleDispatcher._body.RunModuleAsync(additionalArguments: json.Replace(@"""", @"\"""));
//Console.WriteLine(res);
//Console.ReadKey();

//internal class Product
//{
//    public int NumberOfImages { get; internal set; }
//    public List<string> Image { get; internal set; } = new List<string>();
//    public string PathToModule { get; internal set; }
//}