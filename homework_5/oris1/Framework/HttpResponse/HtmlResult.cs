using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace oris1.Framework.HttpResponse;

public class HtmlResult
{
    private readonly string _html;

    public HtmlResult(string html)
    {
        _html = html;
    }
    
}