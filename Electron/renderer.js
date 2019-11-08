// This file is required by the index.html file and will
// be executed in the renderer process for that window.
// All of the Node.js APIs are available in this process.
var inputElement = document.getElementById("input");
var resultsElement = document.getElementById("results");
var rightPanelElement = document.getElementById("righttextpanel");
var relatedDocumentsElement = document.getElementById("relateddocuments");
var similarityRowElement = document.getElementById("similarityrow");
var deleteElement = document.getElementById("delete");
var toggleOffElement = document.getElementById("toggle-off");
var toggleOnElement = document.getElementById("toggle-on");

const moment = require("moment");
const DragSelect = require("dragselect");

var ds = new DragSelect({
    selectables: document.getElementsByClassName('similarity_row_item'),
    area: relatedDocumentsElement,
    callback: onElementSelect,
    multiSelectMode: false
});

var selectedElementMetadata = {};

function CreateResultRow(docId, group, text, ingestionTime, category, highlightedText, tags)
{
    const div = document.createElement('div');
    div.className = 'result_row';
    div.onclick = onRowTextClick;

    if(text.length > 100)
    {
        //text = text.substring(0, 100) + "...";
    }

    if (tags) {
        tags.forEach(tag => {
            div.innerHTML += `<button class="result_row_tag" onClick="StartTagSearch(this)">${tag}</button>`;
        });
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
        <div class="result_row_group">${moment(ingestionTime).fromNow()}</div>
        <div id="result_row_group" class="hidden">${ingestionTime}</div>
        <div class="result_row_id">${docId}</div>
    `

    return div;
}

function CreateRelatedRowElement(docId, group, text, ingestionTime, category, myId) {
    const li = document.createElement('li');
    li.className = 'similarity_row_item_parent';

    // const select = document.createElement('input');
    // select.id = 'select';
    // select.type = 'checkbox';
    // select.className = 'similarity_select';

    // li.appendChild(select);

    var div = document.createElement('div');
    div.className = 'similarity_row_item';

    if (selectedElementMetadata.docId == docId) {
        div.innerHTML += `
        <div class="similarity_row_time">Selected document</div>
    `
    }else{
        div.innerHTML += `
            <div class="similarity_row_time">${moment(ingestionTime).from(selectedElementMetadata.ingestionTime)}</div>
        `
    }

    if(category == -10)
    {
        div.innerHTML += `<img class="result_row_img" src="${group}">`;
    }

    var textClass = "similarity_row_text";
    div.innerHTML += `
        <div id="textcontent" class="${textClass}">${text}</div>
        <div id="docId" class="result_row_id">${docId}</div>
        <div id="myId" class="result_row_id">${myId}</div>
        <div id="result_row_group" class="hidden">${ingestionTime}</div>
    `;

    li.appendChild(div);

    div = document.createElement('div');
    div.className = "similarity_row_item_filler";
    li.appendChild(div);

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
    selectedElementMetadata.ingestionTime = event.currentTarget.querySelector('#result_row_group').textContent;

    RemoveAllChildren(rightPanelElement);
    RemoveContextMenu();

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

    RemoveAllChildren(similarityRowElement);
    ds.clearSelection();

    var json = JSON.parse(response);
    json.forEach(e => {
        var newElement = CreateRelatedRowElement(e.docId, e.group, e.text, e.ingestionTime, e.processId, e.myId);
        ds.addSelectables(newElement);
        similarityRowElement.appendChild(newElement);
    });
}

function onElementSelect(element) {
    CreateContextMenu(element);
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
    var response = http("GET", "http://localhost:4153/query/" + encodeURIComponent(text));

    if(response == "" || response == undefined){
        return "No result found";
    }

    var json = JSON.parse(response);
    json.forEach(e => {
        resultsElement.appendChild(
            CreateResultRow(
                e.docId, 
                e.group, 
                e.text, 
                e.ingestionTime, 
                e.processId, 
                e.highlightedText,
                e.tags));
    });
}
inputElement.onkeyup = refreshList;

deleteElement.onclick = function(event){
    var response = http("DELETE", "http://localhost:4153/delete/" + selectedElementMetadata.docId);

    refreshList();

    rightPanelElement.textContent = "Deleted.";
}

function StartTagSearch(element) {
    inputElement.value = "tags:" + element.textContent.split(' ').join('?');
    refreshList();
}

toggleOffElement.onclick = swapTextImage;
toggleOnElement.onclick = swapTextImage;

function http(method, theUrl, post) {
    var xmlHttp = new XMLHttpRequest();
    xmlHttp.open(method, theUrl, false); // false for synchronous request
    xmlHttp.setRequestHeader('Content-type', "application/json");
    xmlHttp.setRequestHeader('accept', "application/json")
    xmlHttp.send(post);

    return xmlHttp.responseText;
}