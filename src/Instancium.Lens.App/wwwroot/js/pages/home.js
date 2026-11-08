import { ensureStylesheet } from "../core/head-assets.js";

// Side-effect module: the indexing page needs no JS behaviour, only its stylesheet.
ensureStylesheet("css/pages/home.css");
