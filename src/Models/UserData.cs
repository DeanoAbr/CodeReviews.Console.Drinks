using System.Collections.Generic;

namespace DrinksApp.Models;

public class UserData
{
    public HashSet<string> FavoriteDrinkIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, DrinkViewInfo> ViewStats { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public class DrinkViewInfo
{
    public string DrinkId { get; set; } = string.Empty;
    public string DrinkName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}
