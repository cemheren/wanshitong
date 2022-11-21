// don't add dependencies from any other file here. This file is loaded first.

const indexerUrl = "http://localhost:4153";

let typingTimer;              
let doneTypingInterval = 3500;

const delay = ms => new Promise(res => setTimeout(res, ms));

async function fetchHtmlAsText(url) {
    return await (await fetch(url)).text();
}

function RemoveAllChildren(node)
{
    while (node.firstChild) {
        node.removeChild(node.firstChild);
    }
}

function SelectElement(node) {
    node.classList.add("selected");
}

function UnselectAllChildren(node)
{
    for (const ch of node.children) {
        ch.classList.remove("selected");
    }
}

function AddClassToChildren(node, classname)
{
    node.classList.add(classname);
    for (const ch of node.children) {
        ch.classList.add(classname);
        AddClassToChildren(ch, classname);
    }
}

function RemoveClassFromChildren(node, classname)
{
    node.classList.remove(classname);
    for (const ch of node.children) {
        ch.classList.remove(classname);
        RemoveClassFromChildren(ch, classname);
    }
}

function http(method, theUrl, post) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, false); // false for synchronous request
    xmlHttp.setRequestHeader('Content-type', "application/json");
    xmlHttp.setRequestHeader('accept', "application/json")
    xmlHttp.send(post);

    return xmlHttp.responseText;
}