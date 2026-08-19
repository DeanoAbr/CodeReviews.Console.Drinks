using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DrinksApp.Models;
using DrinksApp.Services;
using DrinksApp.UI;
using Spectre.Console;

namespace DrinksApp.Controllers;

public class DrinksController
{
    private readonly DrinksApiService _apiService;
    private readonly StorageService _storageService;

    public DrinksController(DrinksApiService apiService, StorageService storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            AnsiConsole.Clear();
            DisplayAppBanner();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]MAIN MENU - What would you like to do?[/]")
                    .PageSize(10)
                    .AddChoices(new[]
                    {
                        "🍸 Browse Drinks by Category",
                        "🔍 Search Drink by Name",
                        "🎲 Discover a Random Drink",
                        "❤️ View Favorite Drinks",
                        "📊 View Most Popular Drinks (Stats)",
                        "❌ Exit"
                    }));

            if (choice.StartsWith("❌"))
            {
                AnsiConsole.MarkupLine("[green]Thank you for using Drinks App. Goodbye![/]");
                break;
            }

            if (choice.StartsWith("🍸"))
            {
                await HandleBrowseCategoriesAsync();
            }
            else if (choice.StartsWith("🔍"))
            {
                await HandleSearchDrinksAsync();
            }
            else if (choice.StartsWith("🎲"))
            {
                await HandleRandomDrinkAsync();
            }
            else if (choice.StartsWith("❤️"))
            {
                await HandleFavoritesAsync();
            }
            else if (choice.StartsWith("📊"))
            {
                HandleViewStats();
            }
        }
    }

    private void DisplayAppBanner()
    {
        var rule = new Rule("[bold purple]RESTAURANT DRINKS MENU PORTAL[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    private async Task HandleBrowseCategoriesAsync()
    {
        var (success, categories, error) = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Fetching categories...", async _ => await _apiService.GetCategoriesAsync());

        if (!success || categories == null || categories.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error ?? "No categories found.")}");
            WaitForUser();
            return;
        }

        var categoryChoices = categories.Select(c => c.Name).ToList();
        categoryChoices.Add("⬅️ Back to Main Menu");

        var selectedCategory = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Select a Drink Category:[/]")
                .PageSize(15)
                .MoreChoicesText("[grey](Move up and down to see more categories)[/]")
                .AddChoices(categoryChoices));

        if (selectedCategory == "⬅️ Back to Main Menu") return;

        await HandleBrowseDrinksInCategoryAsync(selectedCategory);
    }

    private async Task HandleBrowseDrinksInCategoryAsync(string category)
    {
        var (success, drinks, error) = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Fetching drinks in '{category}'...", async _ => await _apiService.GetDrinksByCategoryAsync(category));

        if (!success || drinks == null || drinks.Count == 0)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error ?? "No drinks found in this category.")}");
            WaitForUser();
            return;
        }

        while (true)
        {
            var drinkChoices = drinks.Select(d => $"{d.Name} (ID: {d.Id})").ToList();
            drinkChoices.Add("⬅️ Back to Categories");

            var prompt = new SelectionPrompt<string>()
                .Title($"[bold yellow]Drinks in category '{Markup.Escape(category)}':[/]")
                .PageSize(15)
                .MoreChoicesText("[grey](Type to search, move up/down to select)[/]")
                .AddChoices(drinkChoices);
            prompt.SearchEnabled = true;

            var selected = AnsiConsole.Prompt(prompt);

            if (selected == "⬅️ Back to Categories") return;

            var selectedDrink = drinks.First(d => $"{d.Name} (ID: {d.Id})" == selected);
            await HandleViewDrinkDetailsAsync(selectedDrink.Id);
        }
    }

    private async Task HandleSearchDrinksAsync()
    {
        var query = AnsiConsole.Ask<string>("[bold cyan]Enter drink name to search (or '0' to cancel):[/]").Trim();
        if (query == "0" || string.IsNullOrWhiteSpace(query)) return;

        var (success, drinks, error) = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"Searching for '{query}'...", async _ => await _apiService.SearchDrinksByNameAsync(query));

        if (!success || drinks == null || drinks.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No drinks found matching '{Markup.Escape(query)}'.[/]");
            WaitForUser();
            return;
        }

        var drinkChoices = drinks.Select(d => $"{d.Name} (ID: {d.Id})").ToList();
        drinkChoices.Add("⬅️ Back to Main Menu");

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[bold yellow]Search Results for '{Markup.Escape(query)}':[/]")
                .PageSize(15)
                .AddChoices(drinkChoices));

        if (selected == "⬅️ Back to Main Menu") return;

        var selectedDrink = drinks.First(d => $"{d.Name} (ID: {d.Id})" == selected);
        await HandleViewDrinkDetailsAsync(selectedDrink.Id);
    }

    private async Task HandleRandomDrinkAsync()
    {
        var (success, drink, error) = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("Finding a random drink...", async _ => await _apiService.GetRandomDrinkAsync());

        if (!success || drink == null)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error ?? "Could not load random drink.")}");
            WaitForUser();
            return;
        }

        await HandleViewDrinkDetailsAsync(drink.Id, drink);
    }

    private async Task HandleFavoritesAsync()
    {
        var favIds = _storageService.GetFavoriteIds();
        if (favIds.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]You have no favorite drinks saved yet. Browse drinks and select 'Add to Favorites' to save some![/]");
            WaitForUser();
            return;
        }

        var choices = favIds.ToList();
        choices.Add("⬅️ Back to Main Menu");

        var selectedId = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Favorite Drinks (by ID):[/]")
                .PageSize(15)
                .AddChoices(choices));

        if (selectedId == "⬅️ Back to Main Menu") return;

        await HandleViewDrinkDetailsAsync(selectedId);
    }

    private void HandleViewStats()
    {
        var topDrinks = _storageService.GetTopViewedDrinks(15);
        if (topDrinks.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No drink view statistics recorded yet.[/]");
            WaitForUser();
            return;
        }

        var table = new Table().Border(TableBorder.Rounded).Title("[bold gold1]Most Popular Drinks Leaderboard[/]");
        table.AddColumn(new TableColumn("[bold]Rank[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Drink Name[/]"));
        table.AddColumn(new TableColumn("[bold]Drink ID[/]"));
        table.AddColumn(new TableColumn("[bold]View Count[/]").Centered());

        int rank = 1;
        foreach (var stat in topDrinks)
        {
            table.AddRow(
                rank.ToString(),
                Markup.Escape(stat.DrinkName),
                Markup.Escape(stat.DrinkId),
                $"[cyan]{stat.ViewCount}[/]"
            );
            rank++;
        }

        AnsiConsole.Clear();
        AnsiConsole.Write(table);
        WaitForUser();
    }

    private async Task HandleViewDrinkDetailsAsync(string drinkId, DrinkDetail? preloadedDrink = null)
    {
        DrinkDetail? drink = preloadedDrink;

        if (drink == null)
        {
            var (success, fetched, error) = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Loading drink details...", async _ => await _apiService.GetDrinkByIdAsync(drinkId));

            if (!success || fetched == null)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(error ?? "Could not load drink details.")}");
                WaitForUser();
                return;
            }
            drink = fetched;
        }

        while (true)
        {
            await DrinkDetailView.RenderDrinkDetailsAsync(drink, _apiService, _storageService);

            bool isFav = _storageService.IsFavorite(drink.Id);
            var favText = isFav ? "💔 Remove from Favorites" : "❤️ Add to Favorites";

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Options:[/]")
                    .AddChoices(new[]
                    {
                        favText,
                        "⬅️ Back"
                    }));

            if (action == "⬅️ Back") break;

            if (action == favText)
            {
                _storageService.ToggleFavorite(drink.Id);
            }
        }
    }

    private void WaitForUser()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }
}
