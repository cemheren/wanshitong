var instructions_settings = document.getElementById("instructions_settings");
var premium_settings = document.getElementById("premium_settings");

var settingsRightPanelElement = document.getElementById("settings_rightpanel");

instructions_settings.onclick = async function (event) {
    var instructionshtml = document.createElement('div');
    instructionshtml.innerHTML = await fetchHtmlAsText('pages/usage_instructions.html');

    RemoveAllChildren(settingsRightPanelElement);
    settingsRightPanelElement.appendChild(instructionshtml);
}

premium_settings.onclick = function (event) {
    
    var response = http("GET", "http://localhost:4153/actions/getpremiumkey");
    
    var premiumsettingshtml = document.createElement('div');
    premiumsettingshtml.innerHTML = `
        <div>
            <div id="currentKeyGuid">
                <p>Current premium key:</p>
                <p>${response}</p>
            </div>
            <button id="purchaseKey">Purchase a new key</button>
        </div>
    `;

    RemoveAllChildren(settingsRightPanelElement);
    settingsRightPanelElement.appendChild(premiumsettingshtml);

    document.getElementById('purchaseKey').addEventListener('click', function(e) {
        // Open Checkout with further options:
        stripeHandler.open({
          name: 'Stripe.com',
          description: 'Librarian One Time Purchase',
          amount: 4000
        });
        e.preventDefault();
      });
}