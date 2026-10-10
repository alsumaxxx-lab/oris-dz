
using System.Net;
using System.Text;

namespace oris.Framework.Handlers
{
    public abstract class Handler
    {
        protected readonly string RequestPrefix;

        public Handler? Successor { get; set; }

        protected Handler(string requestPrefix) => RequestPrefix = requestPrefix;

        public abstract void HandleRequest(HttpListenerContext context);
        
        protected void PassToSuccessor(HttpListenerContext context)
        {
            if (Successor != null)
                Successor.HandleRequest(context);
            else
                SendText(context.Response, 404, "404 Not Found");
        }

        protected string GetRelativePath(HttpListenerContext context)
        {
            string path = Uri.UnescapeDataString(context.Request.Url!.AbsolutePath);

            if (path.StartsWith(RequestPrefix, StringComparison.OrdinalIgnoreCase))
                path = path.Substring(RequestPrefix.Length);

            return path.TrimStart('/');
        }

        public static void SendText(HttpListenerResponse response, int statusCode, string text,
            string contentType = "text/plain; charset=utf-8")
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            response.StatusCode = statusCode;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer);
            response.Close();

            Console.WriteLine(statusCode);
        }
    }
}
