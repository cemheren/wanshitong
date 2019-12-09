
const { ipcRenderer } = require('electron');
const version = document.getElementById('version');

ipcRenderer.send('app_version');
ipcRenderer.on('app_version', (event, arg) => {
    ipcRenderer.removeAllListeners('app_version');
    version.innerText = 'Version ' + arg.version;
});

const notification = document.getElementById('notification');
const message = document.getElementById('message');
const restartButton = document.getElementById('restart-button');
    ipcRenderer.on('update_available', () => {
    ipcRenderer.removeAllListeners('update_available');
    message.innerText = 'A new update is available 🎁. Downloading now...';
    notification.classList.remove('hidden');
});

ipcRenderer.on('update_downloaded', () => {
    ipcRenderer.removeAllListeners('update_downloaded');
    message.innerText = 'Update downloaded. It will be installed on restart. Restart now?';
    restartButton.classList.remove('hidden');
    notification.classList.remove('hidden');
});

ipcRenderer.on('screenshot', () => {
    message.innerText = "New document is ready. Try searching for a keyword or type * (star) to see all docs.";
    notification.classList.remove('hidden');
});

ipcRenderer.on('screenshot_error', () => {
    message.innerText = 'Screenshot was not successful 😭. Please contact us at (760) 364-6209‬.';
    notification.classList.remove('hidden');
});

ipcRenderer.on('clipboard', () => {
    message.innerText = "Clipboard copied. Try searching for a keyword or type * (star) to see all docs.";
    notification.classList.remove('hidden');
});

ipcRenderer.on('clipboard_error', () => {
    message.innerText = 'Cound not copy clipboard. This rarely happens. Please contact us at (760) 364-6209‬.';
    notification.classList.remove('hidden');
});

function closeNotification() {
    notification.classList.add('hidden');
}
function restartApp() {
    ipcRenderer.send('restart_app');
}

function saveCompleteNotification(params) {
    message.innerText = 'Added query to the stories 😎. Go to the stories tab to check it out.';
    notification.classList.remove('hidden');
}

function PopNotificationInfo(text) {
    message.innerText = text;
    notification.classList.remove('hidden');
}