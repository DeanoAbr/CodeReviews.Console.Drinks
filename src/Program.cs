using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;
using Newtonsoft.Json;

namespace DrinksApp
{
    class Program
    {
        private static readonly DrinksApi api = new DrinksApi();

        static async Task Main(string[] args)
        {
            await RunAppAsync();
        }

        private static async Task RunAppAsync()
        {
            try
            {
                // Display the main menu
                var choice = AnsiConsole.Prompt(new PromptOptions
                {
                    Message = "Select a category to view drinks:"
                });

                // Get the list of categories
                var categories = await api.GetCategoriesAsync();

                if (categories == null || categories.Count == 0)
                {
                    AnsiConsole.WriteLine("No categories found.");
                    return;
                }
                // Display the categories
                var categoryChoices = categories.Select((category, index) => new SelectListItem
                {
                    Text = category.strCategory,
                Value = index.ToString()
            });

            var selectedCategoryIndex = AnsiConsole.Prompt(new PromptOptions
            {
                    Message = "Select a category:",
                    Choices = categoryChoices
            });

                // Get the drinks in the selected category
                var drinks = await api.GetDrinksByCategoryAsync(categories[selectedCategoryIndex].strCategory);

                if (drinks == null || drinks.Count == 0)
                {
                    AnsiConsole.WriteLine("No drinks found in this category.");
                    return;
                }
                // Display the drinks
                var drinkChoices = drinks.Select((drink, index) => new SelectListItem
                {
                    Text = drink.strDrink,
                    Value = index.ToString()
                });

                var selectedDrinkIndex = AnsiConsole.Prompt(new PromptOptions
                {
                    Message = "Select a drink to view details:",
                    Choices = drinkChoices
                });

                // Get the details of the selected drink
                var drinkDetails = await api.GetDrinkDetailsAsync(drinks[selectedDrinkIndex].strDrink);

                if (drinkDetails == null)
                {
                    AnsiConsole.WriteLine("Drink details not found.");
                    return;
                }

                // Display the drink details
                AnsiConsole.Write(new Markup($@"**{drinkDetails.strDrink}**

**Ingredients:**
{drinkDetails.strInstructions}

**Glass:** {drinkDetails.strGlass}

**Tags:** {drinkDetails.strTags}

**Instructions:**
{drinkDetails.strInstructions}

**Thumbnail:** {drinkDetails.strDrinkThumb}
"));
        }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"An error occurred: {ex.Message}");
    }
}
    }
}

