using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DrinksApp.Models;
using Newtonsoft.Json;

namespace DrinksApp.Services;

public class DrinksApiService : IDisposable
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.thecocktaildb.com/api/json/v1/1/";

    public DrinksApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    public async Task<(bool Success, List<Category>? Categories, string? ErrorMessage)> GetCategoriesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}list.php?c=list");
            var result = JsonConvert.DeserializeObject<CategoryResponse>(response);
            var categories = result?.Categories?.OrderBy(c => c.Name).ToList() ?? new List<Category>();
            return (true, categories, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Network error while connecting to Drinks API: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, null, "Request timed out while contacting Drinks API.");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<(bool Success, List<DrinkItem>? Drinks, string? ErrorMessage)> GetDrinksByCategoryAsync(string category)
    {
        try
        {
            var encodedCategory = Uri.EscapeDataString(category);
            var response = await _httpClient.GetStringAsync($"{BaseUrl}filter.php?c={encodedCategory}");
            var result = JsonConvert.DeserializeObject<DrinkListResponse>(response);
            var drinks = result?.Drinks?.OrderBy(d => d.Name).ToList() ?? new List<DrinkItem>();
            return (true, drinks, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Network error while fetching category drinks: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, null, "Request timed out while contacting Drinks API.");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<(bool Success, DrinkDetail? Drink, string? ErrorMessage)> GetDrinkByIdAsync(string drinkId)
    {
        try
        {
            var encodedId = Uri.EscapeDataString(drinkId);
            var response = await _httpClient.GetStringAsync($"{BaseUrl}lookup.php?i={encodedId}");
            var result = JsonConvert.DeserializeObject<DrinkDetailResponse>(response);
            var drink = result?.Drinks?.FirstOrDefault();
            if (drink == null)
            {
                return (false, null, $"Drink with ID '{drinkId}' was not found.");
            }
            return (true, drink, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Network error while fetching drink details: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, null, "Request timed out while contacting Drinks API.");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<(bool Success, List<DrinkDetail>? Drinks, string? ErrorMessage)> SearchDrinksByNameAsync(string query)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var response = await _httpClient.GetStringAsync($"{BaseUrl}search.php?s={encodedQuery}");
            var result = JsonConvert.DeserializeObject<DrinkDetailResponse>(response);
            var drinks = result?.Drinks?.OrderBy(d => d.Name).ToList() ?? new List<DrinkDetail>();
            return (true, drinks, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Network error while searching drinks: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, null, "Request timed out while contacting Drinks API.");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<(bool Success, DrinkDetail? Drink, string? ErrorMessage)> GetRandomDrinkAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync($"{BaseUrl}random.php");
            var result = JsonConvert.DeserializeObject<DrinkDetailResponse>(response);
            var drink = result?.Drinks?.FirstOrDefault();
            if (drink == null)
            {
                return (false, null, "Could not fetch a random drink at this time.");
            }
            return (true, drink, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, $"Network error while fetching random drink: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, null, "Request timed out while contacting Drinks API.");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    public async Task<byte[]?> DownloadImageBytesAsync(string imageUrl)
    {
        try
        {
            return await _httpClient.GetByteArrayAsync(imageUrl);
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
