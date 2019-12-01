var storiesListElement = document.getElementById("stories_list_element");
var storiesRightPanelElement = document.getElementById("stories_right_panel");

function StoriesOnLoad() {
    
    refreshStories();
}

function CreateStoryRow(phrase)
{
    const div = document.createElement('div');
    div.className = 'stories_left_panel_clickable_row';
    //div.onclick = onRowTextClick;

    div.innerHTML = `<span class="phrase">${phrase}</span>`;
    div.onclick = executeStoryQuery;

    return div;
}

function CreateStoryItem(docId, group, text, ingestionTime, category, highlightedText, tags) {
    const div = document.createElement('div');
    div.className = 'story_item';
    div.onclick = onRowTextClick;

    if(text.length > 100)
    {
        //text = text.substring(0, 100) + "...";
    }

    if (tags) {
        tags.forEach(tag => {
            div.innerHTML += `<button class="story_item_tag" onClick="StartTagSearch(this)">${tag}</button>`;
        });
    }

    var textClass = "story_item_text";
    if(category == -10)
    {
        div.innerHTML += `
            <div class="crop">
                <img class="story_item_img" src="${group}">
            </div>
            `;
        
        textClass = "story_item_short_text";
    }

    if (highlightedText) {
        div.innerHTML += `
            <div class="${textClass}">${highlightedText}</div>
            <div id="story_item_textcontent" class="hidden">${text}</div>
        `;
    }else{
        div.innerHTML += `
            <div id="story_item_textcontent" class="${textClass}">${text}</div>
        `;
    }
    div.innerHTML += `
        <div class="story_item_group">${moment.utc(ingestionTime).local().fromNow()}</div>
        <div id="story_item_group" class="hidden">${ingestionTime}</div>
        <div class="story_item_id">${docId}</div>
    `

    return div;
}

function executeStoryQuery(event) {
    var text = event.currentTarget.querySelector('.phrase').textContent;
    var response = http("GET", "http://localhost:4153/query/" + encodeURIComponent(text));

    if(response == "" || response == undefined){
        return "No result found";
    }

    RemoveAllChildren(storiesRightPanelElement);

    var json = JSON.parse(response);
    json.forEach(e => {
        storiesRightPanelElement.appendChild(
            CreateStoryItem(
                e.docId, 
                e.group, 
                e.text, 
                e.ingestionTime, 
                e.processId, 
                e.highlightedText,
                e.tags));
    });
}


var refreshStories = function(event){
    
    var response = http("GET", indexerUrl + "/savedsearch/get");

    if(response == "" || response == undefined){
        return "No result found";
    }

    RemoveAllChildren(storiesListElement);

    var json = JSON.parse(response);
    json.forEach(e => {
        storiesListElement.appendChild(
            CreateStoryRow(
               e.searchPhrase));
    });
}