using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace SqlServerJson;

public class Database : DbContext
{
    public DbSet<Product> Products { get; set; }
        = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseSqlServer("Data Source=localhost,11433;database=Json;UID=sa;password=Pass123!;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(typeof(Database).GetMethod(nameof(JsonValue))!)
            .HasName("JSON_VALUE")
            .IsBuiltIn();
        
        modelBuilder.HasDbFunction(typeof(Database).GetMethod(nameof(JsonQuery))!)
            .HasName("JSON_QUERY")
            .IsBuiltIn();
    }
    
    public static string JsonValue(string column, [NotParameterized] string path)
        => throw new NotSupportedException();

    public static string JsonQuery(string column, [NotParameterized] string path) => 
        throw new NotSupportedException();

    public Database TrySeed()
    {
        Database.Migrate();

        if (Products.Any()) return this;
        
        var faker = new Faker<ProductInfo>();
        var results = faker.RuleFor(m => m.Name, f => f.Commerce.ProductName())
            .RuleFor(m => m.Price, f => f.Commerce.Random.Decimal(1, 1000))
            .Generate(1000);

        var products = results.Select(i => new Product
        {
            Json = JsonSerializer.Serialize(i)
        });
            
        Products.AddRange(products);
        SaveChanges();

        return this;
    }
}

public class Product
{
    public int Id { get; set; }
    // serialized ProductInfo
    public string Json { get; set; }
        = "{ }";

    [NotMapped]
    public ProductInfo? Info
    {
        get => JsonSerializer.Deserialize<ProductInfo>(Json) ?? new ProductInfo();
        set => Json = value is {} ? JsonSerializer.Serialize(value) : "{}";
    }
}

public class ProductInfo
{
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}