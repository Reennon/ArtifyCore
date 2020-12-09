using System;
using System.Collections.Generic;

namespace RuntimeCore
{
    internal static partial class ModuleHandler
    {
        public static readonly Dictionary<Type, ILinkerBaseFields> Modules = new()
        {
            {typeof(Dispatcher), Dispatcher.GetInstance()},
            , {typeof(InputHandler), new InputHandler()},
            , { typeof(ModuleDispatcher), new ModuleDispatcher() }
            ,{typeof(CompileDispatcher), new CompileDispatcher()}
        };
    }
    
    internal static partial class IOHandler<T>
        where T : ILinkerBaseFields
    {

        internal static void TInputInvoke( String command) 
        {
            (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.InputInvoker(command);
            

        }   
        
        internal static String TIOutputInvoke(String command) 

        {
            return (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.OutputInvoker(command);


        }
        
        internal static void TJsonInvoke(String command, String json)
        {
            (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.JsonInput(command, json);
        }

    }
}