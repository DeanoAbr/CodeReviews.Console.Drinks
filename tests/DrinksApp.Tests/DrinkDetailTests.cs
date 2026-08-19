using DrinksApp.Models;
using Xunit;

namespace DrinksApp.Tests;

public class DrinkDetailTests
{
    [Fact]
    public void GetNonEmptyProperties_ExcludesNullAndWhitespaceValues()
    {
        var drink = new DrinkDetail
        {
            Id = "11007",
            Name = "Margarita",
            Category = "Ordinary Drink",
            Alcoholic = "Alcoholic",
            Glass = "Cocktail glass",
            Instructions = "Rub the rim of the glass with the lime slice...",
            Tags = "   ", // whitespace only - should be excluded
            IBA = null,   // null - should be excluded
            Video = ""    // empty - should be excluded
        };

        var properties = drink.GetNonEmptyProperties();

        Assert.Equal("11007", properties["ID"]);
        Assert.Equal("Margarita", properties["Name"]);
        Assert.Equal("Ordinary Drink", properties["Category"]);
        Assert.Equal("Alcoholic", properties["Alcoholic"]);
        Assert.Equal("Cocktail glass", properties["Glass Type"]);
        Assert.Equal("Rub the rim of the glass with the lime slice...", properties["Instructions"]);
        Assert.False(properties.ContainsKey("Tags"));
        Assert.False(properties.ContainsKey("IBA Category"));
        Assert.False(properties.ContainsKey("Video Tutorial"));
    }

    [Fact]
    public void GetIngredients_PairsValidIngredientsAndMeasuresCorrectly()
    {
        var drink = new DrinkDetail
        {
            Ingredient1 = "Tequila",
            Measure1 = "1 1/2 oz",
            Ingredient2 = "Triple sec",
            Measure2 = "1/2 oz",
            Ingredient3 = "Lime juice",
            Measure3 = "1 oz",
            Ingredient4 = "Salt",
            Measure4 = null, // valid ingredient without measure
            Ingredient5 = "   ", // empty ingredient
            Measure5 = "1 splash"
        };

        var ingredients = drink.GetIngredients();

        Assert.Equal(4, ingredients.Count);
        Assert.Equal(("Tequila", "1 1/2 oz"), ingredients[0]);
        Assert.Equal(("Triple sec", "1/2 oz"), ingredients[1]);
        Assert.Equal(("Lime juice", "1 oz"), ingredients[2]);
        Assert.Equal(("Salt", null), ingredients[3]);
    }
}
