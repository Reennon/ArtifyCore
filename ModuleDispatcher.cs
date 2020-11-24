using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher
{
    internal class ModuleDispatcher
    {
        //Properties
        private static readonly Dictionary<string, string> _executableLanguage;//key - name of Language, value - path to env
        private static readonly Dictionary<string, string> _executableModule;//key - name of Module, value - path to module
        private readonly string _pathToModules;
        //

        //Create ModuleDispatcher
        static ModuleDispatcher()
        {

            _executableLanguage = new Dictionary<string, string>
            {
                {"python", "D:\\repo\\pythonProj\\Scripts\\python.exe"}
            };
            _executableModule = new Dictionary<string, string>
            {
                {"procPhoto.py", "D:\\ProjArtify\\ArtifyCore\\modules\\python\\main.py" }
            };
        }
        internal ModuleDispatcher()
        {
            Console.WriteLine("We have created ModuleDispatcher");
        }
        internal static ModuleDispatcher GetInstance { get { return Nested.instance; } }
        private class Nested
        {
            static Nested() { }
            internal static readonly ModuleDispatcher instance = new ModuleDispatcher();
        }
        //

        internal string RunModuleAsync
            (
            string languageType = "python", 
            string moduleType="main.py", 
            string additionalArguments = "D:\\ProjArtify\\ArtifyCore\\Users\\Yura\\image2.png"
            )
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = _executableLanguage[languageType];
            start.Arguments = string.Format("{0} {1}", _executableModule[moduleType], additionalArguments);
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

        //Set + Get Languages
        internal string GeLanguageValue(string key)
        {
            return _executableLanguage[key];     
        }
        internal IEnumerable<String> GetListOfLanguages()
        {
           return _executableLanguage.Where(x => _executableLanguage.Values.Contains(x.Value)).Select(x => x.Key);
        }
        internal static void SetLanguageValue(string moduleName, string modulePath)
        {
            _executableLanguage.Add(moduleName, modulePath);
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
                    if (!_executableModule.ContainsKey(fileName))//check if the file is in the dictionary
                    {
                        _executableModule.Add(fileName, file);
                        Console.WriteLine("File successfully added");
                    }
                }
            }
        }
        internal IEnumerable<String> GetListOfModules()
        {
            return _executableModule.Where(x => _executableModule.Values.Contains(x.Value)).Select(x => x.Key);
        }
        //
    }

}

