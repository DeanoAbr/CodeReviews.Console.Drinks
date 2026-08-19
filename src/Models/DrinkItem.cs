using Newtonsoft.Json;

namespace DrinksApp.Models;

public class DrinkListResponse
{
    [JsonProperty("drinks")]
    public List<DrinkItem>? Drinks { get; set; }
}

public class DrinkItem
{
    [JsonProperty("idDrink")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("strDrink")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("strDrinkThumb")]
    public string? ThumbnailUrl { get; set; }

    public override string ToString() => Name;
}
