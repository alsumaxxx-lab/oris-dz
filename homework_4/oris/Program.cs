using System.Text;
using System.Text.Json;
using oris;
using oris.Http;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

string settingsJson = File.ReadAllText("settings.json");

Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson)!;

HttpServer server = new HttpServer(setting);

server.Start();

while (true)
{
    string? command = Console.ReadLine();

    if (command == "stop")
    {
        server.Stop();
        break;
    }
}