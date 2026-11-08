// Head asset loader shared by page modules.
// WHY: each page module owns its stylesheet; injecting it from the module keeps the
// global CSS payload limited to the design system and avoids cross-module style noise.
const loaded = new Set();

export function ensureStylesheet(href) {
    const url = new URL(href, document.baseURI).toString();
    if (loaded.has(url)) {
        return;
    }
    loaded.add(url);
    const link = document.createElement("link");
    link.rel = "stylesheet";
    link.href = url;
    document.head.appendChild(link);
}
