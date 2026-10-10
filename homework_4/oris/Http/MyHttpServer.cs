using System.Net;
using System.Text;
using oris;
using oris.Framework.Handlers;

namespace oris.Http;


public class HttpServer
{
    private readonly HttpListener server;
    private readonly string urlPrefix;
    private readonly Handler firstHandler;

    public HttpServer(Settings setting)
    {
        server = new HttpListener();

        string path = setting.Server.Path.Trim('/');
        string requestPrefix = path == "" ? "/" : $"/{path}/";

        urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}{requestPrefix}";
        server.Prefixes.Add(urlPrefix);
        
        var controllerHandler = new ControllerHandler(requestPrefix);
        var staticFileHandler = new StaticFileHandler(requestPrefix);
        controllerHandler.Successor = staticFileHandler;
        firstHandler = controllerHandler;
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

                try
                {
                    firstHandler.HandleRequest(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка обработки: " + ex.Message);
                    Handler.SendText(context.Response, 500, "500 Internal Server Error");
                }
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
}
