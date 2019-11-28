// don't add dependencies from any other file here. This file is loaded first.

async function fetchHtmlAsText(url) {
    return await (await fetch(url)).text();
}