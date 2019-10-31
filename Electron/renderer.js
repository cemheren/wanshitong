// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.
var inputElement = document.getElementById("input");
var resultsElement = document.getElementById("results");
var rightPanelElement = document.getElementById("righttextpanel");
var relatedDocumentsElement = document.getElementById("similarityrow");
var deleteElement = document.getElementById("delete");
var toggleOffElement = document.getElementById("toggle-off");
var toggleOnElement = document.getElementById("toggle-on");

// var Quill = require("quill");
// var editor = new Quill('#editor', {
//     modules: { toolbar: '#toolbar' },
//     theme: 'snow'
//   });
var selectedElementMetadata = {};

function CreateResultRow(docId, group, text, ingestionTime, category, highlightedText)
{
    const div = document.createElement('div');
    div.className = 'result_row';
    div.onclick = onRowTextClick;

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

    if (highlightedText) {
        div.innerHTML += `
            <div class="${textClass}">${highlightedText}</div>
            <div id="textcontent" class="hidden">${text}</div>
        `;
    }else{
        div.innerHTML += `
            <div id="textcontent" class="${textClass}">${text}</div>
        `;
    }

    div.innerHTML += `
        <div class="result_row_group">${ingestionTime}</div>
        <div class="result_row_id">${docId}</div>
    `

    return div;
}

function CreateRelatedRowElement(docId, group, text, ingestionTime, category) {
    const li = document.createElement('li');
    li.className = 'similarity_row_item';
    
    li.onclick = onRowTextClick;

    li.innerHTML += `
        <div class="similarity_row_time">${ingestionTime}</div>
    `

    if(category == -10)
    {
        li.innerHTML += `<img class="result_row_img" src="${group}">`;
    }

    var textClass = "similarity_row_text";
    li.innerHTML += `
        <div id="textcontent" class="${textClass}">${text}</div>
        <div class="result_row_id">${docId}</div>
        <div class="result_row_group">${ingestionTime}</div>
    `;

    return li;
}

function onImageDoubleClick(event) {
    window.open(event.target.currentSrc);
}

function swapTextImage(event)
{
    var rightPanelText = rightPanelElement.querySelector("#right_panel_text");
    var rightPanelImage = rightPanelElement.querySelector("#right_panel_image");

    if (rightPanelImage == null) {
        return;
    }

    if (rightPanelText.className == "hidden") {
        rightPanelText.className = "max_width";
        rightPanelImage.className = "hidden";
        toggleOffElement.classList.add("hidden");
        toggleOnElement.classList.remove("hidden");
    }else{
        rightPanelText.className = "hidden";
        rightPanelImage.className = "right_panel_image";  
        toggleOnElement.classList.add("hidden");
        toggleOffElement.classList.remove("hidden");
    }
}

function onRowTextClick(event)
{
    selectedElementMetadata.docId = event.currentTarget.querySelector('.result_row_id').textContent;
    selectedElementMetadata.text = event.currentTarget.querySelector('#textcontent').textContent;
    selectedElementMetadata.ingestionTime = event.currentTarget.querySelector('.result_row_group').textContent;
    
    RemoveAllChildren(rightPanelElement);

    var textDiv = document.createElement('div');
    textDiv.textContent = selectedElementMetadata.text;
    textDiv.id = "right_panel_text";
    textDiv.className = "max_width";
    rightPanelElement.appendChild(textDiv);

    //create image
    var img = document.createElement('img');
    var imgElement = event.currentTarget.querySelector('.result_row_img');
    if (imgElement !== null) {
        img.src = imgElement.src;
        img.ondblclick = onImageDoubleClick;
        img.id = 'right_panel_image';
        img.className = 'right_panel_image';
        rightPanelElement.appendChild(img);

        toggleOnElement.classList.add("hidden");
        toggleOffElement.classList.remove("hidden");
        textDiv.className = "hidden";
    }

    var documentDateStart = new Date(Date.parse(selectedElementMetadata.ingestionTime));
    var documentDateEnd = new Date(Date.parse(selectedElementMetadata.ingestionTime));
    documentDateStart.setHours(documentDateStart.getHours() - 1);
    documentDateEnd.setHours(documentDateEnd.getHours() + 1);

    var response = http("GET", "http://localhost:4153/timerange/" + 
        documentDateStart.toISOString() + "/" + documentDateEnd.toISOString());

    if(response == "" || response == undefined){
        return "No result found";
    }

    RemoveAllChildren(relatedDocumentsElement);

    var json = JSON.parse(response);
    json.forEach(e => {
        relatedDocumentsElement.appendChild(CreateRelatedRowElement(e.docId, e.group, e.text, e.ingestionTime, e.processId));
    });
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
    var response = http("GET", "http://localhost:4153/query/" + text);

    if(response == "" || response == undefined){
        return "No result found";
    }

    var json = JSON.parse(response);
    json.forEach(e => {
        resultsElement.appendChild(CreateResultRow(e.docId, e.group, e.text, e.ingestionTime, e.processId, e.highlightedText));
    });
}
inputElement.onkeyup = refreshList;

deleteElement.onclick = function(event){
    var response = http("DELETE", "http://localhost:4153/delete/" + selectedElementMetadata.docId);
    
    refreshList();

    rightPanelElement.textContent = "Deleted.";
}

toggleOffElement.onclick = swapTextImage;
toggleOnElement.onclick = swapTextImage;

function http(method, theUrl) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, false); // false for synchronous request
    xmlHttp.send(null);

    return xmlHttp.responseText;
}