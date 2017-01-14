using GTANetworkServer;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace ScriptInjector.Server
{
    public class Main : Script
    {
        WebServer _webServer;

        public Main()
        {
            API.onResourceStart += OnResourceStart;
            API.onResourceStop += OnResourceStop;
        }

        private void OnResourceStart()
        {
            var port = int.Parse(API.getSetting<string>("port"));

            API.consoleOutput("Starting webserver on port: " + port);

            this._webServer = new WebServer(port, API.getResourceFolder() + "\\Server\\Html");
            _webServer.AddGetHandler("/get/players", GetPlayers);
            _webServer.AddPostHandler("/post/client", ExecuteClientScript);
            _webServer.Start();

            Process.Start("http://127.0.0.1:" + port);
        }

        private void GetPlayers(HttpListenerRequest request, HttpListenerResponse response)
        {
            var playersArray = JsonConvert.SerializeObject(API.getAllPlayers().Select(p => p.socialClubName).ToArray());
            response.ContentType = "application/json";
            response.WriteOutputString(playersArray);
        }

        private void ExecuteClientScript(HttpListenerRequest request, HttpListenerResponse response)
        {
            var body = request.ReadInputString();
            var code = JsonConvert.DeserializeObject<CodeData>(body);

            API.consoleOutput(code.Code);
            API.consoleOutput(code.TargetPlayer);

            var targetPlayer = API.getAllPlayers().FirstOrDefault(p => p.socialClubName == code.TargetPlayer);
            API.triggerClientEvent(targetPlayer, "INJECT_SCRIPT", code.Code);
        }

        private void OnResourceStop()
        {
            _webServer.Stop();
        }
    }
}
