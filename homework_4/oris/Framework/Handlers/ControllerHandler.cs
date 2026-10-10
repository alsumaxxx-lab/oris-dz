using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using oris.Controllers;
using oris.Framework.Attributes;

namespace oris.Framework.Handlers
{
    public class ControllerHandler : Handler
    {
        private record Route(string Key, string HttpMethod, Type ControllerType, MethodInfo Method);

        private readonly List<Route> routes = new();

        public ControllerHandler(string requestPrefix) : base(requestPrefix)
        {
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var controllerAttr = type.GetCustomAttribute<HttpControllerAttribute>();
                if (controllerAttr == null) continue;

                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var get = method.GetCustomAttribute<GetAttribute>();
                    if (get != null)
                        routes.Add(new Route($"{controllerAttr.Name}/{get.Route}", "GET", type, method));

                    var post = method.GetCustomAttribute<PostAttribute>();
                    if (post != null)
                        routes.Add(new Route($"{controllerAttr.Name}/{post.Route}", "POST", type, method));
                }
            }
        }

        public override void HandleRequest(HttpListenerContext context)
        {
            string path = GetRelativePath(context).Trim('/');
            string httpMethod = context.Request.HttpMethod;

            Route? route = routes.FirstOrDefault(r =>
                r.HttpMethod == httpMethod &&
                r.Key.Equals(path, StringComparison.OrdinalIgnoreCase));

            if (route == null)
            {
                PassToSuccessor(context);
                return;
            }

            var query = HttpUtility.ParseQueryString(context.Request.Url!.Query);
            var form = new System.Collections.Specialized.NameValueCollection();

            if (httpMethod == "POST" && context.Request.HasEntityBody)
            {
                using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                form = HttpUtility.ParseQueryString(reader.ReadToEnd());
            }

            ParameterInfo[] parameters = route.Method.GetParameters();
            object?[] args = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                string? value = form[parameters[i].Name] ?? query[parameters[i].Name];

                args[i] = value == null
                    ? null
                    : Convert.ChangeType(value, parameters[i].ParameterType);
            }

            object controller = Activator.CreateInstance(route.ControllerType)!;
            route.ControllerType.GetProperty("Context")?.SetValue(controller, context);

            try
            {
                route.Method.Invoke(controller, args);
            }
            finally
            {
                context.Response.Close();
            }

            Console.WriteLine($"200 {httpMethod} {context.Request.Url.AbsolutePath}");
        }
    }


}
