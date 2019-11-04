const Pickr = require("@simonwep/pickr");

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
        <div class="color-picker"></div>
        <button class="button context_menu_submit" onClick="SubmitContextMenu(this)">Create</button>
    `
    document.body.appendChild(div);

    const pickr = Pickr.create({
        el: '.color-picker',
        theme: 'nano', // or 'monolith', or 'nano'
        container: '.context_menu',
        default: 'aliceblue',
        swatches: [
            'rgba(244, 67, 54, 1)',
            'rgba(233, 30, 99, 1)',
            'rgba(156, 39, 176, 1)',
            'rgba(103, 58, 183, 1)',
            'rgba(63, 81, 181, 1)',
            'rgba(33, 150, 243, 1)',
            'rgba(3, 169, 244, 1)',
            'rgba(0, 188, 212, 1)',
            'rgba(0, 150, 136, 1)',
            'rgba(76, 175, 80, 1)',
            'rgba(139, 195, 74, 1)',
            'rgba(205, 220, 57, 1)',
            'rgba(255, 235, 59, 1)',
            'rgba(255, 193, 7, 1)'
        ],
    
    });

    pickr.on('change', (color, instance) => {
        var menu = document.getElementById('contextMenu');
        menu.style['backgroundColor'] = color.toHEXA(); 

        var nodes = ds.getSelection();
        nodes.forEach(node => {
            node.style['backgroundColor'] = color.toHEXA(); 
        });
    });

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
}

function RemoveContextMenu(params) {
    var oldMenu = document.getElementById("contextMenu");
    if (oldMenu && oldMenu.parentNode) {
        oldMenu.parentNode.removeChild(oldMenu);
    }
}