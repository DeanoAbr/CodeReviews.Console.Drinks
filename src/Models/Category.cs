using Newtonsoft.Json;

namespace DrinksApp.Models;

public class CategoryResponse
{
    [JsonProperty("drinks")]
    public List<Category>? Categories { get; set; }
}

public class Category
{
    [JsonProperty("strCategory")]
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
