var searchToggle = document.getElementById("search");
var storiesToggle = document.getElementById("stories");
var settingsToggle = document.getElementById("settings");

var searchView = document.getElementById("searchView");
var storiesView = document.getElementById("storiesView");
var settingsView = document.getElementById("settingsView");

searchToggle.onclick = function (event) {
    searchView.classList.remove("hidden");
    storiesView.className = "hidden";
    settingsView.className = "hidden";
}

function OpenSettings(event) {
    settingsView.classList.remove("hidden");
    storiesView.className = "hidden";
    searchView.className = "hidden";
}
settingsToggle.onclick = OpenSettings;

storiesToggle.onclick = function (event) {
    storiesView.classList.remove("hidden");
    settingsView.className = "hidden";
    searchView.className = "hidden";

    StoriesOnLoad();
}