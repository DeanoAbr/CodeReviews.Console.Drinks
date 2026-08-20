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

Building this app was my first real experience consuming a third-party REST API, and it taught me more about real-world development than any tutorial. The biggest surprise was how untidy production JSON actually is. TheCocktailDB doesn't hand you clean objects — it gives you `strIngredient1` through `strIngredient15`, and deciding how to model that forced me to think hard about data design. I went with explicit properties plus a helper method that pairs ingredients with their measures, which keeps the model readable and the UI logic simple.

Error handling also took more thought than I expected. A crashed API, a timeout, and an empty result set are three completely different failures, and each needs a different, friendly response — the app shouldn't just die because the vendor had a bad day. Wrapping every call in the service layer and returning structured results meant the UI never has to guess what went wrong.

The most fun part was `Spectre.Console.ImageSharp`. Rendering an actual drink photo in the terminal felt like magic the first time it worked, and it turned a plain menu app into something you can proudly show a restaurant owner.

If I did it again, I'd extract an interface for the API client so the service is mockable without an HTTP layer, and I'd probably switch persistence to SQLite once the data grows beyond a single JSON file. Favorites and view counts work great now, but they'd scale better in a real database.

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
