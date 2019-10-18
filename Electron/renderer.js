// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.

var inputElement = document.getElementById("input");
var resultsElement = document.getElementById("results");
var rightPanelElement = document.getElementById("righttextpanel");
var deleteElement = document.getElementById("delete");

// var Quill = require("quill");
// var editor = new Quill('#editor', {
//     modules: { toolbar: '#toolbar' },
//     theme: 'snow'
//   });
var selectedElementMetadata = {};

function CreateResultRow(docId, group, text, ingestionTime, category)
{
    const div = document.createElement('div');
    div.className = 'result_row';

    if(text.length > 100)
    {
        //text = text.substring(0, 100) + "...";
    }

    var textClass = "result_row_text";
    if(category == -10)
    {
        div.innerHTML += `<img class="result_row_img" src="${group}">`;
        textClass = "result_row_short_text";
    }

    div.innerHTML += `
        <div class="${textClass}" onClick="onRowTextClick(this)">${text}</div>
    `;

    div.innerHTML += `
        <div class="result_row_group">${ingestionTime}</div>
        <div class="result_row_id">${docId}</div>
    `

    return div;
}

function onRowTextClick(event)
{
    // editor.setText(event.textContent);

    selectedElementMetadata.docId = event.parentElement.querySelector('.result_row_id').textContent;
    selectedElementMetadata.text = event.textContent;
    rightPanelElement.textContent = event.textContent;

    //create image
    var img = document.createElement('img');
    img.src = event.parentElement.querySelector('.result_row_img').src;
    img.className = 'right_panel';
    rightPanelElement.appendChild(img);
}

function RemoveAllChildren(node)
{
    while (node.firstChild) {
        node.removeChild(node.firstChild);
    }
}

var refreshList = function(event){
    var text = document.getElementById("input").value;

    RemoveAllChildren(resultsElement);
    var response = http("GET", "http://localhost:5000/query/" + text);

    if(response == "" || response == undefined){
        return "No result found";
    }

    var json = JSON.parse(response);
    json.forEach(e => {
        resultsElement.appendChild(CreateResultRow(e.docId, e.group, e.text, e.ingestionTime, e.processId));
    });
}
inputElement.onkeyup = refreshList;

deleteElement.onclick = function(event){
    var response = http("DELETE", "http://localhost:5000/delete/" + selectedElementMetadata.docId);
    
    refreshList();

    rightPanelElement.textContent = "Deleted.";
}

function http(method, theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, false); // false for synchronous request
    xmlHttp.send(null);

    return xmlHttp.responseText;
}