using System.Net;
using System.Text;

namespace MyHttpServer;

public class HttpServer
{
    private readonly HttpListener server;
    private readonly string urlPrefix;

    public HttpServer(Settings setting)
    {
        server = new HttpListener();

        urlPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/{setting.Server.Path}";

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

                string htmlFileText = File.ReadAllText("search.html");

                byte[] buffer = Encoding.UTF8.GetBytes(htmlFileText);

                var response = context.Response;

                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;

                await output.WriteAsync(buffer);
                await output.FlushAsync();

                Console.WriteLine("Запрос обработан");
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