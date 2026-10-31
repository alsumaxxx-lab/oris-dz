using System.Net;
using System.Text;
using kr;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8888/");
        listener.Start();
        Console.WriteLine("http://localhost:8888/leads");

        while (true)
        {
            var ctx = listener.GetContext();
            try { Handlers.Handle(ctx); }
            catch { try { ctx.Response.StatusCode = 500; ctx.Response.Close(); } catch { } }
        }
    }
}