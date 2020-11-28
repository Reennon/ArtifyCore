using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuntimeCore;


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
        // !!! BE AWARE TO RUN IN SEPARATE THREAD !!!
        public static (String, String) RecompileScript(String script, String dllName
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
                {"System.Private.CoreLib", "System.Console", "System.Runtime"};
            
            START_PARAMS = new Object[] {new[] {"arg1", "arg2", "etc"}};

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
            var references = new List<MetadataReference>();
            //adding the core dll containing object and other classes
            
            // Precompiler Libs Analysis and Injection
            Array.ForEach(new Regex(@"((?:(?![using] ))+[A-z.]+[\;])").Matches(script).ToArray(),
                match =>
                {
                    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!,
                        match.ToString().TrimEnd(';') + ".dll")));
                });
            
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
                .ToArray();

            //for each using directive add a metadata reference to it
            references.AddRange(usings
                .Select(u => 
                    MetadataReference.CreateFromFile(
                        Path.Combine(assemblyPath!, u.Name + ".dll"))));


            compilation = compilation.AddReferences(references);


            using var ms = new MemoryStream();
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
                File.WriteAllBytes($@"{dllName}.dll", ms.ToArray());
                return ($@"{dllName}.dll", CreateJSONRuntimeConfig($"{dllName}.runtimeConfig", ASSEMBLY_NAME));
            }

            return ($"Start Error message - {result.Diagnostics} \n - Error Message end", String.Empty);

            String CreateJSONRuntimeConfig(String name, String assemblyName
                , String frameworkName = "Microsoft.NETCore.App"
                , String version = "5.0.0")
            {
                using var sw = new StreamWriter($"{name}.json");
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
            return command switch
            {
                _ => _body.Start
            };
        }

        public Func<String> SwitchOutputAction(String command)
        {
            return command switch
            {
                _ => _body.GetStr
            };
        }
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
