using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtifyCore
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
            
            // #nullable enable
            // var method = typeof(T).GetMethod("InputInvoker");
            // var generic = method?.MakeGenericMethod(typeof(String));
            // generic?.Invoke(typeof(T), new Object?[] {command});
            // #nullable disable
        }   
    
    
        internal static String TIOutputInvoke(T type, String command) 
        {
            return (ModuleHandler
                .Modules[typeof(T)] is T ? (T) ModuleHandler
                .Modules[typeof(T)] : default)?.OutputInvoker(command);
            
            //return type.OutputInvoker(command);
        
            // #nullable enable
            //         var method = typeof(T).GetMethod("OutputInvoker");
            //         var generic = method?.MakeGenericMethod(typeof(String));
            //         return generic?.Invoke(typeof(T), new Object?[] {command}) as String;
            // #nullable disable
        }  
        
    
    }
}