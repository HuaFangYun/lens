import { ensureStylesheet } from "../core/head-assets.js";

ensureStylesheet("css/pages/chat.css");

// Keeps the transcript pinned to the newest token while the answer streams.
export function scrollToEnd(element) {
    element.scrollTop = element.scrollHeight;
}
