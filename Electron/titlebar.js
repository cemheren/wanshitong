const ElectronTitlebarWindows = require('electron-titlebar-windows');
const titlebar = new ElectronTitlebarWindows({darkMode: "true", draggable: true, fullscreen: false});
titlebar.appendTo(document.getElementById("title"));

titlebar.on('close', function(e) {
    ipcRenderer.send('app_close');
});

titlebar.on('minimize', function(e) {
    ipcRenderer.send('app_minimize');
});

titlebar.on('maximize', function(e) {
    ipcRenderer.send('app_maximize');
});

titlebar.on('fullscreen', function(e) {
    ipcRenderer.send('app_fullscreen');
});