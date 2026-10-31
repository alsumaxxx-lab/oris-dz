using System.Net;
using System.Text;
using System.Web;

namespace kr;
static class Utils
{
    public static Dictionary<string, string> ReadForm(HttpListenerRequest req)
    {
        var d = new Dictionary<string, string>();
        using var r = new StreamReader(req.InputStream, Encoding.UTF8);
        foreach (var p in r.ReadToEnd().Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = p.Split('=', 2);
            d[HttpUtility.UrlDecode(kv[0])] = kv.Length > 1 ? HttpUtility.UrlDecode(kv[1]) : "";
        }
        return d;
    }

    public static string E(string? s) => HttpUtility.HtmlEncode(s ?? "");

    public static void Send(HttpListenerContext ctx, int code, string html)
    {
        var body = $"<html><head><meta charset='utf-8'></head><body>{html}</body></html>";
        var bytes = Encoding.UTF8.GetBytes(body);
        ctx.Response.StatusCode = code;
        ctx.Response.ContentType = "text/html; charset=utf-8";
        ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
        ctx.Response.Close();
    }

    public static void Redirect(HttpListenerContext ctx, string url)
    {
        ctx.Response.StatusCode = 303;
        ctx.Response.RedirectLocation = url;
        ctx.Response.Close();
    }

    public static void NoContent(HttpListenerContext ctx)
    {
        ctx.Response.StatusCode = 204;
        ctx.Response.Close();
    }
}