var instructions_settings = document.getElementById("instructions_settings");
var premium_settings = document.getElementById("premium_settings");

var settingsRightPanelElement = document.getElementById("settings_rightpanel");

const path = require("path");
const os = require("os");
const Store = require('electron-store');

const store = new Store({"cwd": path.join(os.homedir(), "Index")});

// Add this to test loading delays
// var response = http("GET", "http://localhost:4153/actions/getpremiumkey");

// Handle first opening of the app
if (store.get("isFirst", true)) {
    OpenSettings();
    OpenInstructions();

    store.set("isFirst", false);
}else
{
    var inputElement = document.getElementById("input");
    if(inputElement.textContent == ""){
        inputElement.value = "*";
        refreshList();
    }
}

async function OpenInstructions(event) {
    var instructionshtml = document.createElement('div');
    instructionshtml.innerHTML = await fetchHtmlAsText('pages/usage_instructions.html');

    premium_settings.classList.remove("selected");
    instructions_settings.classList.add("selected");

    RemoveAllChildren(settingsRightPanelElement);
    settingsRightPanelElement.appendChild(instructionshtml);
}
instructions_settings.onclick = OpenInstructions;

premium_settings.onclick = function (event) {
    
    var premiumKey = http("GET", "http://localhost:4153/actions/getpremiumkey");
    
    var stats = JSON.parse(http("GET", "http://localhost:4153/actions/getstats"));
    
    var premiumsettingshtml = document.createElement('div');
    
    premium_settings.classList.add("selected");
    instructions_settings.classList.remove("selected");

    premiumsettingshtml.innerHTML = `
        <div>
            <div id="currentKeyGuid">
                <p>Current premium key:</p>
                <p>${premiumKey}</p>
            </div>
            <button id="purchaseKey">Purchase a new key</button>
        </div>
        <p></p>
        <div>
            <div id="stats">
                Number of documents:
                <p>${stats.docCount}</p>
            </div>
        </div>
    `;

    RemoveAllChildren(settingsRightPanelElement);
    settingsRightPanelElement.appendChild(premiumsettingshtml);

    document.getElementById('purchaseKey').addEventListener('click', function(e) {
      });
}