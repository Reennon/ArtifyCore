using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using System.Text.Json.Serialization;
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


        private void CopyNotCsFiles(String userId)
        {
            CopyFolder(
                new DirectoryInfo(@"..\..\..\..\Resources").Exists
                    ? new DirectoryInfo(@"..\..\..\..\Resources")
                    : Directory.CreateDirectory(@"..\..\..\..\Resources"),
                Directory.CreateDirectory(@$"Release\{userId}\Resources"));
            //CopyFolder(new DirectoryInfo(@"..\..\..\..\Modules"), Directory.CreateDirectory(@"Release\Modules"));

            static void CopyFolder(DirectoryInfo source, DirectoryInfo target) {
                Array.ForEach(source.GetDirectories()
                    , dir => CopyFolder(dir, target.CreateSubdirectory(dir.Name)));
                Array.ForEach(source.GetFiles().ToArray().Where(file=>file.Extension!=".cs").ToArray()
                    , file => file.CopyTo(Path.Combine(target.FullName, file.Name), true));
            }
        }

        public String RunRecompilation(String dllName
            , String userId
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
            // ReSharper disable once InconsistentNaming
            // ReSharper disable once RedundantAssignment
            //, Object START_PARAMS = null
            //
            )
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
                , userId
                , NECESSARY_DLLS
                , ASSEMBLY_NAME
                , UNSAFE_CODE
                //, START_PARAMS
            );
        }
        
        public async Task<String> RunRecompilationAsync(String dllName
            , String userId
            , String path
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
        )
        {
            var script = String.Empty;
            var pathPrefix = @"..\..\..\..\..\ArtifyAPI";
            //Console.WriteLine(ASSEMBLY_NAME);
            Array.ForEach(
                new DirectoryInfo(@$"{pathPrefix}\{path}\Scripts")
                    .GetFiles("*.cs")
                , file
                    => script += File.ReadAllText($@"{pathPrefix}\{path}\Scripts\{file.Name}"));

            var task = new Task<String>( () =>
            {
                return RecompileScript(
                    String.Join("", new Regex(@"(((using )+[A-z.]+[\;]))").Matches(script)
                        .Select(elem => elem.Value).ToArray()
                        .Distinct())
                    + Regex.Replace(script, @"(((using )+[A-z.]+[\;]))", "")
                    , dllName
                    , userId
                    , NECESSARY_DLLS
                    , ASSEMBLY_NAME
                    , UNSAFE_CODE
                );
            });
            task.Start();

            return await task;
        }
        
        
        // !!! BE AWARE TO RUN IN SEPARATE THREAD !!!
        private String RecompileScript(String script, String dllName
            , String userId = "user"
            // ReSharper disable once InconsistentNaming
            , List<String> NECESSARY_DLLS = null
            // ReSharper disable once InconsistentNaming
            , String ASSEMBLY_NAME = "assemblyName"
            // ReSharper disable once InconsistentNaming
            , Boolean UNSAFE_CODE = false
        )
        {
            Console.WriteLine((DateTime.Now.Date + DateTime.Now.TimeOfDay + DateTime.Now.Millisecond.ToString() + "_Assembly")
                .Replace(' ', '_').Replace(',', '_').Replace('.', '_').Replace(':', '_') + userId +"start recompile");
            
            NECESSARY_DLLS ??= new List<String>()
                {"System.Private.CoreLib", "System.Console", "System.Runtime", "mscorlib"};
            // ReSharper disable once InconsistentNaming
            var LIBS_NO_MAP = new List<String>()
                {"System.Collections.Specialized", "System.Collections.Generic"};
            // ReSharper disable once InconsistentNaming
            var NAMESPACES = new List<String>() { };
            
            
            //START_PARAMS = new Object[] {new[] {"arg1", "arg2", "etc"}};
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
            Console.WriteLine(3);
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
                /*var assembly = context.LoadFromStream(ms);
                #nullable enable
                Console.WriteLine(assembly
                    .EntryPoint?
                    .Invoke(
                        null
                        ,(Object?[]?) START_PARAMS));*/
                #nullable disable
                
                
                /*var assembly =
                    context.LoadFromStream(new FileStream($@"Release\{userId}\{dllName}.dll", FileMode.Open));
                Console.WriteLine(assembly
                    .EntryPoint?
                    .Invoke(
                        null
                        ,(Object?[]?) START_PARAMS));*/
                
                Console.WriteLine("Compilation Successful");
                Directory.CreateDirectory("Release");
                Directory.CreateDirectory(@$"Release\{userId}");
                File.WriteAllBytes($@"Release\{userId}\{dllName}.dll", ms.ToArray());
                CopyNotCsFiles(userId);
                CreateJSONRuntimeConfig($@"Release\{userId}\{dllName}.runtimeConfig", ASSEMBLY_NAME);
                return JsonSerializer.Serialize(
                    new
                    {
                        command = "buildResult"
                        , result = "Success"
                        , value = $@"{Assembly.GetExecutingAssembly().Location}\..\Release\{userId}\{dllName}.dll"
                        , userId
                    });
                //return ($@"{userId}\{dllName}.dll", CreateJSONRuntimeConfig($@"{userId}\{dllName}.runtimeConfig", ASSEMBLY_NAME));
            }
            return JsonSerializer.Serialize(
                new
                {
                    command = "buildResult"
                    , result = "Failure"
                    , value = $"Error message \n--- Start >>> \n{result.Diagnostics}\n<<< End --- \nError Message"
                    , userId
                });

            String CreateJSONRuntimeConfig(String path, String assemblyName
                , String frameworkName = "Microsoft.NETCore.App"
                , String version = "5.0.0")
            {
                using var sw = new StreamWriter($@"{path}.json");
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
                return $"{path}.json";
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
        
        switch (json!["command"].ToString())
        {
            case "run_build":
            {
                _body.RunBuild(json["START_PARAMS"]);
                break;
            }
            case "build":
            {
                /*var userId = json["userId"].ToString() as String;
                _body.RunRecompilationAsync(json["dllName"]
                    , userId
                    , json["NECESSARY_DLLS"]
                    , json["ASSEMBLY_NAME"] ??=
                        (DateTime.Now.Date + DateTime.Now.TimeOfDay + DateTime.Now.Millisecond.ToString() + "_Assembly")
                        .Replace(' ', '_').Replace(',', '_').Replace('.', '_').Replace(':', '_')
                    , json["UNSAFE_CODE"]
                );
                */
                OutputInvoker(
                    JsonConvert.SerializeObject(
                        new
                        {
                            to = "Dispatcher"
                            , command = "return_build"
                            , value = _body.RunRecompilationAsync( json["dllName"]
                                , json["userId"].ToString() as String
                                , json["path"]
                                , json["NECESSARY_DLLS"]
                                , json["ASSEMBLY_NAME"] ??=
                                    (DateTime.Now.Date + DateTime.Now.TimeOfDay + DateTime.Now.Millisecond.ToString() + "_Assembly")
                                    .Replace(' ', '_').Replace(',', '_').Replace('.', '_').Replace(':', '_')
                                , json["UNSAFE_CODE"]).Result
                                
                        }));
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
        //if(command.StartsWith("{"))
        var json = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(command);
        var value = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(json["value"]);
        switch(json!["command"].ToString()){
            case "return_build":
            {
                //var json = JsonConvert.DeserializeObject<Dictionary<String,dynamic>>(command);
                IOHandler<Dispatcher>.TInputInvoke(
                    value["result"] switch
                    {
                        "Success" => JsonSerializer.Serialize(
                            new
                            {
                                command = "return_build", data = value["value"], userId = value["userId"]
                            }),
                        "Failure" => JsonSerializer.Serialize(
                            new
                            {
                                command = "return_error", errorMessage = value["value"], userId = value["userId"]
                            }),
                        _ => JsonSerializer.Serialize(
                            new
                            {
                                command = "_"
                            })
                    });
                break;
            };
            default:
            {
                Console.WriteLine(value!["result"].ToString());
                throw new Exception($"{json["command"]} is not an input command");
            }
        }
        return String.Empty;
        
        
    }
}
