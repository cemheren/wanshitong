var storiesListElement = document.getElementById("stories_list_element");
var storiesGroupingTagElement = document.getElementById("stories_grouping_tag");
var storiesRightPanelElement = document.getElementById("stories_right_panel");

var currentGroupingTag = null;
var currentStory = null;

function StoriesOnLoad() {
    
    refreshStories();
    var currentStory = null;
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
    div.onclick = enlargeStory;

    if(text.length > 100)
    {
        //text = text.substring(0, 100) + "...";
    }

    if (tags) {
        tags.forEach(tag => {
            div.innerHTML += `<button class="story_item_tag" onClick="setTagAsGrouping(this)">${tag}</button>`;
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
            <div id="story_item_textcontent" class="story_item_full_text">${text}</div>
        `;
    }else{
        div.innerHTML += `
            <div id="story_item_textcontent" class="${textClass}">${text}</div>
            <div id="bs" class="story_item_full_text">${text}</div>
        `;
    }
    div.innerHTML += `
        <div class="story_item_group">${moment.utc(ingestionTime).local().fromNow()}</div>
        <div id="story_item_group" class="hidden">${ingestionTime}</div>
        <div class="story_item_id">${docId}</div>
    `

    return div;
}

function CreateStoryGroup(groupName)
{
    const div = document.createElement('div');
    div.className = 'story_group';
    div.innerHTML = `<div id="story_group_id" class="story_group_id">${groupName}</div>`;
    return div;
}

function executeStoryQuery(event) {
    UnselectAllChildren(storiesListElement);
    SelectElement(event.currentTarget);
    currentStory = event.currentTarget;
    redrawStory(currentStory);
}

function redrawStory(currentStory)
{
    var text = currentStory.querySelector('.phrase').textContent;
    var response = http("POST", "http://localhost:4153/query/", JSON.stringify({"SearchPhrase" : text, "GroupingPhrase" : currentGroupingTag}));

    if(response == "" || response == undefined){
        return "No result found";
    }

    RemoveAllChildren(storiesRightPanelElement);

    var allGroups = {};

    var json = JSON.parse(response);
    json.forEach(e => {

        var assignedGroup = e.groupingNumber == null ? "none" : e.groupingNumber;

        var storyGroup = null;
        if (allGroups[assignedGroup] == null) {
            storyGroup = CreateStoryGroup(assignedGroup);
            allGroups[assignedGroup] = storyGroup;
        }else{
            storyGroup = allGroups[assignedGroup]
        }

        storyGroup.appendChild(
            CreateStoryItem(
                e.docId, 
                e.group, 
                e.text, 
                e.ingestionTime, 
                e.processId, 
                e.highlightedText,
                e.tags));
    });

    for (let i = 0; i < Object.keys(allGroups).length + 1; i++) {

        if (allGroups[i] == null) {
            continue;
        }

        storiesRightPanelElement.appendChild(allGroups[i]);
    }
}

function setTagAsGrouping(element)
{
    currentGroupingTag = element.innerText;
    storiesGroupingTagElement.innerHTML = `<div class="text">Grouping by: ${currentGroupingTag}</div>`;
    redrawStory(currentStory);
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

var enlargeStory = function (event) {
    var selectedStory = event.currentTarget;

    if(selectedStory.classList.contains("enlarged"))
    {
        RemoveClassFromChildren(selectedStory, "enlarged");
    }else
    {
        AddClassToChildren(selectedStory, "enlarged");
    }
}