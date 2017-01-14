using GTANetworkServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace GTANetworkTruckShitUp.resources.script_injector.Server
{
    public class WebServer
    {
        private readonly HttpListener _httpListener;
        private readonly string _rootPath;
        private readonly Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> _postHandlers =
            new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>> _getHandlers =
            new Dictionary<string, Action<HttpListenerRequest, HttpListenerResponse>>(StringComparer.InvariantCultureIgnoreCase);

        public WebServer(int port, string rootPath)
        {
            _rootPath = rootPath.TrimEnd(new[] { '\\' }).Replace("\\", "/");

            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));

            _httpListener.Start();
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                API.shared.consoleOutput("Webserver running...");

                try
                {
                    while (_httpListener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((callback) =>
                            {
                                var context = callback as HttpListenerContext;
                                try
                                {
                                    RespondToRequest(context.Request, context.Response);
                                    context.Response.OutputStream.Flush();
                                    context.Response.OutputStream.Close();
                                }
                                catch (Exception ex)
                                {
                                    API.shared.consoleOutput(ex.ToString());
                                }
                                finally
                                {
                                    context.Response.OutputStream.Close();
                                }
                            },
                            _httpListener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    API.shared.consoleOutput(ex.ToString());
                }
            });
        }

        private void RespondToRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            switch (request.HttpMethod)
            {
                case "GET":
                    RespondToGet(request, response);
                    break;
                case "POST":
                    RespondToPost(request, response);
                    break;
            }
        }

        private void RespondToGet(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_getHandlers.ContainsKey(request.RawUrl))
            {
                _getHandlers[request.RawUrl].Invoke(request, response);
                return;
            }

            var requestPath = request.RawUrl;
            if (requestPath == "/") requestPath = "/index.html";

            var requestedFilePath = _rootPath + requestPath;
            API.shared.consoleOutput("GET Request: " + requestedFilePath);

            if (!File.Exists(requestedFilePath))
            {
                response.StatusCode = 404;
                return;
            }

            var fileContent = File.ReadAllBytes(requestedFilePath);
            var mimeType = MimeMapping.GetMimeMapping(requestedFilePath.Split(new[] { '/' }).LastOrDefault());

            API.shared.consoleOutput("Mime type: " + mimeType);

            response.ContentType = mimeType;
            response.OutputStream.Write(fileContent, 0, fileContent.Length);
        }

        private void RespondToPost(HttpListenerRequest request, HttpListenerResponse response)
        {
            API.shared.consoleOutput("POST Request: " + request.RawUrl);

            if (_postHandlers.ContainsKey(request.RawUrl))
            {
                _postHandlers[request.RawUrl].Invoke(request, response);
            }
            else
            {
                API.shared.consoleOutput("No handler.");
            }
        }

        public void AddPostHandler(string postUrl, Action<HttpListenerRequest, HttpListenerResponse> handler)
        {
            _postHandlers.Add(postUrl, handler);
        }

        public void AddGetHandler(string getUrl, Action<HttpListenerRequest, HttpListenerResponse> handler)
        {
            _getHandlers.Add(getUrl, handler);
        }

        public void Stop()
        {
            _httpListener.Stop();
            _httpListener.Close();
        }
    }
}
