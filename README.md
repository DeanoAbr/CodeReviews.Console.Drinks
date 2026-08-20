# 🍸 Drinks Console App

A .NET 8 console application for restaurant staff to browse the restaurant's drinks menu.
Drink data is fetched live from [TheCocktailDB](https://www.thecocktaildb.com/api.php) public API —
no local database is used. Built with **Spectre.Console** for a modern terminal UI.

## ✅ How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Internet connection (the app calls TheCocktailDB API)
- A terminal that supports Unicode (Windows Terminal recommended) for emoji/icons

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/the-csharp-academy/CodeReviews.Console.Drinks.git
   cd CodeReviews.Console.Drinks
   ```
2. Restore and run:
   ```bash
   dotnet restore
   dotnet run --project DrinksApp.csproj
   ```
3. Use the arrow keys to navigate the menus, Enter to select, and type to search.

### Run the tests
```bash
dotnet test
```
All 8 unit tests should pass (model filtering, favorites/view-count persistence, API error resilience).

### Tip: first-time setup
If emojis render as boxes, run in Windows Terminal and set the font to a color-emoji font
(e.g. "Segoe UI Emoji").

## 📖 What the App Does & How It Works

The app is a menu portal for restaurant employees:

1. **Browse by Category** — pick a drink category (e.g. *Ordinary Drink*, *Cocktail*);
   the app lists every drink in that category.
2. **Search by Name** — search the whole database for a drink.
3. **Random Drink** — discover a random drink for inspiration.
4. **Drink Details** — view a drink's properties (glass, alcoholic, IBA, tags...),
   its **recipe** (ingredients paired with measures), **preparation instructions**,
   and a **terminal-rendered photo** of the drink.
   Properties with empty values are never shown.
5. **Favorites** ❤️ — add/remove drinks to a favorites list that persists between sessions.
6. **Most Popular** 📊 — a leaderboard of the most-viewed drinks, tracked per drink.

### How it works under the hood
- All drink data comes from **TheCocktailDB REST API** (`https://www.thecocktaildb.com/api/json/v1/1/`)
  over HTTP, using .NET's built-in `HttpClient` (no third-party REST library).
- Every API call is wrapped in error handling: if the API is down, times out, or returns
  malformed data, the app shows a friendly message instead of crashing.
- Favorites and view counts are stored locally in `drinks_userdata.json`,
  written next to the executable.

### API endpoints used
| Endpoint | Purpose |
|---|---|
| `list.php?c=list` | Fetch all drink categories |
| `filter.php?c={category}` | Fetch drinks in a category |
| `lookup.php?i={id}` | Fetch full details of one drink by ID |
| `search.php?s={query}` | Search drinks by name |
| `random.php` | Fetch a random drink |

## 🏗️ Architectural Choices

The project follows a simple **layered architecture** so each concern stays isolated:

| Layer | Folder | Responsibility |
|---|---|---|
| Models | `src/Models/` | Plain C# classes mapped from the API JSON via Newtonsoft.Json (`Category`, `DrinkItem`, `DrinkDetail`, `UserData`) |
| Services | `src/Services/` | `DrinksApiService` (all HTTP calls + error handling), `StorageService` (JSON file persistence) |
| UI | `src/UI/` | `DrinkDetailView` — pure presentation: tables, panels, `CanvasImage` thumbnail rendering |
| Controllers | `src/Controllers/` | `DrinksController` — user navigation/menu flow, decides *what* to show |
| Entry point | `src/Program.cs` | Wires services together, top-level exception guard |

Key decisions:
- **`HttpClient` over a REST client library** — it's the .NET standard for HTTP and needs no extra package.
- **Newtonsoft.Json** for deserialization because the API's JSON is loose/snake_case;
  attributes like `[JsonProperty("strDrink")]` map it cleanly to readable C# properties.
- **Spectre.Console** for the UI — tables, selection prompts, status spinners, and `CanvasImage`
  (via `Spectre.Console.ImageSharp`) which renders drink photos in the terminal.
- **Detail view shows only non-empty properties** — `DrinkDetail.GetNonEmptyProperties()`
  filters out null/empty/whitespace values before rendering (reviewer requirement).
- **Favorites + view counts without SQL** — a tiny JSON file (`StorageService`) is enough for
  single-user local data; no database dependency needed.
- **Async everywhere** — all API calls are `async` so the UI stays responsive while waiting on the network.

## 🧠 Reflection

This was my first time actually working with a third-party API, and I learned more from it than I did from any tutorial. Real-world JSON can be quite messy. TheCocktailDB doesn't just hand you clean, tidy objects, you get `strIngredient1` all the way up to `strIngredient15`, and untangling that made me actually stop and think about data design instead of just bashing things together until it worked. I landed on explicit properties plus a helper method that pairs each ingredient with its measure. Kept things readable, kept the UI logic from turning into spaghetti.

Error handling was a bigger headache than I thought it'd be. The API crashed, timed out and empty result sets needed their own responses. The app can't not work because the server is struggling. So I wrapped every call at the service layer and had it return structured results, which meant the UI never had to sit there guessing what went wrong.

The most fun part was getting `Spectre.Console.ImageSharp` working. The first time an actual drink photo rendered in the terminal I was amped. The console keeps surprising me with what it can handle.

If I went back and did it again, I'd pull out an interface for the API client so I could mock the service without dragging in a real HTTP layer. I'd also probably swap persistence over to SQLite once the data grows past what a single JSON file can comfortably hold — favorites and view counts are fine for now, but they'd hold up a lot better in a proper database.

## 📝 Project Structure

```
CodeReviews.Console.Drinks/
├── DrinksApp.csproj              # .NET 8 project (Newtonsoft.Json, Spectre.Console)
├── src/
│   ├── Program.cs                # Entry point — wires services, global exception guard
│   ├── Models/                   # Category, DrinkItem, DrinkDetail, UserData
│   ├── Services/                 # DrinksApiService, StorageService
│   ├── Controllers/              # DrinksController (navigation)
│   └── UI/                       # DrinkDetailView (Spectre.Console rendering)
└── tests/
    └── DrinksApp.Tests/          # xUnit tests (8 tests)
```

---

*Built with .NET 8 · Spectre.Console · TheCocktailDB API*
