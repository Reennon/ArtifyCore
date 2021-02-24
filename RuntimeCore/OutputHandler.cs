using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace RuntimeCore {


    internal sealed class OutputHandler : ILinkerBaseFields
    {

        private static readonly OutputHandler.Body _body;
        private static readonly OutputHandler.Initializer _initializer;
        private static readonly OutputHandler.InvokeHandler _invokeHandler;
        
        static OutputHandler()
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
            }
            public void Invoke() => Invoke(String.Empty);
        }


        private sealed class Body : IModuleBody
        {
            private static readonly HttpClient Client = new HttpClient();
            private const String ApiRoute = "http://192.168.0.103:4000";
            private const String NewEndpointRoute = ApiRoute+"/artify/new/";
            
            public void InputOutput(String command)
            {

                //var content = new FormUrlEncodedContent(JsonConvert.DeserializeObject<Dictionary<String, String>>(command));

                
                Client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Post
                    , RequestUri = new(NewEndpointRoute)
                    , Content = new StringContent(command, Encoding.UTF8, "json")
                }).ConfigureAwait(true).GetAwaiter().GetResult().EnsureSuccessStatusCode();


            }

            public void Update()
            {
                

            }
            
            public void Start()
            {
                

            }
            
            public String GetStr() => base.ToString();

            public void SetStr() => Console.WriteLine(base.ToString());

            public async Task<HttpResponseMessage> PostBuildAsync(
                String data
                , String userId = "user"
                // ReSharper disable once StringLiteralTypo
                // ReSharper disable once InconsistentNaming
                , String ADDRESS = NewEndpointRoute
                // ReSharper disable once InconsistentNaming
                , String MEDIA_TYPE = "application/json"
                // ReSharper disable once InconsistentNaming
                , Boolean? CONFIGURE_AWAIT = false)
            {
                ADDRESS ??= NewEndpointRoute;
                MEDIA_TYPE ??= "application/json";
                CONFIGURE_AWAIT ??= false;
                return await Client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Post, RequestUri = new Uri(ADDRESS+userId),
                    Content = new StringContent(data, Encoding.UTF8, MEDIA_TYPE)
                }).ConfigureAwait(CONFIGURE_AWAIT.Value);
            }

            public async Task<HttpResponseMessage> PostErrorAsync(String data
                , String userId = "user"
                // ReSharper disable once StringLiteralTypo
                // ReSharper disable once InconsistentNaming
                , String ADDRESS = "http://127.0.0.1:4000/artify/error/"
                // ReSharper disable once InconsistentNaming
                , String MEDIA_TYPE = "json"
                // ReSharper disable once InconsistentNaming
                , Boolean? CONFIGURE_AWAIT = false)
            {
                ADDRESS ??= ApiRoute+@"/artify/error";
                MEDIA_TYPE ??= "application/json";
                CONFIGURE_AWAIT ??= false;
                return await Client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Post, RequestUri = new Uri(ADDRESS+userId),
                    Content = new StringContent(data, Encoding.UTF8, MEDIA_TYPE)
                }).ConfigureAwait(CONFIGURE_AWAIT.Value);
            }

            public async Task<HttpResponseMessage> WarnDependencyException(String value
                , String userId = "user"
                // ReSharper disable once StringLiteralTypo
                // ReSharper disable once InconsistentNaming
                , String ADDRESS = "http://127.0.0.1:4000/artify/new/"
                // ReSharper disable once InconsistentNaming
                , String MEDIA_TYPE = "json"
                // ReSharper disable once InconsistentNaming
                , Boolean? CONFIGURE_AWAIT = false)
            {
                ADDRESS ??= "http://127.0.0.1:4000/artify/new/";
                MEDIA_TYPE ??= "application/json";
                CONFIGURE_AWAIT ??= false;
                return await Client.SendAsync(new HttpRequestMessage()
                {
                    Method = HttpMethod.Get, RequestUri = new Uri(ADDRESS+userId),
                    Content = new StringContent(value, Encoding.UTF8, MEDIA_TYPE)
                }).ConfigureAwait(CONFIGURE_AWAIT.Value);
            }
            

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
            var json = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(command);

            switch (json!["command"].ToString())
            {
                
                case "post_build":
                {
                    Console.WriteLine(json["userId"]+"---------------------------");
                    Console.WriteLine(_body.PostBuildAsync(json["data"]
                        , json["userId"]
                        , json["ADDRESS"] 
                        , json["MEDIA_TYPE"]
                        , json["CONFIGURE_AWAIT"]).Result+(DateTime.Now.Date + DateTime.Now.TimeOfDay + DateTime.Now.Millisecond.ToString() + "_Assembly")
                        .Replace(' ', '_').Replace(',', '_').Replace('.', '_').Replace(':', '_')
                    );
                    break;
                }
                case "post_error":
                {
                    Console.WriteLine(_body.PostErrorAsync(json["errorMessage"]
                        , json["userId"]
                        , json["ADDRESS"] 
                        , json["MEDIA_TYPE"]
                        , json["CONFIGURE_AWAIT"]).Result);
                    break;
                }
            }

            /*var action = _invokeHandler.SwitchInputAction(command);
            action();*/

        }

        public String OutputInvoker(String command)
        {
            var json = JsonConvert.DeserializeObject<Dictionary<String, dynamic>>(command);

            switch (json!["command"].ToString())
            {
                case "warn_dependency_exception":
                {
                    _body.WarnDependencyException(json["value"]);
                    break;
                }
            }
            
            var func = _invokeHandler.SwitchOutputAction(command);
            return func();
        }
    }
}