# ReelTime

ReelTime is a movie browser built with **Blazor WebAssembly** on **.NET 10**. It uses the free [TMDB (The Movie Database)](https://www.themoviedb.org/) API to show what's now playing, what's popular, search results, and details for each movie (cast, trailer, and more). You can also save favorites, and they stay in your browser.

This README is written for someone new to .NET and Blazor. It explains what the pieces are, where things live, and how a request travels through the app.

---

## 1. What is Blazor WebAssembly?

Normally you'd write a website's interactive parts in JavaScript. **Blazor** lets you write them in **C#** instead.

- **Components**: a page, or a piece of a page, is a `.razor` file. It mixes HTML with C#.
- **WebAssembly**: when someone opens the site, their browser downloads the .NET runtime plus our compiled code and runs it locally. There is **no server running C# code**. The site is just static files (HTML, CSS, DLLs) that any static host can serve.

Because everything runs in the browser, the app can't hide secrets. That's why the TMDB API key is handled differently in development and in production (see [Configuration](#7-the-tmdb-api-key-and-configuration)).

---

## 2. Running it

**You need:** the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run             # http://localhost:5262
dotnet run -lp https   # https://localhost:7170
dotnet watch           # auto-reload while you edit
```

Or open `ReelTime.slnx` in Visual Studio and press **F5**.

You'll also need a TMDB access token to see movies locally. See [section 7](#7-the-tmdb-api-key-and-configuration).

---

## 3. Project layout

```
ReelTime/
├── ReelTime.csproj           Project file: SDK type, .NET version, NuGet packages
├── ReelTime.slnx             Solution file (Visual Studio opens this)
├── Program.cs                Entry point: starts the app and registers services
├── App.razor                 The router: maps URLs to pages
├── _Imports.razor            Shared "using" statements for every .razor file
│
├── Components/
│   ├── Layout/               Page "frames": nav bar, footer, wrappers
│   │   ├── TopNavLayout      Default layout: top nav + content + footer
│   │   ├── TopNavMenu        The navigation bar (includes the search box)
│   │   ├── TopNavFooter      The footer
│   │   ├── HomeLayout        Special layout used only by the Home page
│   │   └── MainLayout, NavMenu   Leftover sidebar layout from the template
│   │
│   ├── Pages/                One file per URL (these have an @page line)
│   │   ├── Home.razor            /
│   │   ├── NowPlayingPage.razor  /now-playing
│   │   ├── Popular.razor         /popular
│   │   ├── Search.razor          /search?query=...
│   │   ├── MovieById.razor       /movie/{id}
│   │   ├── Favorites.razor       /favorites
│   │   ├── Themes.razor          /themes
│   │   └── NotFound.razor        /not-found
│   │
│   └── UI/                   Small reusable pieces used inside pages
│       ├── MovieCard.razor       One movie poster card
│       └── ActorSwiper/          Scrolling row of cast members
│
├── Models/                   Plain C# classes that describe TMDB's JSON
│   ├── Movie, MovieDetails, MovieListResponse
│   ├── Cast, Crew, CreditsResponse
│   └── Videos, MovieVideoResponse
│
├── Services/                 Classes that do work (API calls, storage)
│   ├── TMDBService.cs        Talks to the TMDB API
│   └── FavoriteService.cs    Saves favorites in the browser's localStorage
│
├── wwwroot/                  Static files, served exactly as they are
│   ├── index.html            The one real HTML page
│   ├── css/app.css           Global styles
│   ├── css/themes.css        Color themes
│   ├── images/               Logos, placeholder poster, etc.
│   ├── lib/bootstrap/        Bootstrap CSS/JS
│   └── _redirects            Netlify routing rules
│
├── netlify/edge-functions/   tmdb.js: a small proxy that adds the API key on Netlify
├── netlify.build.sh          Netlify build script (installs .NET and publishes)
├── .github/workflows/        deploy.yml: GitHub Pages deployment
└── Properties/launchSettings.json   Local ports and environment
```

---

## 4. How the app starts

1. The browser loads **`wwwroot/index.html`**. It's an almost empty page with a loading spinner inside `<div id="app">`.
2. `index.html` loads `_framework/blazor.webassembly.js`. That downloads the .NET runtime and our compiled code.
3. **`Program.cs`** runs:
   ```csharp
   builder.RootComponents.Add<App>("#app");          // draw the App component inside #app
   builder.Services.AddScoped<TMDBService>();         // register our services
   builder.Services.AddScoped<FavoritesService>();
   ```
4. **`App.razor`** contains a `<Router>`. It looks at the URL, finds the page whose `@page "..."` matches, and draws it inside the default layout.

---

## 5. Key Blazor ideas (as used here)

### Components and routing
Every `.razor` file is a component. If it has `@page "/popular"` at the top, it becomes a page reachable at `/popular`. Parameters in the route work too: `@page "/movie/{movieId:int}"` captures the movie ID from the URL.

### Layouts
A layout is a frame around a page. It has a `@Body` placeholder where the page goes. The default is `TopNavLayout` (set in `App.razor`): nav bar on top, page in the middle, footer at the bottom. `Home.razor` opts into a different layout with `@layout HomeLayout`.

### `@code` blocks
C# for a component goes in an `@code { ... }` block at the bottom of its `.razor` file. A tiny example:

```razor
<button @onclick="Add">Clicked @count times</button>

@code {
    private int count;
    private void Add() => count++;   // Blazor re-draws the page after the click
}
```

### Dependency injection (`@inject`)
Services are registered once in `Program.cs`. Any component asks for one by type:

```razor
@inject TMDBService TMDBService
```

Blazor gives it a ready-made instance. This is how every page shares the same API code.

### Lifecycle: loading data
Pages fetch data in `OnInitializedAsync` (or `OnParametersSetAsync` when a URL parameter can change, like the search query). The usual pattern is:
1. Start with `null` and show "Loading...".
2. `await` the API call.
3. When the data arrives, Blazor re-draws and shows the results.

### Parameters
A component receives data from its parent through `[Parameter]` properties, for example `<MovieCard Movie="movie" />` or `<ActorSwiper Actors="cast" />`.

`[SupplyParameterFromQuery]` does the same for the URL's query string. `Search.razor` uses it to read `?query=batman`.

### Scoped CSS
A file like `MovieCard.razor.css` styles **only** `MovieCard.razor`. Blazor rewrites the selectors at build time so they can't leak into other components. They're bundled into `ReelTime.styles.css`, which `index.html` links.

### JavaScript interop
Sometimes C# needs the browser's help. `FavoritesService` calls `localStorage.setItem` through `IJSRuntime`. Components can also have a matching `.razor.js` file (like `ActorSwiper.razor.js`), which is loaded on demand with `JS.InvokeAsync<IJSObjectReference>("import", ...)`.

### `_Imports.razor`
Holds `@using` lines that apply to every `.razor` file. That's why pages can use `TMDBService` or `Movie` without importing them one by one.

---

## 6. How data flows (example: searching)

1. You type in the search box in `TopNavMenu.razor` and click **Search**.
2. `HandleSearch()` calls `Nav.NavigateTo("search?query=...")`. No page reload happens.
3. The router matches `/search` and draws `Search.razor`.
4. `[SupplyParameterFromQuery] Query` is filled in from the URL.
5. `OnParametersSetAsync` calls `TMDBService.SearchMovies(Query)`.
6. `TMDBService` sends an HTTP GET to TMDB and converts the JSON into C# objects (`MovieListResponse`, made up of `Movie` objects from `Models/`).
7. `Search.razor` loops over the results and draws a `<MovieCard>` for each one.
8. **Add Fav** on a card calls `FavoritesService`, which stores the list in the browser's localStorage. `Favorites.razor` reads it back.

### Services

| Service | What it does |
|---|---|
| `TMDBService` | `GetNowPlayingMovies`, `GetPopularMovies`, `SearchMovies`, `GetMovieById`, `GetMovieCredits`, `GetMovieTrailer`. It also fixes poster paths and falls back to `/images/poster.png`. |
| `FavoritesService` | `GetFavorites`, `AddFavorite`, `RemoveFavorite`, `IsFavorite`, `SaveFavorites`, using localStorage |

### Models
Models are plain classes whose properties match TMDB's JSON. TMDB uses `snake_case` names (like `poster_path`). `TMDBService` is set up to map those to C# `PascalCase` properties (like `PosterPath`) automatically.

---

## 7. The TMDB API key and configuration

Anything in a WebAssembly app can be read by anyone who opens the browser's dev tools, so a secret key can't be shipped in the app. `TMDBService` handles this with two modes:

| Where | How it works |
|---|---|
| **Local development** | You put your TMDB token in `wwwroot/appsettings.Development.json` as `"TmdbAccessKey"`. The service then calls `api.themoviedb.org` directly. This file is **git-ignored**, so it never gets committed. |
| **Deployed (Netlify)** | No key is shipped. The app calls `/tmdb/...` on its own site, and a Netlify **edge function** (`netlify/edge-functions/tmdb.js`) adds the key on the server and forwards the request to TMDB. The key lives in Netlify's environment variables (`API_KEY` and `API_URL`). |

To set up local development, create `wwwroot/appsettings.Development.json`:

```json
{
  "TmdbAccessKey": "your-tmdb-read-access-token"
}
```

Get a token from your TMDB account settings (API → "API Read Access Token").

---

## 8. Styling and themes

- **Bootstrap 5** (in `wwwroot/lib`) gives the grid, buttons, cards, and utility classes like `d-flex` and `py-3`.
- **Bootstrap Icons** (`bi bi-...`) come from a CDN.
- **`css/app.css`** has global styles and Blazor's default error banner and loading spinner.
- **`css/themes.css`** defines color themes using CSS variables (such as `--cf-theme-800`). `<body data-cf-theme="grey">` picks the active one. The `/themes` page lets you try them, and a small script in `index.html` (`setTheme`) switches it.
- **Scoped `.razor.css` files** style a single component (see above).

---

## 9. Deployment

The project can be built into plain static files:

```bash
dotnet publish ReelTime.csproj -c Release -o release
# the website is in release/wwwroot
```

- **Netlify** uses `netlify.build.sh` to install .NET and run the publish. The `_redirects` file sends every URL to `index.html`, so deep links like `/movie/550` work after a refresh. The `/TMDB/*` route goes to the edge function.
- **GitHub Pages** is handled by `.github/workflows/deploy.yml`, which publishes and rewrites `<base href>` for the `/ReelTime/` path. That route has no edge function, so the TMDB key would have to be handled some other way there.

---

## 10. Common tasks

**Add a new page**
1. Create `Components/Pages/MyPage.razor` with `@page "/my-page"` at the top.
2. Add a `<NavLink href="my-page">` in `Components/Layout/TopNavMenu.razor`.

**Add a new TMDB call**
1. Add a method to `Services/TMDBService.cs` (follow `GetPopularMovies`).
2. If the JSON has new fields, add or extend a class in `Models/`.
3. In a page, `@inject TMDBService TMDBService` and call it from `OnInitializedAsync`.

**Add a reusable piece of UI**
Create `Components/UI/MyThing.razor`, give it `[Parameter]` properties, and use it as `<MyThing ... />` anywhere. A `MyThing.razor.css` next to it gives it its own styles.

---

## 11. Troubleshooting

- **A page shows an error bar saying "An unhandled error has occurred".** Open the browser console (F12) for the exception message.
- **Movies don't load locally.** Check that `wwwroot/appsettings.Development.json` exists with a valid token.
- **Changes don't appear.** Hot reload isn't perfect, especially for `@implements`, `@inject`, and `Program.cs` changes. Stop and restart.
- **Refreshing a deep link gives a 404.** The host needs an "everything goes to `index.html`" rule (`_redirects` on Netlify, the `404.html` copy on GitHub Pages).
