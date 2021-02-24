using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuntimeCore;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;


Dispatcher.GetInstance();
Console.ReadKey();

//var c = new Dispatcher();
//Array.ForEach(ModuleHandler.Modules.ToArray(),x=>x.GetType());

//var c = Dispatcher.GetInstance();
//-Dispatcher.GetInstance();
// Console.WriteLine(c.GetHashCode());
// Console.WriteLine(ModuleHandler.Modules[typeof(Dispatcher)].GetHashCode());
// Console.WriteLine(c.GetHashCode());
//-Console.ReadKey();
//Task.WaitAll();

internal sealed class Dispatcher : ILinkerBaseFields
{
    private static readonly Body _body;
    private static readonly Initializer _initializer;
    private static readonly InvokeHandler _invokeHandler;
    private static Dispatcher _self;

    private Dispatcher()
    {
    }

    public static Dispatcher GetInstance()
    {
        return _self ??= new Dispatcher();
    }

    static Dispatcher()
    {
        _body = new Body();
        _initializer = new Initializer();
        _invokeHandler = new InvokeHandler();
        _initializer.Invoke("");

        IOHandler<InputHandler>.TInputInvoke("GetName");
        IOHandler<InputHandler>.TInputInvoke("GetName");
        //IOHandler<InputHandler>.TInputInvoke(ArtifyCore.ModuleHandler.Modules[typeof(InputHandler)] as InputHandler, "GetName");

        // IOHandler<InputHandler>
        //     .TInputInvoke(
        //         // ArtifyCore
        //         // .ModuleHandler
        //         // .Modules[typeof(InputHandler)] 
        //         //     as InputHandler
        //         // ,
        //         "GetName");

        //_inputHandler = new InputHandler();
        //Console.WriteLine(typeof(Dispatcher)+"!");
        //throw new Exception();
    }

    public void NewData<T>() where T : new()
    {
    }

    private sealed class Initializer : IModuleInitializer, IInputOutputHandler
    {
        public void Invoke(String str = "")
        {
            _body.Start();
            Console.WriteLine($"{ToString()} has started!");
            //throw new NotImplementedException();
        }

        public void Invoke()
        {
            Invoke(String.Empty);
        }
    }

    private sealed class Body : IModuleBody
    {
        //public void Controller();

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
            //throw new NotImplementedException();
        }

        public String GetStr()
        {
            return String.Empty;
        }
    }

    private sealed class InvokeHandler : IInvokeHandler
    {
        public Action SwitchInputAction(String command)
        {
            try
            {
                IOHandler<CompileDispatcher>.TInputInvoke(command);
            }
            catch (JsonException e)
            {
                Console.WriteLine(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return (Action) new Object();
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

        if (!command.StartsWith('{')) throw new Exception("not a json file");

        var json = JsonConvert.DeserializeObject<Dictionary<String,dynamic>>(command);
        
        /*
         * !!! Compile Dispatcher's routes !!!
         */
        if (json!["command"] == "build" 
            || json!["command"]  == "run_build" 
            || json!["command"]  == "update_executable")
        {
            IOHandler<CompileDispatcher>.TInputInvoke(json!["command"] switch 
            {
                "run_build" => JsonSerializer.Serialize(
                    new
                    {
                        command = "run_build"
                        , userId = json["userId"]
                    })
                , "build" => JsonSerializer.Serialize(
                    new
                    {
                        command = "build"
                        , userId = json["userId"]
                        , dllName = json["dllName"]
                        , path = json["path"]
                        , NECESSARY_DLLS = json["NECESSARY_DLLS"]
                        , ASSEMBLY_NAME = json["ASSEMBLY_NAME"]
                        , UNSAFE_CODE = json["UNSAFE_CODE"]
                    })
                , "update_executable" => JsonSerializer.Serialize(
                    new
                    {
                        command = "update_executable"
                        , userId = json["userId"]
                        , Environments = json["Environments"]
                        , Modules = json["Modules"].ToString()
                    })
                , "post_build" => JsonSerializer.Serialize(
                    new
                    {
                        command = "post_build"
                        , userId = json["userId"]
                        , adress = json["ADDRESS"] 
                        , mediaType = json["MEDIA_TYPE"]
                        , configureAwait = json["CONFIGURE_AWAIT"]
                    })
                , _ => JsonSerializer.Serialize(
                    new
                    {
                        command = "default_enhance"
                    })
            });
                
        }

        
        /*
         *  !!! Output Handler's routes !!!
         */
        else if (json!["command"] == "return_build"
                 || json["command"] == "return_error")
        {
            IOHandler<OutputHandler>.TInputInvoke(json!["command"] switch
            {
                "return_build" => JsonSerializer.Serialize( new
                {
                    command = "post_build"
                    , data = json["data"]
                    , userId = json["userId"]
                    , ADDRESS = json.ContainsKey("ADDRESS")? json["ADDRESS"] : null
                    , MEDIA_TYPE = json.ContainsKey("MEDIA_TYPE")? json["MEDIA_TYPE"] : null
                    , CONFIGURE_AWAIT = json.ContainsKey("CONFIGURE_AWAIT")? json["CONFIGURE_AWAIT"] : null
                })
                , "return_error" => JsonSerializer.Serialize( new
                {
                    command = "post_error"
                    , errorMessage = json["data"]
                    , userId = json["userId"]
                    , ADDRESS = json.ContainsKey("ADDRESS")? json["ADDRESS"] : null
                    , MEDIA_TYPE = json.ContainsKey("MEDIA_TYPE")? json["MEDIA_TYPE"] : null
                    , CONFIGURE_AWAIT = json.ContainsKey("CONFIGURE_AWAIT")? json["CONFIGURE_AWAIT"] : null
                })
                , _ => JsonSerializer.Serialize( new
                {
                    command = "post_error"
                    , errorMessage = new
                    {
                        data = "JSON inner core unparsed exception"
                        , errorCode = 0
                    }
                })
            });
        }
        else
        {
            IOHandler<CompileDispatcher>.TIOutputInvoke(json["command"] switch
            {
                "get_build" => JsonSerializer.Serialize(
                    new
                    {
                        command = "get_build" 
                        , START_PARAMS = json.ContainsKey("START_PARAMS") ? json["START_PARAMS"] : null
                    })
                , _ => JsonSerializer.Serialize(
                    new
                    {
                        command = "default_enhance"
                    })
            });
        }
        
    }

    public String OutputInvoker(String command)
    {
        var func = _invokeHandler.SwitchOutputAction(command);
        return func();
    }
}

internal static partial class ModuleHandler
{
}