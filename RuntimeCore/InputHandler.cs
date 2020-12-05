using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RuntimeCore {


    internal sealed class InputHandler : ILinkerBaseFields
    {

        private static readonly InputHandler.Body _body;
        private static readonly InputHandler.Initializer _initializer;
        private static readonly InputHandler.InvokeHandler _invokeHandler;
        
        static InputHandler()
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
            
            public void Invoke(String runArgument = null)
            {
                _body.Start();
                Console.WriteLine("ip in handler");
                //throw new NotImplementedException();
            }
            public void Invoke() => Invoke(String.Empty);
        }


        private sealed class Body : IModuleBody
        {
            private const Int32 Port = 50000;

            private readonly IPEndPoint _ipPoint = new (IPAddress.Parse("127.0.0.1"), Port);

            // create socket
            private readonly Socket _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            
            public void InputOutput(String command)
            {
                //var values = JsonSerializer.Deserialize<Dictionary<String, String>>(command.Replace('\'', '\"'));

                IOHandler<Dispatcher>.TInputInvoke(command.Replace('\'', '\"'));
                
                


                //     += values?["module_language"] switch
                // {
                //     "run_module" => _invokeHandler.SwitchInputAction("default_enhance"),
                //     "run_script" => _invokeHandler.SwitchInputAction("default_enhance"), 
                //     "build_script" => _invokeHandler.SwitchInputAction("default_enhance"),
                //     "get_build" => _invokeHandler.SwitchInputAction("default_enhance"),
                //     "update_executable" => _invokeHandler.SwitchInputAction("default_enhance"),
                //     "default_enhance" => _invokeHandler.SwitchInputAction("default_enhance"),
                //     _ => _invokeHandler.SwitchInputAction("default_enhance"),
                // };

                //Console.WriteLine($"-----------------{command}----------------------------------------");
            }

            public void Update()
            {
                Console.WriteLine(3);
                //InputOutput("");
                
                //Console.WriteLine(Port.ToString());
                //InputOutput(String.Empty);
                var handler = _listenSocket.Accept();
                
                // get message
                var builder = new StringBuilder();
                
                // message's bytes
                
                
                // buffer
                var data = new Byte[1024];
                
                do
                {
                    var bytes = handler.Receive(data);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (handler.Available > 0);
                //
                //
                InputOutput(builder.ToString());
                // // send response
                // var message = "your message has been sent";
                // data = Encoding.Unicode.GetBytes(message);
                // handler.Send(data);
                //
                
                // // close socket
                // handler.Shutdown(SocketShutdown.Both);
                // handler.Close();
            }
            
            public void Start()
            {
                Console.WriteLine($"{base.ToString()} has started!");
                _listenSocket.Bind(_ipPoint);
                
                // start to listen the socket
                
                _listenSocket.Listen(10);
                
                // try
                // {
                //     var handler = _listenSocket.Accept();
                // }
                // catch (Exception e)
                // {
                //     Console.WriteLine(e);
                // }
                //var handler = _listenSocket.Accept();
                
                Console.WriteLine("Server is up. Waiting for the response");

                //var c = (IModuleBody) this;
                
                IModuleBody.Controller(Update); // Run Update via new Thread
                
                //throw new NotImplementedException();

            }
            
            public String GetStr() => base.ToString();

            public void SetStr() => Console.WriteLine(base.ToString());

        }


        private sealed class InvokeHandler : IInvokeHandler
        {
        
            public Action SwitchInputAction(String command) =>
                command switch
                {
                    "GetName" => _body.SetStr,
                    _ => throw new Exception()
                };
        
            public Func<String> SwitchOutputAction(String command) =>
                command switch
                {
                    "GetName" => _body.GetStr,
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
}