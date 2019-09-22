// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.

var inputElement = document.getElementById("input");
var resultsElement = document.getElementById("results");

inputElement.onkeyup = function(event){
    var text = document.getElementById("input").value;

    resultsElement.value = httpGet("http://localhost:5000/query/" + text);
}

function httpGet(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", theUrl, false); // false for synchronous request
    xmlHttp.send(null);
    if(xmlHttp.responseText == "" || xmlHttp.responseText == undefined){
        return "No result found";
    }

    var text = "";
    var json = JSON.parse(xmlHttp.responseText);
    json.forEach(e => {
        text += e.group + ": " + e.text + "\n";
    });

    return text;
}