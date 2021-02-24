using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

            private readonly IPEndPoint _ipPoint = new (IPAddress.Parse("192.168.0.103"), Port);

            // create socket
            private readonly Socket _listenSocket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            
            private void InputOutput(String command)
            {
                new Task(() => { IOHandler<Dispatcher>.TInputInvoke(command.Replace('\'', '\"')); }).Start();

            }

            public void Update()
            {
                Console.WriteLine("New Update Cycle");

                var handler = _listenSocket.Accept();
                
                var builder = new StringBuilder();
                
                var data = new Byte[512];
                
                do
                {
                    var bytes = handler.Receive(data);
                    builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                } while (handler.Available > 0);

                InputOutput(builder.ToString());
            }
            
            public void Start()
            {
                Console.WriteLine($"{base.ToString()} has started!");
                _listenSocket.Bind(_ipPoint);

                _listenSocket.Listen(10);

                Console.WriteLine("Server is up. Waiting for the response");

                IModuleBody.Controller(Update); // Run Update via new Thread

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