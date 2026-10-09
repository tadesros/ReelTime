export default {
    // Netlify config for routing
    path: "/TMDB/*",  // This means any request to /TMDB/ will be handled here

    async handler(req) {
        const API_KEY = Deno.env.get("API_KEY");    // from Netlify environment vars
        const API_URL = Deno.env.get("API_URL");    // e.g., https://api.themoviedb.org/3/

        // Ensure trailing slash if missing
        let baseUrl = API_URL.endsWith("/")
            ? API_URL
            : API_URL + "/";

        // Example: the user calls /TMDB/movie/popular
        // Remove '/TMDB' portion so we can forward to actual TMDB endpoint
        let newUrl = req.url.replace("/TMDB/", "");

        // Rebuild the full URL, e.g. https://api.themoviedb.org/3/movie/popular
        let targetUrl = `${baseUrl}${newUrl}`;

        // Add your Bearer or Query Parameter auth if needed
        // This example might look for the "?" to attach &api_key= or Bearer tokens.

        // Proxy the request with fetch
        const response = await fetch(targetUrl, {
            headers: {
                Authorization: `Bearer ${API_KEY}`
            },
            method: req.method
        });

        return new Response(response.body, {
            status: response.status,
            headers: response.headers
        });
    }
}