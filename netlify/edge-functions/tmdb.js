// netlify/edge-functions/tmdb.js
// Proxies /tmdb/* from the Blazor app to the TMDB API so the key stays server-side.
export default async (request) => {
    const apiKey = Netlify.env.get("API_KEY");
    const apiUrl = (Netlify.env.get("API_URL") ?? "https://api.themoviedb.org/3/").replace(/\/?$/, "/");

    if (!apiKey) {
        return new Response("API_KEY is not set in Netlify environment variables", { status: 500 });
    }

    // e.g. https://mysite.netlify.app/tmdb/movie/now_playing?region=US
    //  ->  https://api.themoviedb.org/3/movie/now_playing?region=US
    const url = new URL(request.url);
    const tmdbPath = url.pathname.replace(/^\/tmdb\//i, "");
    const target = `${apiUrl}${tmdbPath}${url.search}`;

    const res = await fetch(target, {
        method: "GET",
        headers: {
            Authorization: `Bearer ${apiKey}`,
            Accept: "application/json",
        },
    });

    return new Response(res.body, {
        status: res.status,
        headers: { "content-type": res.headers.get("content-type") ?? "application/json" },
    });
};

export const config = { path: "/tmdb/*" };