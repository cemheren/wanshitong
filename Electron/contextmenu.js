function CreateContextMenu(e) {
    
    RemoveContextMenu();

    var rect = e.getBoundingClientRect();

    const div = document.createElement('div');
    div.className = 'context_menu';
    div.id = 'contextMenu';
    div.style['left'] = rect.right + "px";
    div.style['top'] = rect.top + "px";

    div.innerHTML += `
        <i class="material-icons context_menu_close" onClick="RemoveContextMenu(this)">close</i>
        <div class="context_menu_label">Add a tag:</div>
        <input id="context_menu_input" type="text" class="context_menu_input"/>
        <button class="button context_menu_submit" onClick="SubmitContextMenu(this)">Create</button>
    `

    document.body.appendChild(div);
    return div;
}

function SubmitContextMenu(element) {

    var nodes = ds.getSelection();
    var indexAndDocId = new Object();

    var i = 0;
    nodes.forEach(node => {
        indexAndDocId[i] = node.querySelector('#myId').textContent
        i++;
    });

    var tagDocModel = {
        IndexAndDocId: indexAndDocId,
        Tag: element.parentElement.querySelector("#context_menu_input").value
    }

    var response = http("POST", "http://localhost:4153/tag", JSON.stringify(tagDocModel));

    console.log(response);

    //refreshList();
}

function RemoveContextMenu(params) {
    var oldMenu = document.getElementById("contextMenu");
    if (oldMenu && oldMenu.parentNode) {
        oldMenu.parentNode.removeChild(oldMenu);
    }
}