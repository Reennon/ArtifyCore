using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using RuntimeCore;




Dispatcher.GetInstance();

Console.ReadKey();





internal sealed class Dispatcher : ILinkerBaseFields
{
    private static readonly Dispatcher.Body _body;
    private static readonly Dispatcher.Initializer _initializer;
    private static readonly Dispatcher.InvokeHandler _invokeHandler;
    private static Dispatcher _self;


    private Dispatcher()
    {
    }

    public static Dispatcher GetInstance()
        => _self ??= new Dispatcher();


    static Dispatcher()
    {
        _body = new Body();
        _initializer = new Initializer();
        _invokeHandler = new InvokeHandler();
        _initializer.Invoke("");

        IOHandler<InputHandler>.TInputInvoke("GetName");
        IOHandler<ModuleDispatcher>.TInputInvoke("GetName");
        


        // IOHandler<InputHandler>
        //     .TInputInvoke(
        //         // ArtifyCore
        //         // .ModuleHandler
        //         // .Modules[typeof(InputHandler)] 
        //         //     as InputHandler
        //         // ,
        //         "GetName");


     
    }

    public void NewData<T>() where T : new()
    {
    }

    private sealed class Initializer : IModuleInitializer, IInputOutputHandler
    {
        public void Invoke(String str = "")
        {
            _body.Start();
            Console.WriteLine($"{base.ToString()} has started!");
           
        }

        public void Invoke() => Invoke(String.Empty);
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
            Console.WriteLine($"{base.ToString()} has started!");
            
        }

        public String GetStr() => String.Empty;
    }

    private sealed class InvokeHandler : IInvokeHandler
    {
        public Action SwitchInputAction(String command) =>
            command switch
            {
                _ => _body.Start
            };

        public Func<String> SwitchOutputAction(String command) =>
            command switch
            {
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


internal static partial class ModuleHandler
{
}