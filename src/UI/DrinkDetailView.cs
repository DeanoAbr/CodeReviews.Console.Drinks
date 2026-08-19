using System;
using System.IO;
using System.Threading.Tasks;
using DrinksApp.Models;
using DrinksApp.Services;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace DrinksApp.UI;

public static class DrinkDetailView
{
    public static async Task RenderDrinkDetailsAsync(
        DrinkDetail drink, 
        DrinksApiService apiService, 
        StorageService storageService)
    {
        AnsiConsole.Clear();

        // Update and get view stats
        storageService.RecordDrinkView(drink.Id, drink.Name);
        int viewCount = storageService.GetViewCount(drink.Id);
        bool isFavorite = storageService.IsFavorite(drink.Id);

        // Header
        var favBadge = isFavorite ? "[red]:heart: FAVORITE[/]" : "[grey]:white_heart: Not Favorite[/]";
        var viewsBadge = $"[yellow]:eye: Views: {viewCount}[/]";

        var headerRule = new Rule($"[bold aqua]{Markup.Escape(drink.Name.ToUpperInvariant())}[/]  ({favBadge} | {viewsBadge})")
        {
            Justification = Justify.Left
        };
        AnsiConsole.Write(headerRule);
        AnsiConsole.WriteLine();

        // Layout: Grid with Image (if available) and Drink Information
        var grid = new Grid();
        grid.AddColumn(new GridColumn().Width(38)); // Left column: Image
        grid.AddColumn(new GridColumn());          // Right column: Details

        // Try load and render thumbnail image
        Renderable leftPane;
        if (!string.IsNullOrWhiteSpace(drink.ThumbnailUrl))
        {
            var imageBytes = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Loading drink thumbnail...", async _ => 
                    await apiService.DownloadImageBytesAsync(drink.ThumbnailUrl));

            if (imageBytes != null && imageBytes.Length > 0)
            {
                try
                {
                    using var ms = new MemoryStream(imageBytes);
                    var canvasImage = new CanvasImage(ms)
                    {
                        MaxWidth = 34
                    };
                    leftPane = new Panel(canvasImage)
                    {
                        Header = new PanelHeader("[bold cyan]Drink Photo[/]"),
                        Border = BoxBorder.Rounded
                    };
                }
                catch
                {
                    leftPane = new Panel(new Text("Image preview unavailable", new Style(Color.Grey)))
                    {
                        Border = BoxBorder.Rounded
                    };
                }
            }
            else
            {
                leftPane = new Panel(new Text("No image preview available", new Style(Color.Grey)))
                {
                    Border = BoxBorder.Rounded
                };
            }
        }
        else
        {
            leftPane = new Panel(new Text("No image URL provided", new Style(Color.Grey)))
            {
                Border = BoxBorder.Rounded
            };
        }

        // Right Pane: Properties Table (Zero empty values)
        var propTable = new Table().Border(TableBorder.Rounded);
        propTable.AddColumn(new TableColumn("[bold yellow]Property[/]").Width(22));
        propTable.AddColumn(new TableColumn("[bold yellow]Details[/]"));

        var properties = drink.GetNonEmptyProperties();
        foreach (var (key, value) in properties)
        {
            propTable.AddRow($"[bold silver]{Markup.Escape(key)}[/]", Markup.Escape(value));
        }

        grid.AddRow(leftPane, propTable);
        AnsiConsole.Write(grid);
        AnsiConsole.WriteLine();

        // Ingredients Table
        var ingredients = drink.GetIngredients();
        if (ingredients.Count > 0)
        {
            var ingTable = new Table().Border(TableBorder.Rounded).Title("[bold green]Recipe & Ingredients[/]");
            ingTable.AddColumn(new TableColumn("[bold teal]Ingredient[/]").Width(30));
            ingTable.AddColumn(new TableColumn("[bold teal]Measure[/]"));

            foreach (var (ingredient, measure) in ingredients)
            {
                ingTable.AddRow(
                    Markup.Escape(ingredient),
                    Markup.Escape(measure ?? "-")
                );
            }

            AnsiConsole.Write(ingTable);
            AnsiConsole.WriteLine();
        }

        if (!string.IsNullOrWhiteSpace(drink.Instructions))
        {
            var instructionsPanel = new Panel(new Markup($"[italic]{Markup.Escape(drink.Instructions)}[/]"))
            {
                Header = new PanelHeader("[bold gold1]Preparation Instructions[/]"),
                Border = BoxBorder.Double
            };
            AnsiConsole.Write(instructionsPanel);
            AnsiConsole.WriteLine();
        }
    }
}
