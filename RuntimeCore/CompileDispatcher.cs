using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using RuntimeCore;
using JsonSerializer = System.Text.Json.JsonSerializer;


internal sealed class CompileDispatcher : ILinkerBaseFields
{
    // ReSharper disable once InconsistentNaming
    private static readonly Body _body;
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private static readonly Initializer _initializer;
    // ReSharper disable once InconsistentNaming
    private static readonly InvokeHandler _invokeHandler;


    static CompileDispatcher()
    {
        _body = new Body();
        _initializer = new Initializer();
        _invokeHandler = new InvokeHandler();
        _initializer.Invoke("");

    }

    public void NewData<T>() where T : new()
    {
    }

    private sealed class Initializer : IModuleInitializer, IInputOutputHandler
    {
        // ReSharper disable once MethodOverloadWithOptionalParameter
        public void Invoke(String str = "")
        {
            _body.Start();
            Console.WriteLine($"{ToString()} has started!");
        }

        public void Invoke()
        {
            Invoke(String.Empty);
        }
    }

    private sealed class Body : IModuleBody
    {
        private (String, String) lastBuild;

        public (String, String) LastBuild => lastBuild;

        public void InputOutput(String command)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            Console.WriteLine($"{ToString()} has started!");
        }

        public String GetStr()
        {
            return String.Empty;
        }

        public void UpdateExecutable(String command)
        {
            var json = JsonSerializer.Deserialize<Dictionary<String, String>>(command);
            var newJson = new
            {
                Environments = json!["Environments"]
                , Modules = json["Modules"]
            };
            using var tw = new StreamWriter(@"InitializeDispatcherModule.txt");
            tw.Write(JsonSerializer.Serialize(newJson));
        }

        public void RunBuild(Object START_PARAMS = null)
        {
            using var fs = new FileStream(@$"{lastBuild.Item1}",FileMode.Open);
            fs.Seek(0, SeekOrigin.Begin);
            var context = AssemblyLoadContext.Default;
            var assembly = context.LoadFromStream(fs);
            #nullable enable
            Console.WriteLine(assembly.EntryPoint?.Invoke(null, START_PARAMS as Object?[]));
            #nullable disable
        }
        

        public String GetLastBuild() =>
            LastBuild.Item1 + ' ' + lastBuild.Item2;
        public String GetLastDll() =>
            LastBuild.Item1;
        
        public String GetLastConfig() =>
            lastBuild.Item2;


        private void CopyNotCsFiles()
        {
            CopyFolder(new DirectoryInfo(@"..\..\..\Resources"), Directory.CreateDirectory(@"Release\Resources"));

            static void CopyFolder(DirectoryInfo source, DirectoryInfo target) {
                Array.ForEach(source.GetDirectories()
                    , dir => CopyFolder(dir, target.CreateSubdirectory(dir.Name)));
                Array.ForEach(source.GetFiles().ToArray().Where(file=>file.Extension!=".cs").ToArray()
                    , file => file.CopyTo(Path.Combine(target.FullName, file.Name), true));
            }
        }

        public (String, String) RunRecompilation(String dllName
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once RedundantAssignment
            , Object START_PARAMS = null)
        {
            var script = String.Empty;
            Array.ForEach(
                new DirectoryInfo(@"..\..\..\..\Scripts")
                    .GetFiles("*.cs")
                , file
                    => script += File.ReadAllText($@"..\..\..\..\Scripts\{file.Name}"));
            return RecompileScript(
                String.Join("", new Regex(@"(((using )+[A-z.]+[\;]))").Matches(script)
                    .Select(elem => elem.Value).ToArray()
                    .Distinct())
                + Regex.Replace(script, @"(((using )+[A-z.]+[\;]))", "")
                , dllName
                , NECESSARY_DLLS
                , ASSEMBLY_NAME
                , UNSAFE_CODE
                , START_PARAMS
            );
        }
        
        public async Task<(String,String)> RunRecompilationAsync(String dllName
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once RedundantAssignment
            , Object START_PARAMS = null)
        {
            var script = String.Empty;
            Console.WriteLine(ASSEMBLY_NAME);
            Array.ForEach(
                new DirectoryInfo(@"..\..\..\..\Scripts")
                    .GetFiles("*.cs")
                , file
                    => script += File.ReadAllText($@"..\..\..\..\Scripts\{file.Name}"));

            var task = new Task<(String, String)>( () =>
            {
                return RecompileScript(
                    String.Join("", new Regex(@"(((using )+[A-z.]+[\;]))").Matches(script)
                        .Select(elem => elem.Value).ToArray()
                        .Distinct())
                    + Regex.Replace(script, @"(((using )+[A-z.]+[\;]))", "")
                    , dllName
                    , NECESSARY_DLLS
                    , ASSEMBLY_NAME
                    , UNSAFE_CODE
                    , START_PARAMS
                );
            });
            task.Start();

            return await task;
        }
        
        
        // !!! BE AWARE TO RUN IN SEPARATE THREAD !!!
        private (String, String) RecompileScript(String script, String dllName
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once RedundantAssignment
            , Object START_PARAMS = null)
        {
            NECESSARY_DLLS ??= new List<String>()
                {"System.Private.CoreLib", "System.Console", "System.Runtime", "mscorlib"};
            var LIBS_NO_MAP = new List<String>()
                {"System.Collections.Specialized", "System.Collections.Generic"};
            var NAMESPACES = new List<String>() { };
            
            START_PARAMS = new Object[] {new[] {"arg1", "arg2", "etc"}};
            //Console.WriteLine(script);
            var syntaxTree = CSharpSyntaxTree.ParseText(script);

            //creating options that tell the compiler to output a console application
            var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Debug, allowUnsafe: UNSAFE_CODE);

            //creating the compilation
            var compilation = CSharpCompilation.Create(assemblyName: ASSEMBLY_NAME, options: options);

            //adding the syntax tree
            compilation = compilation.AddSyntaxTrees(syntaxTree);

            //getting the local path of the assemblies
            var assemblyPath = Path.GetDirectoryName(typeof(Object).Assembly.Location);

            // Add Namespaces
            Array.ForEach(new Regex(@"\s*namespace\s*(\S*)\s*{").Matches(script)
                .Select(match=>match.Groups[1].Value).ToArray(), NAMESPACES.Add);
            
            
            
            var references = new Regex(@"\s*using\s*(\S*)\s*;").Matches(script)
                .Select(match => match.Groups[1].Value)
                .Except(LIBS_NO_MAP)
                .ToArray()
                .Except(NAMESPACES)
                .ToArray()
                .Select(match
                    => MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, match + ".dll")))
                    .Cast<MetadataReference>()
                    .ToList();
            //adding the core dll containing object and other classes
            
            // Precompiler Libs Analysis and Injection



            NECESSARY_DLLS.ForEach(library 
                => references.Add(
                    MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, $"{library}.dll"))));
            
            /*
             Original Libs below \\ // are Refactored to LINQ ex above ^
             Libs Start
             // Necessary Libs are listed below
             references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")));
             references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Console.dll")));
             references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
             // Unnecessary Lib are listed below
             //references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.IO.FileSystem.dll"))); 
             Libs End
            */



            //gathering all using directives in the compilation
            
            // ReSharper disable once IdentifierTypo
            var usings = compilation.SyntaxTrees
                .Select(tree => tree.GetRoot().DescendantNodes().OfType<UsingDirectiveSyntax>()).SelectMany(s => s)
                .Where(u =>
                    !LIBS_NO_MAP.Contains(u.Name.ToString()) && !NAMESPACES.Contains(u.Name.ToString())).ToArray();

            //for each using directive add a metadata reference to it
            references.AddRange(usings
                .Select(u => 
                    MetadataReference.CreateFromFile(
                        Path.Combine(assemblyPath!, u.Name + ".dll"))));


            compilation = compilation.AddReferences(references);


            using var ms = new MemoryStream();
            ms.Flush();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                
                foreach (var diagnostic in failures)
                    Console.Error.WriteLine("{0}: {1}, {2}", diagnostic.Id, diagnostic.GetMessage(),
                        diagnostic.Location);
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                var context = AssemblyLoadContext.Default;
                var assembly = context.LoadFromStream(ms);
                
                #nullable enable
                Console.WriteLine(assembly
                    .EntryPoint?
                    .Invoke(
                        null
                        ,(Object?[]?) START_PARAMS));
                #nullable disable
                Directory.CreateDirectory("Release");
                File.WriteAllBytes($@"Release\{dllName}.dll", ms.ToArray());
                
                CopyNotCsFiles();

                return ($@"{dllName}.dll", CreateJSONRuntimeConfig($@"{dllName}.runtimeConfig", ASSEMBLY_NAME));
            }
            return ($"Start Error message - {result.Diagnostics} \n - Error Message end", String.Empty);

            String CreateJSONRuntimeConfig(String name, String assemblyName
                , String frameworkName = "Microsoft.NETCore.App"
                , String version = "5.0.0")
            {
                using var sw = new StreamWriter($@"Release\{name}.json");
                sw.Write("" +
                         "{" +
                            "\"runtimeOptions\": {" +
                               "\"tfm\":"+'\"' +assemblyName +'\"' +
                               ",\"framework\": {" +
                                   "\"name\": "+'\"'+frameworkName+'\"'+
                                   ",\"version\": "+'\"'+ version +'\"'+
                               "}" +
                            "} " +
                         "}");
                return $"{name}.json";
            }
        }
    }

    private sealed class InvokeHandler : IInvokeHandler
    {
        public Action SwitchInputAction(String command)
        {
            throw new NotImplementedException();
        }
        
        public Func<String> SwitchOutputAction(String command)
        {
            var json = JsonSerializer.Deserialize<Dictionary<String, String>>(command);
            return json!["command"] switch
            {
                "get_build" => _body.GetLastBuild
                , _ => throw new Exception($"{json["command"]} is not an output command")
            };
        }
    }

    public void InputInvoker(String command)
    {
        var json = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(command);
        ;
        switch (json!["command"].ToString())
        {
            case "run_build":
            {
                _body.RunBuild(json["START_PARAMS"]);
                break;
            }
            case "build":
            {
                Console.WriteLine(_body.RunRecompilationAsync(json["dllName"]
                    , json["NECESSARY_DLLS"]?.Split(' ').ToList()
                    , json["ASSEMBLY_NAME"] ??= (DateTime.Now.Date.ToString()+DateTime.Now.TimeOfDay.ToString()+DateTime.Now.Millisecond.ToString()+"Styopa_blyat_dai_imya_suka_namespace_new").Replace(' ','_').Replace(',', '_').Replace('.','_').Replace(':','_')
                    , json["UNSAFE_CODE"]
                    , json["START_PARAMS"]?.Split(' ').ToList<Object>()));
                break;
            }
            case "update_executable":
            {
                _body.UpdateExecutable(json["language_exe"]);
                break;
            }
            default:
            {
                Console.WriteLine(json!["command"].ToString());
                throw new Exception($"{json["command"]} is not an input command");
            }
        }

        //var action = _invokeHandler.SwitchInputAction(command);
        //action();
    }

    public String OutputInvoker(String command)
    {
        var func = _invokeHandler.SwitchOutputAction(command);
        return func();
    }
}
