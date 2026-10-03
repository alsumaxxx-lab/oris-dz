using System.Net;
using System.Text;

namespace MyHttpServer3;


public class HttpServer
{
    private readonly HttpListener server;
    private readonly string urlPrefix;
    private readonly string requestPrefix;
    private readonly string staticRoot = Path.GetFullPath("static");
 
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
 
    public HttpServer(Settings setting)
    {
        server = new HttpListener();
 
        urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/{setting.Server.Path}";
        requestPrefix = "/" + setting.Server.Path;
 
        server.Prefixes.Add(urlPrefix);
    }
 
    public void Start()
    {
        server.Start();
 
        Console.WriteLine("Сервер запущен и слушает: " + urlPrefix);
 
        _ = Listen();
    }
 
    public void Stop()
    {
        server.Stop();
 
        Console.WriteLine("Сервер завершил работу");
    }
 
    private async Task Listen()
    {
        while (server.IsListening)
        {
            try
            {
                var context = await server.GetContextAsync();
                await HandleRequest(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }
    }
 
    private async Task HandleRequest(HttpListenerContext context)
    {
        var response = context.Response;
        
        string path = Uri.UnescapeDataString(context.Request.Url!.AbsolutePath);
        string relativePath = path.Substring(requestPrefix.Length);
        
        if (relativePath == "" || relativePath.EndsWith('/'))
            relativePath += "index.html";
 
        string filePath = Path.GetFullPath(Path.Combine(staticRoot, relativePath));
        
        bool isInsideStatic = filePath.StartsWith(staticRoot + Path.DirectorySeparatorChar);
 
        if (!isInsideStatic || !File.Exists(filePath))
        {
            await SendNotFound(response);
            return;
        }
 
        string extension = Path.GetExtension(filePath);
        string contentType = MimeTypes.GetValueOrDefault(extension, "application/octet-stream");
 
        byte[] buffer = await File.ReadAllBytesAsync(filePath);
 
        response.StatusCode = 200;
        response.ContentType = contentType;
        response.ContentLength64 = buffer.Length;
 
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
 
        Console.WriteLine($"200 {path}");
    }
 
    private static async Task SendNotFound(HttpListenerResponse response)
    {
        byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
 
        response.StatusCode = 404;
        response.ContentType = "text/plain; charset=utf-8";
        response.ContentLength64 = buffer.Length;
 
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
 
        Console.WriteLine("404");
    }
}