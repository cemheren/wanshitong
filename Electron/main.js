// Modules to control application life and create native browser window
const {app, BrowserWindow, ipcMain, globalShortcut} = require('electron')
const path = require('path')
const child_process = require('child_process').execFile;
const { autoUpdater } = require('electron-updater');
const request = require("request");
const electronLog = require("electron-log");
const os = require("os");
const Store = require('electron-store');

// Handle logging
autoUpdater.logger = electronLog;
autoUpdater.logger.transports.file.level = "info";
Object.assign(console, electronLog.functions);

// Handle local storage
const store = new Store({"cwd": path.join(os.homedir(), "Index")});

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow
let loading

function createWindow () {
  // Create the loading window.
  loading = new BrowserWindow({
    width: 600,
    height: 400,
    frame: false, 
    show: false,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: true
    },
    icon: __dirname + '/icon.ico'
  });

  loading.once('show', () => {
    mainWindow = new BrowserWindow({
      width: 1200,
      height: 800,
      frame: false, 
      show: false,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js'),
        nodeIntegration: true
      },
      icon: __dirname + '/icon.ico'
    });
    mainWindow.webContents.once('did-finish-load', () => {
      console.log('mainWindow loaded')
      mainWindow.show()
      loading.hide()
      loading.close();
      // Open the DevTools.
      // mainWindow.webContents.openDevTools()
    })
    // long loading html
    mainWindow.loadFile('index.html');
    mainWindow.on('closed', function () {
      // Dereference the window object, usually you would store windows
      // in an array if your app supports multi windows, this is the time
      // when you should delete the corresponding element.
      mainWindow = null
    })
  })
  loading.loadFile('loading.html')
  loading.show();
  console.log('mainWindow loaded')

  createServerProcess();
  registerShortcuts();

  autoUpdater.checkForUpdatesAndNotify();

  setInterval(() => {
    autoUpdater.checkForUpdatesAndNotify();
  }, 1000 * 60 * 15);
}

function createServerProcess() {
  var isWin = process.platform === "win32";
  
  var appPath = app.getAppPath();
  var version = app.getVersion();
  
  console.log(appPath);
  console.log(version);
  
  var serverPath;
  if(isWin)
  {
    serverPath = "server/win/indexer.exe";
  }
  else
  {
    serverPath = path.normalize(`${appPath}/../../server/osx/Indexer`);
  }
  var serverProcess = child_process(serverPath, [version, "d9707c0b9f544cb1b974caff2041a589"], function(err, data) {
      if(err){
        console.error(err);
        return;
      }
  
      console.log(data.toString());
  });
}

function registerShortcuts() {
  var ret = globalShortcut.register('alt+a', () => {

    request.get(
        "http://localhost:4153/actions/screenshot",
        function (error, response, body) {
            if (!error && response.statusCode == 200 && response.body == "true") {
                mainWindow.webContents.send('screenshot');
            }else{
                mainWindow.webContents.send('screenshot_error');
            }
        }
    );
  })
  
  if (!ret) {
    console.log('registration failed alt+a');
  }

  ret = globalShortcut.register('alt+c', () => {
    request.get(
        "http://localhost:4153/actions/clipboard",
        function (error, response, body) {
            if (!error && response.statusCode == 200 && response.body == "true") {
              mainWindow.webContents.send('clipboard');
            }else{
              mainWindow.webContents.send('clipboard_error');
            }
        }
    );
  })

  if (!ret) {
    console.log('registration failed');
  }
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') app.quit()
})

app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (mainWindow === null) createWindow()
})

autoUpdater.on('update-available', () => {
  mainWindow.webContents.send('update_available');
});

autoUpdater.on('update-downloaded', () => {
  mainWindow.webContents.send('update_downloaded');
});

ipcMain.on('restart_app', () => {
  autoUpdater.quitAndInstall();
});

ipcMain.on('app_version', (event) => {
  event.sender.send('app_version', { version: app.getVersion() });
});

ipcMain.on('app_close', () => { app.quit() })
ipcMain.on('app_minimize', () => { mainWindow.minimize() })
ipcMain.on('app_maximize', () => { 
  var is = mainWindow.isMaximized(); 
  if(is)
    mainWindow.unmaximize();
  else
    mainWindow.maximize();
})
ipcMain.on('app_fullscreen', () => 
{
  var is = mainWindow.isMaximized(); 
  if(is)
    mainWindow.unmaximize();
  else
    mainWindow.maximize();
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.
