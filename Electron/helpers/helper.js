// don't add dependencies from any other file here. This file is loaded first.

async function fetchHtmlAsText(url) {
    return await (await fetch(url)).text();
}

function http(method, theUrl, post) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, false); // false for synchronous request
    xmlHttp.setRequestHeader('Content-type', "application/json");
    xmlHttp.setRequestHeader('accept', "application/json")
    xmlHttp.send(post);

    return xmlHttp.responseText;
}