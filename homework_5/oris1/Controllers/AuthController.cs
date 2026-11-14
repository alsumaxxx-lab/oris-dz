using System.Net;
using oris1.Framework.Attributes;
using oris1.Framework.Handlers;

namespace oris1.Controllers


{
    [HttpController("auth")]
    public class AuthController
    {
        public HttpListenerContext Context { get; set; } = null!;

        [Get("login")]
        public void login()
        {
            StaticFileHandler.SendStaticFile(Context.Response, "login.html");
        }

        [Post("login")]
        public void login(string login, string password)
        {
            Console.WriteLine("=== Попытка входа ===");
            Console.WriteLine($"Login:    {login}");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine("=====================");
            
            Context.Response.StatusCode = 204;
            Context.Response.Close();
        }
        
        
        [Post("steam")]
        public void steam(string login, string password)
        {
            Console.WriteLine("=== Попытка входа (Steam) ===");
            Console.WriteLine($"Login:    {login}");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine("=============================");

            Context.Response.StatusCode = 204;
            Context.Response.Close();
        }
    }
}