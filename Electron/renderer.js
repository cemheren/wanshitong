// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.

var inputElement = document.getElementById("input");
var resultsElement = document.getElementById("results");
var rightPanelElement = document.getElementById("rightpanel");

function CreateResultRow(group, text)
{
    const div = document.createElement('div');
    div.className = 'result_row';

    if(text.length > 100)
    {
        //text = text.substring(0, 100) + "...";
    }

    div.innerHTML = `
        <div class="result_row_text" onClick="onRowTextClick(this)">${text}</div>
        <div class="result_row_group">${group}</div>
    `;
    return div;
}

function onRowTextClick(event)
{
    rightPanelElement.textContent = event.textContent;
}

function RemoveAllChildren(node)
{
    while (node.firstChild) {
        node.removeChild(node.firstChild);
    }
}

inputElement.onkeyup = function(event){
    var text = document.getElementById("input").value;

    RemoveAllChildren(resultsElement);
    var response = httpGet("http://localhost:5000/query/" + text);

    if(response == "" || response == undefined){
        return "No result found";
    }

    var text = "";
    var json = JSON.parse(response);
    json.forEach(e => {
        resultsElement.appendChild(CreateResultRow(e.group, e.text));
        text += e.group + ": " + e.text + "\n";
    });
}

function httpGet(theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open("GET", theUrl, false); // false for synchronous request
    xmlHttp.send(null);

    return xmlHttp.responseText;
}