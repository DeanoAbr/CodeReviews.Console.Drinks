using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DrinksApp
{
    public class DrinksApi
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string apiUrl = "https://www.thecocktaildb.com/api/json/v1/1/";

        public static async Task<List<Drink>> GetCategoriesAsync()
        {
            var response = await client.GetStringAsync(apiUrl + "categories.php");
            return JsonConvert.DeserializeObject<List<Drink>>(response);
        }

        public static async Task<List<Drink>> GetDrinksByCategoryAsync(string category)
        {
            var response = await client.GetStringAsync(apiUrl + "filter.php?c=" + category);
            return JsonConvert.DeserializeObject<List<Drink>>(response);
        }

        public static async Task<Drink> GetDrinkDetailsAsync(string drinkName)
        {
            var response = await client.GetStringAsync(apiUrl + "search.php?s=" + drinkName);
            var drinks = JsonConvert.DeserializeObject<List<Drink>>(response);
            return drinks.FirstOrDefault();
        }
    }
}
