# BlazorTemplate

A starter template for **Blazor WebAssembly** apps on **.NET 10**. It runs entirely in the browser (no server-side rendering) and comes with a sidebar layout, a blank top-nav layout to build on, Bootstrap 5, Bootstrap Icons, Devicons, Google Fonts, and a set of CSS variables for theming.

The project and root namespace are named `ReelTime`.

This can be turned into a Template Package (`.nupkg`) for Visual Studio, or you can just clone it and rename the project to start a new app.
 
New information. 
ss
This is a test of GIT

---

## Getting started

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

```bash
git clone https://github.com/PangeaMade/BlazorTemplate.git
cd BlazorTemplate
dotnet run            # http://localhost:5262
dotnet run -lp https  # https://localhost:7170
```

For hot reload during development:

```bash
dotnet watch
```

To produce a static build you can host anywhere (Azure Static Web Apps, GitHub Pages, Netlify, S3, IIS, etc.):

```bash
dotnet publish -c Release
# output: bin/Release/net10.0/publish/wwwroot
```

### Starting a new project from this template

1. Click **Use this template** on GitHub (or clone and re-init git).
2. Optionally rename `ReelTime.csproj`, `ReelTime.slnx`, and the `ReelTime` namespace used in `Program.cs` and `_Imports.razor`. If you rename the project, update the `ReelTime.styles.css` link in `wwwroot/index.html` to match the new project name.
3. Update the page `<title>` in `wwwroot/index.html` and the brand text in `Layout/NavMenu.razor`.
4. Change the theme variables at the top of `wwwroot/css/app.css`.

---

## Project structure

```
BlazorTemplate/
├── ReelTime.slnx              # Solution file (new XML .slnx format)
├── ReelTime.csproj            # Project file – Blazor WebAssembly SDK, net10.0
├── Program.cs                     # App entry point – builds and runs the WASM host
├── App.razor                      # Root component – the router
├── _Imports.razor                 # Global @using directives for all .razor files
│
├── Layout/                        # Shared page shells
│   ├── MainLayout.razor           # Default layout: sidebar + top row + content
│   ├── MainLayout.razor.css       # Scoped styles for MainLayout
│   ├── NavMenu.razor              # Sidebar navigation (collapsible on mobile)
│   ├── NavMenu.razor.css          # Scoped styles for NavMenu
│   └── TopNavLayout.razor         # Alternate layout skeleton: top nav / main / footer
│
├── Pages/                         # Routable components (one per URL)
│   ├── Home.razor                 # /
│   ├── Counter.razor              # /counter  – interactivity example
│   ├── Weather.razor              # /weather  – data-fetching example
│   └── NotFound.razor             # /not-found – shown for unknown routes
│
├── Properties/
│   └── launchSettings.json        # Local dev profiles (http :5262, https :7170)
│
└── wwwroot/                       # Static files, served as-is
    ├── index.html                 # Host page – loads CSS, fonts, and the Blazor runtime
    ├── css/app.css                # Global styles + theme variables
    ├── lib/bootstrap/             # Bootstrap 5 (bundled locally)
    ├── sample-data/weather.json   # Mock data for the Weather page
    ├── favicon.png
    └── icon-192.png
```

---

## How it works

### 1. Startup: `wwwroot/index.html` → `Program.cs`

When a browser requests the site, it gets the static **`index.html`**. That page:

- Loads stylesheets: local Bootstrap, Bootstrap Icons and Devicons (CDN), Google Fonts (Bebas Neue and Montserrat), `css/app.css`, and `ReelTime.styles.css` (the bundle of every component's scoped `.razor.css` file, generated at build time).
- Contains a `<div id="app">` holding a loading spinner. It shows while the .NET runtime downloads.
- Contains a hidden `#blazor-error-ui` banner that Blazor reveals if an unhandled exception occurs.
- Loads `_framework/blazor.webassembly.js`, which downloads the .NET runtime and your compiled app into the browser as WebAssembly.

Because the `.csproj` sets `OverrideHtmlAssetPlaceholders`, the build fills in the `#[.{fingerprint}]` placeholder, the empty `<script type="importmap">`, and the `<link rel="preload" id="webassembly">` tag. This gives assets cache-busting fingerprints and preloads the runtime.

Once the runtime starts, it runs **`Program.cs`**:

```csharp
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");               // render App into <div id="app">
builder.RootComponents.Add<HeadOutlet>("head::after");  // lets pages set <PageTitle>/<HeadContent>

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
```

- `App` replaces the spinner inside `#app`.
- `HeadOutlet` is what makes `<PageTitle>` in a page update the browser tab title.
- An `HttpClient` is registered for dependency injection. Its base address is the site's own URL, so relative paths like `sample-data/weather.json` resolve correctly. Register your own services here too.

### 2. Routing: `App.razor`

```razor
<Router AppAssembly="@typeof(App).Assembly" NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
</Router>
```

- The `Router` scans the assembly for every component with an `@page "/path"` directive and matches it against the current URL.
- On a match, `RouteView` renders the page inside **`MainLayout`**, unless the page picks a different layout with `@layout`.
- `FocusOnNavigate` moves keyboard focus to the page's `<h1>` after navigation, which helps screen reader users.
- `NotFoundPage` (new in .NET 10) renders `Pages/NotFound.razor` when no route matches.

Navigation is client-side. Clicking a link swaps the page component without reloading the whole document.

### 3. Layouts: `Layout/`

A layout is a component that inherits `LayoutComponentBase` and places `@Body` where the page content should go.

**`MainLayout.razor`** (the default) is a two-column shell:

```
┌───────────┬──────────────────────────────────┐
│           │  top-row (About link)            │
│  NavMenu  ├──────────────────────────────────┤
│ (sidebar) │  <article class="content">       │
│           │      @Body   ← page renders here │
│           │                                  │
└───────────┴──────────────────────────────────┘
```

Its styles live in `MainLayout.razor.css`. On narrow screens the sidebar stacks above the content.

**`NavMenu.razor`** is the sidebar. It uses `NavLink`, which works like `<a>` but adds the `active` CSS class when its `href` matches the current URL. `Match="NavLinkMatch.All"` on Home means it is only active on exactly `/`, not on every URL. A small `@code` block toggles a `collapse` class so the menu opens and closes via the hamburger button on mobile.

**`TopNavLayout.razor`** is an empty skeleton for a top-navigation design. It's a full-height flex column with placeholders for a nav bar, a growing `<main>`, and a footer. To use it, fill in the placeholders and either add `@layout TopNavLayout` to individual pages or change `DefaultLayout` in `App.razor`.

### 4. Pages: `Pages/`

Each page is a Razor component with markup and an optional `@code { }` block of C#.

| Page | Route | Shows you how to… |
|---|---|---|
| `Home.razor` | `/` | Make a basic static page and set the tab title with `<PageTitle>` |
| `Counter.razor` | `/counter` | Handle events: `@onclick` calls a C# method, a field changes, and Blazor re-renders |
| `Weather.razor` | `/weather` | Inject a service (`@inject HttpClient Http`), load data in `OnInitializedAsync`, show a loading state while it's `null`, and render a table with `@foreach` |
| `NotFound.razor` | `/not-found` | Build a fallback page, with `@layout MainLayout` set explicitly |

`Weather.razor` loads `wwwroot/sample-data/weather.json` with `Http.GetFromJsonAsync<WeatherForecast[]>(...)`. To use a real API, change that URL, or register a separate `HttpClient` with the API's base address in `Program.cs`.

**Adding a page:** create `Pages/MyPage.razor` with `@page "/my-page"` at the top, then add a matching `NavLink` in `NavMenu.razor`.

### 5. Styling

The template uses three kinds of styling:

1. **Bootstrap 5** (`wwwroot/lib/bootstrap`) for the grid, utilities, and components, plus **Bootstrap Icons** (`bi bi-*`) and **Devicons** (`devicon-*`) from CDNs.
2. **Global CSS** in `wwwroot/css/app.css`. The top of the file defines theme variables:

   ```css
   :root {
       --cf-title-font: 'Bebas Neue', sans-serif;  /* h1–h6 and .h1–.h6 */
       --cf-body-font:  'Montserrat', sans-serif;  /* html, body */
       --cf-dark-color: #212121;
       --cf-light-color: #deeefb;
       --cf-font-size: 1.1rem;
   }
   ```

   Below that, a `#region` holds Blazor's default styles: the focus outline, form validation colors, the error banner, and the loading spinner. The spinner reads `--blazor-load-percentage`, which the runtime updates as it downloads.
3. **Scoped CSS** in files like `MainLayout.razor.css` and `NavMenu.razor.css`. These styles apply only to their matching component. At build time Blazor rewrites them with unique attributes and bundles them into `ReelTime.styles.css`. To style a single component without affecting anything else, add a `MyComponent.razor.css` file next to it.

### 6. `_Imports.razor`

This file's `@using` directives apply to every `.razor` file in its folder and subfolders. That's why pages can use `HttpClient`, `NavLink`, `GetFromJsonAsync`, and so on without their own `using` lines. Add your own namespaces here, such as a `Services` or `Components` folder.

---

## Configuration reference

| File | What to change |
|---|---|
| `ReelTime.csproj` | Target framework, NuGet packages |
| `Properties/launchSettings.json` | Local ports (`5262` / `7170`) and the environment name |
| `wwwroot/index.html` | Page title, fonts, CDN links, favicon, `<base href>` (change it when hosting under a sub-path, e.g. `/my-app/`) |
| `wwwroot/css/app.css` | Theme variables and global styles |
| `App.razor` | Default layout and not-found page |
| `Program.cs` | Dependency injection (services, `HttpClient`s) |

---

## Useful links

- [Blazor documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [Blazor WebAssembly hosting and deployment](https://learn.microsoft.com/aspnet/core/blazor/host-and-deploy/webassembly)
- [Bootstrap 5](https://getbootstrap.com/docs/5.3/) · [Bootstrap Icons](https://icons.getbootstrap.com/) · [Devicon](https://devicon.dev/)
