using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RuntimeCore
{
    internal sealed class ModuleDispatcher : ILinkerBaseFields
    {
        
        private static readonly ModuleDispatcher.Body _body;
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
                Console.WriteLine("We have created ModuleDispatcher");
                //throw new NotImplementedException();
            }
            public void Invoke() => Invoke(String.Empty);
        }
        
        private class Body : IModuleBody
        {

            private static readonly Dictionary<string, string> ExecutableLanguage = new Dictionary<string, string>
            {
                {"python", "D:\\repo\\pythonProj\\Scripts\\python.exe"}
            };//key - name of Language, value - path to env
            
            private static readonly Dictionary<string, string> ExecutableModule= new Dictionary<string, string>
            {
                {"procPhoto.py", "D:\\ProjArtify\\ArtifyCore\\modules\\python\\main.py" }
            };//key - name of Module, value - path to module
            
            private readonly string _pathToModules;
            
            public void InputOutput(String command)
            {
                
                Console.WriteLine($"-----------------{command}----------------------------------------");
            }

            public void Update()
            {
                
            }
            
            public void Start()
            {

            }
            
            public String GetStr() => base.ToString();

            public void SetStr() => Console.WriteLine(base.ToString());
            
            internal string RunModuleAsync
            (
                string languageType = "python", 
                string moduleType="main.py", 
                string additionalArguments = "D:\\ProjArtify\\ArtifyCore\\Users\\Yura\\image2.png"
            )
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = ExecutableLanguage[languageType];
                start.Arguments = string.Format("{0} {1}", ExecutableLanguage[moduleType], additionalArguments);
                start.UseShellExecute = false;
            
                start.RedirectStandardOutput = true;
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();
                        Console.Write(result);
                    
                    }
                }


                return "234234";

            }
            
            internal string GetLanguageValue(string key)
            {
                return ExecutableLanguage[key];     
            }
            internal IEnumerable<String> GetListOfLanguages()
            {
                return ExecutableLanguage.Where(x => ExecutableLanguage.Values.Contains(x.Value)).Select(x => x.Key);
            }
            internal static void SetLanguageValue(string moduleName, string modulePath)
            {
                ExecutableLanguage.Add(moduleName, modulePath);
            }
            //

            //Set + Get Modules
            internal static void SetModuleValue(string languageName = "python", string fileName= "main.py")//register module in the dictionary
            {
                string[] fileEntries = Directory.GetFiles("D:\\ProjArtify\\ArtifyCore\\modules\\" + languageName);//all file in directory

                foreach (string file in fileEntries)
                {
                    if(file == "D:\\ProjArtify\\ArtifyCore\\modules\\" + languageName + "\\" + fileName)//check if the file is in a folder
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
        
        
        
        
        
  
        

        //Set + Get Languages
        
        //
    }

}

