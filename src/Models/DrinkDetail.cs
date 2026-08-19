using System.Reflection;
using Newtonsoft.Json;

namespace DrinksApp.Models;

public class DrinkDetailResponse
{
    [JsonProperty("drinks")]
    public List<DrinkDetail>? Drinks { get; set; }
}

public class DrinkDetail
{
    [JsonProperty("idDrink")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("strDrink")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("strDrinkAlternate")]
    public string? AlternateName { get; set; }

    [JsonProperty("strTags")]
    public string? Tags { get; set; }

    [JsonProperty("strVideo")]
    public string? Video { get; set; }

    [JsonProperty("strCategory")]
    public string? Category { get; set; }

    [JsonProperty("strIBA")]
    public string? IBA { get; set; }

    [JsonProperty("strAlcoholic")]
    public string? Alcoholic { get; set; }

    [JsonProperty("strGlass")]
    public string? Glass { get; set; }

    [JsonProperty("strInstructions")]
    public string? Instructions { get; set; }

    [JsonProperty("strInstructionsES")]
    public string? InstructionsES { get; set; }

    [JsonProperty("strInstructionsDE")]
    public string? InstructionsDE { get; set; }

    [JsonProperty("strInstructionsFR")]
    public string? InstructionsFR { get; set; }

    [JsonProperty("strInstructionsIT")]
    public string? InstructionsIT { get; set; }

    [JsonProperty("strDrinkThumb")]
    public string? ThumbnailUrl { get; set; }

    [JsonProperty("strImageSource")]
    public string? ImageSource { get; set; }

    [JsonProperty("strImageAttribution")]
    public string? ImageAttribution { get; set; }

    [JsonProperty("strCreativeCommonsConfirmed")]
    public string? CreativeCommonsConfirmed { get; set; }

    [JsonProperty("dateModified")]
    public string? DateModified { get; set; }

    // Ingredients 1 to 15
    [JsonProperty("strIngredient1")] public string? Ingredient1 { get; set; }
    [JsonProperty("strIngredient2")] public string? Ingredient2 { get; set; }
    [JsonProperty("strIngredient3")] public string? Ingredient3 { get; set; }
    [JsonProperty("strIngredient4")] public string? Ingredient4 { get; set; }
    [JsonProperty("strIngredient5")] public string? Ingredient5 { get; set; }
    [JsonProperty("strIngredient6")] public string? Ingredient6 { get; set; }
    [JsonProperty("strIngredient7")] public string? Ingredient7 { get; set; }
    [JsonProperty("strIngredient8")] public string? Ingredient8 { get; set; }
    [JsonProperty("strIngredient9")] public string? Ingredient9 { get; set; }
    [JsonProperty("strIngredient10")] public string? Ingredient10 { get; set; }
    [JsonProperty("strIngredient11")] public string? Ingredient11 { get; set; }
    [JsonProperty("strIngredient12")] public string? Ingredient12 { get; set; }
    [JsonProperty("strIngredient13")] public string? Ingredient13 { get; set; }
    [JsonProperty("strIngredient14")] public string? Ingredient14 { get; set; }
    [JsonProperty("strIngredient15")] public string? Ingredient15 { get; set; }

    // Measures 1 to 15
    [JsonProperty("strMeasure1")] public string? Measure1 { get; set; }
    [JsonProperty("strMeasure2")] public string? Measure2 { get; set; }
    [JsonProperty("strMeasure3")] public string? Measure3 { get; set; }
    [JsonProperty("strMeasure4")] public string? Measure4 { get; set; }
    [JsonProperty("strMeasure5")] public string? Measure5 { get; set; }
    [JsonProperty("strMeasure6")] public string? Measure6 { get; set; }
    [JsonProperty("strMeasure7")] public string? Measure7 { get; set; }
    [JsonProperty("strMeasure8")] public string? Measure8 { get; set; }
    [JsonProperty("strMeasure9")] public string? Measure9 { get; set; }
    [JsonProperty("strMeasure10")] public string? Measure10 { get; set; }
    [JsonProperty("strMeasure11")] public string? Measure11 { get; set; }
    [JsonProperty("strMeasure12")] public string? Measure12 { get; set; }
    [JsonProperty("strMeasure13")] public string? Measure13 { get; set; }
    [JsonProperty("strMeasure14")] public string? Measure14 { get; set; }
    [JsonProperty("strMeasure15")] public string? Measure15 { get; set; }

    /// <summary>
    /// Extracts all non-empty ingredients paired with their non-empty measure (if any).
    /// </summary>
    public List<(string Ingredient, string? Measure)> GetIngredients()
    {
        var ingredients = new List<(string Ingredient, string? Measure)>();
        for (int i = 1; i <= 15; i++)
        {
            var ingredientProp = GetType().GetProperty($"Ingredient{i}");
            var measureProp = GetType().GetProperty($"Measure{i}");

            var ingredient = ingredientProp?.GetValue(this)?.ToString()?.Trim();
            var measure = measureProp?.GetValue(this)?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(ingredient))
            {
                ingredients.Add((ingredient, string.IsNullOrWhiteSpace(measure) ? null : measure));
            }
        }
        return ingredients;
    }

    /// <summary>
    /// Returns dictionary of non-empty properties (excluding ingredients and raw measures which are presented formatted).
    /// Requirement: "When the users visualise the drink detail, there shouldn't be any properties with empty values."
    /// </summary>
    public Dictionary<string, string> GetNonEmptyProperties()
    {
        var dict = new Dictionary<string, string>();

        void AddIfValid(string label, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                dict[label] = value.Trim();
            }
        }

        AddIfValid("ID", Id);
        AddIfValid("Name", Name);
        AddIfValid("Category", Category);
        AddIfValid("Alcoholic", Alcoholic);
        AddIfValid("Glass Type", Glass);
        AddIfValid("IBA Category", IBA);
        AddIfValid("Tags", Tags);
        AddIfValid("Instructions", Instructions);
        AddIfValid("Instructions (Spanish)", InstructionsES);
        AddIfValid("Instructions (German)", InstructionsDE);
        AddIfValid("Instructions (French)", InstructionsFR);
        AddIfValid("Instructions (Italian)", InstructionsIT);
        AddIfValid("Video Tutorial", Video);
        AddIfValid("Alternate Name", AlternateName);
        AddIfValid("Image Source", ImageSource);
        AddIfValid("Image Attribution", ImageAttribution);
        AddIfValid("Creative Commons", CreativeCommonsConfirmed);
        AddIfValid("Date Modified", DateModified);
        AddIfValid("Thumbnail URL", ThumbnailUrl);

        return dict;
    }
}
