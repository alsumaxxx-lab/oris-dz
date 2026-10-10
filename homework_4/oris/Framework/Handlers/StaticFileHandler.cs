using System.Net;
using System.Text;

namespace oris.Framework.Handlers
{
    public class StaticFileHandler : Handler
    {
        public static readonly string StaticRoot = Path.GetFullPath("static");

        private static readonly Dictionary<string, string> MimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = "text/html; charset=utf-8",
            [".css"] = "text/css; charset=utf-8",
            [".js"] = "text/javascript; charset=utf-8",
            [".json"] = "application/json",
            [".txt"] = "text/plain; charset=utf-8",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".gif"] = "image/gif",
            [".svg"] = "image/svg+xml",
            [".ico"] = "image/vnd.microsoft.icon",
            [".mp3"] = "audio/mpeg",
            [".mp4"] = "video/mp4",
            [".woff2"] = "font/woff2",
            [".pdf"] = "application/pdf",
            [".zip"] = "application/zip",
        };

        public StaticFileHandler(string requestPrefix) : base(requestPrefix)
        {
        }

        public override void HandleRequest(HttpListenerContext context)
        {
            string? filePath = FindFile(GetRelativePath(context));

            if (filePath == null)
            {
                PassToSuccessor(context);
                return;
            }

            SendFile(context.Response, filePath);
            Console.WriteLine($"200 {context.Request.Url!.AbsolutePath}");
        }

        public static string? FindFile(string relativePath)
        {
            relativePath = relativePath.Replace('\\', '/').Trim('/');
            if (relativePath == "")
                relativePath = "index.html";

            var candidates = new List<string> { relativePath };
            if (!Path.HasExtension(relativePath))
                candidates.Add(relativePath + ".html");

            foreach (string candidate in candidates)
            {
                string full = Path.GetFullPath(Path.Combine(StaticRoot, candidate));

                if (!full.StartsWith(StaticRoot + Path.DirectorySeparatorChar))
                    return null;

                if (File.Exists(full))
                    return full;
            }

            if (!Directory.Exists(StaticRoot))
                return null;

            foreach (string candidate in candidates)
            {
                string? found = Directory
                    .EnumerateFiles(StaticRoot, "*", SearchOption.AllDirectories)
                    .FirstOrDefault(f =>
                    {
                        string rel = Path.GetRelativePath(StaticRoot, f).Replace('\\', '/');
                        return rel.Equals(candidate, StringComparison.OrdinalIgnoreCase)
                               || rel.EndsWith("/" + candidate, StringComparison.OrdinalIgnoreCase);
                    });

                if (found != null)
                    return found;
            }

            return null;
        }
        
        public static void SendStaticFile(HttpListenerResponse response, string relativePath)
        {
            string? filePath = FindFile(relativePath);

            if (filePath == null)
                SendText(response, 404, "404 Not Found");
            else
                SendFile(response, filePath);
        }

        private static void SendFile(HttpListenerResponse response, string filePath)
        {
            string extension = Path.GetExtension(filePath);
            string contentType = MimeTypes.GetValueOrDefault(extension, "application/octet-stream");

            byte[] buffer = File.ReadAllBytes(filePath);

            response.StatusCode = 200;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer);
            response.Close();
        }
    }
}
