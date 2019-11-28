var instructions_settings = document.getElementById("instructions_settings");
var premium_settings = document.getElementById("premium_settings");

var rightPanelElement = document.getElementById("settings_rightpanel");

instructions_settings.onclick = async function (event) {
    var instructionshtml = document.createElement('div');
    instructionshtml.innerHTML = await fetchHtmlAsText('pages/usage_instructions.html');

    RemoveAllChildren(rightPanelElement);
    rightPanelElement.appendChild(instructionshtml);
}

premium_settings.onclick = function (event) {
    var premiumsettingshtml = document.createElement('div');
    premiumsettingshtml.innerHTML = `
        <div>
            <span id="currentKeyGuid"></span>
            <button id="purchaseKey">Purchase a new key</button>
        </div>
    `;

    RemoveAllChildren(rightPanelElement);
    rightPanelElement.appendChild(premiumsettingshtml);

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