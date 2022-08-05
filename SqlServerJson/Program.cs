using Microsoft.EntityFrameworkCore;

// project namespace
using SqlServerJson;
using static SqlServerJson.Database;
using PI = SqlServerJson.ProductInfo;

var database = new Database().TrySeed();

var expensive = database.Products
        .Select(p => new {
            p.Id,
            Name = JsonValue(p.Json, $"$.{nameof(PI.Name)}"),
            Price = Convert.ToDecimal(JsonValue(p.Json, $"$.{nameof(PI.Price)}"))
        })
        .Where(x => x.Price > 800)
        .OrderByDescending(x => x.Price)
        .Take(10);

Console.WriteLine(expensive.ToQueryString() + "\n");

foreach (var product in expensive)
{
    Console.WriteLine($"{product.Name} ({product.Price:C})");
}

var deserialized = database.Products.Take(10).ToList();

Console.Write("\nUsing NotMapped Attribute\n");
foreach (var product in deserialized)
{
    var info = product.Info;
    Console.WriteLine($"{info?.Name} ({info?.Price:C})");
}
