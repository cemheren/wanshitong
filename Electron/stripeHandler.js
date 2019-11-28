var handler = StripeCheckout.configure({
    key: 'pk_test_33HGY2nhmuWFI4uPzZkcCnzX00lMXpeSYU',
    image: 'https://stripe.com/img/documentation/checkout/marketplace.png',
    locale: 'auto',
    token: function(token) {
      // Send the token in an AJAX request
      var xhr = new XMLHttpRequest();
      xhr.open('POST', 'http://localhost:7071/api/wanshitongbackend?api=newkey');
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
  
  document.getElementById('purchaseKey').addEventListener('click', function(e) {
    // Open Checkout with further options:
    handler.open({
      name: 'Stripe.com',
      description: 'Librarian One Time Purchase',
      amount: 4000
    });
    e.preventDefault();
  });
  
  // Close Checkout on page navigation:
  window.addEventListener('popstate', function() {
    handler.close();
  });