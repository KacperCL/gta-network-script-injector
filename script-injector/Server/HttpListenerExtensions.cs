using System.IO;
using System.Net;
using System.Text;

namespace ScriptInjector.Server
{
    public static class HttpListenerExtensions
    {
        public static void WriteOutputString(this HttpListenerResponse response, string @string)
        {
            var buffer = Encoding.UTF8.GetBytes(@string);
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        public static string ReadInputString(this HttpListenerRequest request)
        {
            using (var body = request.InputStream)
            {
                using (var reader = new StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
