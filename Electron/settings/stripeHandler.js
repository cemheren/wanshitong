var stripeHandler = StripeCheckout.configure({
    key: 'pk_test_33HGY2nhmuWFI4uPzZkcCnzX00lMXpeSYU',
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
            alert(xhr.responseText);
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