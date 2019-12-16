var stripeHandler = StripeCheckout.configure({
    key: 'pk_live_KowPRzdx02IRFfAfLvPygz0Q00a5LAJ0mz',
    image: 'https://stripe.com/img/documentation/checkout/marketplace.png',
    locale: 'auto',
    token: function(token) {
      // Send the token in an AJAX request
      var xhr = new XMLHttpRequest();
      xhr.open('POST', 'https://wanshitong.azurewebsites.net/api/wanshitongbackend?api=newkey');
      xhr.setRequestHeader('Content-Type', "application/json;charset=UTF-8");
      xhr.onload = function() {
        if (xhr.status === 200) {
            //var resp = JSON.parse(xhr.responseText);
            console.log(xhr.responseText);
            
            var response = http("GET", "http://localhost:4153/actions/setpremiumkey/" + xhr.responseText);
            alert("Premium key " + xhr.responseText + " is set.");
        }
        else if (xhr.status !== 200) {
            alert('Request failed. Returned status of ' + xhr.status);
        }
      };
      xhr.send(JSON.stringify({"token": token.id}));
    }
  });
  
  // Close Checkout on page navigation:
  window.addEventListener('popstate', function() {
    stripeHandler.close();
  });