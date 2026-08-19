using System;
using System.IO;
using DrinksApp.Services;
using Xunit;

namespace DrinksApp.Tests;

public class StorageServiceTests : IDisposable
{
    private readonly string _testFilePath;

    public StorageServiceTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"drinks_test_{Guid.NewGuid():N}.json");
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    [Fact]
    public void ToggleFavorite_AddsAndRemovesCorrectly()
    {
        var storage = new StorageService(_testFilePath);
        Assert.False(storage.IsFavorite("11007"));

        bool added = storage.ToggleFavorite("11007");
        Assert.True(added);
        Assert.True(storage.IsFavorite("11007"));

        // Test persistence by reloading from disk
        var newStorageInstance = new StorageService(_testFilePath);
        Assert.True(newStorageInstance.IsFavorite("11007"));

        bool removed = newStorageInstance.ToggleFavorite("11007");
        Assert.False(removed);
        Assert.False(newStorageInstance.IsFavorite("11007"));
    }

    [Fact]
    public void RecordDrinkView_IncrementsCountAndOrdersTopViewed()
    {
        var storage = new StorageService(_testFilePath);
        storage.RecordDrinkView("1", "Drink One");
        storage.RecordDrinkView("1", "Drink One");
        storage.RecordDrinkView("2", "Drink Two");
        storage.RecordDrinkView("2", "Drink Two");
        storage.RecordDrinkView("2", "Drink Two");

        Assert.Equal(2, storage.GetViewCount("1"));
        Assert.Equal(3, storage.GetViewCount("2"));
        Assert.Equal(0, storage.GetViewCount("3"));

        var top = storage.GetTopViewedDrinks(10);
        Assert.Equal(2, top.Count);
        Assert.Equal("2", top[0].DrinkId);
        Assert.Equal(3, top[0].ViewCount);
        Assert.Equal("1", top[1].DrinkId);
        Assert.Equal(2, top[1].ViewCount);
    }
}
