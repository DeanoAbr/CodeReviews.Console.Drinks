using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrinksApp.Models;
using Newtonsoft.Json;

namespace DrinksApp.Services;

public class StorageService
{
    private readonly string _filePath;
    private UserData _data;

    public StorageService(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "drinks_userdata.json");
        _data = LoadData();
    }

    private UserData LoadData()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var data = JsonConvert.DeserializeObject<UserData>(json);
                if (data != null)
                {
                    return data;
                }
            }
        }
        catch
        {
            // Silently recover with clean state if corrupt
        }

        return new UserData();
    }

    private void SaveData()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Fail silently or handle storage error
        }
    }

    public bool IsFavorite(string drinkId)
    {
        return _data.FavoriteDrinkIds.Contains(drinkId);
    }

    public bool ToggleFavorite(string drinkId)
    {
        bool isFav;
        if (_data.FavoriteDrinkIds.Contains(drinkId))
        {
            _data.FavoriteDrinkIds.Remove(drinkId);
            isFav = false;
        }
        else
        {
            _data.FavoriteDrinkIds.Add(drinkId);
            isFav = true;
        }
        SaveData();
        return isFav;
    }

    public HashSet<string> GetFavoriteIds()
    {
        return new HashSet<string>(_data.FavoriteDrinkIds, StringComparer.OrdinalIgnoreCase);
    }

    public void RecordDrinkView(string drinkId, string drinkName)
    {
        if (_data.ViewStats.TryGetValue(drinkId, out var info))
        {
            info.ViewCount++;
            info.DrinkName = drinkName; // Update name in case it changed
        }
        else
        {
            _data.ViewStats[drinkId] = new DrinkViewInfo
            {
                DrinkId = drinkId,
                DrinkName = drinkName,
                ViewCount = 1
            };
        }
        SaveData();
    }

    public int GetViewCount(string drinkId)
    {
        if (_data.ViewStats.TryGetValue(drinkId, out var info))
        {
            return info.ViewCount;
        }
        return 0;
    }

    public List<DrinkViewInfo> GetTopViewedDrinks(int count = 10)
    {
        return _data.ViewStats.Values
            .OrderByDescending(v => v.ViewCount)
            .Take(count)
            .ToList();
    }
}
