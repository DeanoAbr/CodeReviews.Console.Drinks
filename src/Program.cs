using System;
using System.Threading.Tasks;
using DrinksApp.Controllers;
using DrinksApp.Services;
using Spectre.Console;

namespace DrinksApp;

public static class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        using var apiService = new DrinksApiService();
        var storageService = new StorageService();
        var controller = new DrinksController(apiService, storageService);

        try
        {
            await controller.RunAsync();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            AnsiConsole.MarkupLine("[red]An unhandled error occurred. Press any key to exit.[/]");
            Console.ReadKey(true);
        }
    }
}
