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

settingsToggle.onclick = function (event) {
    settingsView.classList.remove("hidden");
    storiesView.className = "hidden";
    searchView.className = "hidden";
}