using System.Net;
using System.Text;

namespace kr;

static class Handlers
{
    public static void Handle(HttpListenerContext ctx)
    {
        var req = ctx.Request;
        var path = req.Url!.AbsolutePath;
        var method = req.HttpMethod;

        if (method == "GET" && path == "/leads") List(ctx);
        else if (method == "GET" && path == "/leads/details") Details(ctx);
        else if (method == "GET" && path == "/leads/create") Form(ctx);
        else if (method == "POST" && path == "/leads/create") Create(ctx);
        else if (method == "PUT" && path == "/leads/update") Update(ctx);
        else Utils.Send(ctx, 404, "Не найдено");
    }

    static void List(HttpListenerContext ctx)
    {
        var source = ctx.Request.QueryString["source"];
        var name = ctx.Request.QueryString["name"];

        var q = Store.Leads.Where(l => string.IsNullOrEmpty(source) || l.Source == source);
        if (!string.IsNullOrEmpty(name))
            q = q.Where(l => l.Name.ToLower().Contains(name!.ToLower()));

        var sb = new StringBuilder("<h1>Клиенты</h1>");
        sb.Append("<form><select name='source'><option value=''>все</option>");
        foreach (var s in Store.Sources) sb.Append($"<option>{s}</option>");
        sb.Append("</select> <input name='name' placeholder='имя'> <button>Фильтр</button></form>");
        sb.Append("<table border=1 cellpadding=5><tr><th>Id<th>Имя<th>Источник<th>Статус<th></tr>");
        foreach (var l in q)
            sb.Append($"<tr><td>{l.Id}<td>{Utils.E(l.Name)}<td>{Utils.E(l.Source)}<td>{Utils.E(l.Status)}" +
                      $"<td><a href='/leads/details?id={l.Id}'>детали</a></tr>");
        sb.Append("</table><p><a href='/leads/create'>Добавить</a>");
        Utils.Send(ctx, 200, sb.ToString());
    }

    static void Details(HttpListenerContext ctx)
    {
        var lead = Store.Leads.FirstOrDefault(l => l.Id.ToString() == ctx.Request.QueryString["id"]);
        if (lead == null) { Utils.Send(ctx, 404, "Не найден"); return; }
        Utils.Send(ctx, 200, $"<h1>{Utils.E(lead.Name)}</h1><p>Источник: {Utils.E(lead.Source)}<p>Статус: {Utils.E(lead.Status)}" +
                             "<p><a href='/leads'>Назад</a>");
    }

    static void Form(HttpListenerContext ctx)
    {
        var sb = new StringBuilder("<h1>Новый клиент</h1>");
        sb.Append("<form method=post action='/leads/create'>");
        sb.Append("Имя: <input name='name'><br>");
        sb.Append("Источник: <select name='source'>");
        foreach (var s in Store.Sources) sb.Append($"<option>{s}</option>");
        sb.Append("</select><br><button>Создать</button></form>");
        Utils.Send(ctx, 200, sb.ToString());
    }

    static void Create(HttpListenerContext ctx)
    {
        var f = Utils.ReadForm(ctx.Request);
        var name = f.GetValueOrDefault("name", "").Trim();
        var source = f.GetValueOrDefault("source", "").Trim();

        if (name.Length < 2 || name.Length > 60) { Utils.Send(ctx, 400, "Имя 2–60 символов"); return; }
        if (!Store.Sources.Contains(source)) { Utils.Send(ctx, 400, "Неверный источник"); return; }

        Store.Leads.Add(new Lead { Id = Store.NextId++, Name = name, Source = source });
        Utils.Redirect(ctx, "/leads");
    }

    static void Update(HttpListenerContext ctx)
    {
        var lead = Store.Leads.FirstOrDefault(l => l.Id.ToString() == ctx.Request.QueryString["id"]);
        if (lead == null) { Utils.Send(ctx, 404, "Не найден"); return; }

        var f = Utils.ReadForm(ctx.Request);
        var name = f.GetValueOrDefault("name", "").Trim();
        var source = f.GetValueOrDefault("source", "").Trim();
        var status = f.GetValueOrDefault("status", "").Trim();

        if (name.Length < 2 || name.Length > 60) { Utils.Send(ctx, 400, "Имя 2–60 символов"); return; }
        if (!Store.Sources.Contains(source)) { Utils.Send(ctx, 400, "Неверный источник"); return; }
        if (status != "new" && status != "processed") { Utils.Send(ctx, 400, "Неверный статус"); return; }

        lead.Name = name; lead.Source = source; lead.Status = status;
        Utils.NoContent(ctx);
    }
}