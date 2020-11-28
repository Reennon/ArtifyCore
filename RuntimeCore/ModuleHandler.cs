using System;
using System.Collections.Generic;

namespace RuntimeCore
{
    internal static partial class ModuleHandler
    {
        public static readonly Dictionary<Type, ILinkerBaseFields> Modules = new()
        {
            {typeof(Dispatcher), Dispatcher.GetInstance()},
            {typeof(InputHandler), new InputHandler()}
        };
    }
    
    internal static partial class IOHandler<T>
        where T : ILinkerBaseFields
    {
        //private static readonly ILinkerBaseFields LinkedTo = typeof(T).;

        internal static void TInputInvoke( String command) 
        {
            (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.InputInvoker(command);
            
        }   
    
    
        internal static String TIOutputInvoke(T type, String command) 
        {
            return (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.OutputInvoker(command);
            
        }  
        
    
    }
}