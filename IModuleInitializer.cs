using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ArtifyCore
{
    
    
    
    public interface IModuleInitializer
    {

        //public void Invoke();



        public void Invoke(String args = "");
        public void Invoke();
        
        public void DefaultMethod()
        {
            Console.WriteLine("Hello");
        }
        
    }


    public interface IModuleSingle
    {
        public void Invoke();

    }
    public interface IModuleMultiple
    {
        public void Invoke();
    }
    
    public abstract class ModuleInitializer
    {
        
        //public void Invoke();
        public void Invoke<T>() where T : new()
        {
            //if (typeof(T).GetMethod("Invoke(String args)").DeclaringType == typeof(T)) { Invoke<T>(String.Empty); }

            typeof(T).GetMethods().Where(mi =>
            {
                var p = mi.GetParameters();
                if (p.Length != 1)
                {
                    
                    return false;

                }
                if (mi.ReturnType == typeof(void)){
                    Invoke<T>(String.Empty);
                    return true;
                }

                return false;

            });
        }

        public void Invoke<T>(String args) where T : new()
        {
            //if (typeof(T).GetMethod("Invoke()").DeclaringType == typeof(T)) { Invoke<T>(); }
            
            typeof(T).GetMethods().Where(mi =>
            {
                var p = mi.GetParameters();
                if (p.Length != 1)
                {
                    Invoke<T>();
                    return false;

                }
                if (mi.ReturnType == typeof(void)){
                    
                    return true;
                }

                return false;

            });
        }

        public void None<T>()
        {
            ;}
        
        public void DefaultMethod()
        {
            Console.WriteLine("Hello");
        }
        
    }

    

    public interface IInputOutputHandler
    {
        //public void Invoke(String ip);
    }
    
    public interface IModuleBody
    {
        public static void Controller(Action action, Int16 taskDelay = 1000, CancellationToken cancellationToken = new CancellationToken())
        {
            // new Task(() =>
            // {
            //     while (true)
            //     {
            //         action();
            //     }
            // }).Start();
            
            Task.Run( async () =>
            {
                while (true)
                {
                    //Console.WriteLine(1);
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Task Run Async Exception"+e.ToString());
                    }
                    await Task.Delay(taskDelay,cancellationToken);

                }
            },cancellationToken);
        }

        //public void InputOutput(String command);
        
        public void Update();
        public void Start();
    }

    public interface ILinkerBaseFields
    {
        //public String OutputInvoker();
        public void InputInvoker(String command);

        public String OutputInvoker(String command);

    }

    public interface IInvokeHandler
    {

        public Action SwitchInputAction(String command);

        public Func<String> SwitchOutputAction(String command);

    
    }
}