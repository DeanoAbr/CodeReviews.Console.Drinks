using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DrinksApp.Services;
using Xunit;

namespace DrinksApp.Tests;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;
    private readonly bool _throwNetworkException;

    public MockHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
        _throwNetworkException = false;
    }

    public MockHttpMessageHandler(bool throwNetworkException)
    {
        _throwNetworkException = throwNetworkException;
        _response = new HttpResponseMessage();
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_throwNetworkException)
        {
            throw new HttpRequestException("Simulated network connection failure");
        }
        return Task.FromResult(_response);
    }
}

public class DrinksApiServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_ReturnsCategoriesOnValidJson()
    {
        var json = @"{""drinks"":[{""strCategory"":""Ordinary Drink""},{""strCategory"":""Cocktail""}]}";
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        using var service = new DrinksApiService(new HttpClient(handler));

        var (success, categories, error) = await service.GetCategoriesAsync();

        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);
        Assert.Equal("Cocktail", categories[0].Name); // Sorted alphabetically
        Assert.Equal("Ordinary Drink", categories[1].Name);
    }

    [Fact]
    public async Task GetCategoriesAsync_HandlesNetworkFailureGracefully()
    {
        var handler = new MockHttpMessageHandler(throwNetworkException: true);
        using var service = new DrinksApiService(new HttpClient(handler));

        var (success, categories, error) = await service.GetCategoriesAsync();

        Assert.False(success);
        Assert.Null(categories);
        Assert.NotNull(error);
        Assert.Contains("Network error", error);
    }

    [Fact]
    public async Task GetDrinkByIdAsync_ReturnsDrinkOnValidJson()
    {
        var json = @"{""drinks"":[{""idDrink"":""11007"",""strDrink"":""Margarita"",""strCategory"":""Ordinary Drink"",""strGlass"":""Cocktail glass""}]}";
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        using var service = new DrinksApiService(new HttpClient(handler));

        var (success, drink, error) = await service.GetDrinkByIdAsync("11007");

        Assert.True(success);
        Assert.Null(error);
        Assert.NotNull(drink);
        Assert.Equal("11007", drink.Id);
        Assert.Equal("Margarita", drink.Name);
    }

    [Fact]
    public async Task GetDrinkByIdAsync_HandlesNotFoundGracefully()
    {
        var json = @"{""drinks"":null}";
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        using var service = new DrinksApiService(new HttpClient(handler));

        var (success, drink, error) = await service.GetDrinkByIdAsync("999999");

        Assert.False(success);
        Assert.Null(drink);
        Assert.NotNull(error);
        Assert.Contains("not found", error);
    }
}
