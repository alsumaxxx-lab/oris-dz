namespace kr;

class Lead
{
    public int Id;
    public string Name = "";
    public string Source = "";
    public string Status = "new";
}

static class Store
{
    public static List<Lead> Leads = new();
    public static int NextId = 1;
    public static string[] Sources = { "website", "social", "recommendation" };

    static Store()
    {
        Leads.Add(new Lead { Id = NextId++, Name = "Иван", Source = "website" });
        Leads.Add(new Lead { Id = NextId++, Name = "Мария", Source = "social", Status = "processed" });
    }
    
}


