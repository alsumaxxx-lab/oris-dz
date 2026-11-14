using System.Text.Json;
using HttpServer1;

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