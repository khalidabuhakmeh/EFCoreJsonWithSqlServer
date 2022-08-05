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

var newProduct = new Product {
    Info = new ProductInfo { Name = "Banana", Price = 3m }
};

// correct
newProduct.Info = new() { Name = "Banana", Price = 4m };
// incorrect (won't serialize when setting the value)
newProduct.Info.Price = 4m;

foreach (var product in expensive)
{
    Console.WriteLine($"{product.Name} ({product.Price:C})");
}