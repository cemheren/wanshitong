const { notarize } = require('electron-notarize');

exports.default = async function notarizing(context) {
  const { electronPlatformName, appOutDir } = context;  
  if (electronPlatformName !== 'darwin') {
    return;
  }

  const appName = context.packager.appInfo.productFilename;
  console.log("appName: " + appName);

  return await notarize({
    appBundleId: 'com.heren.librarian',
    appPath: `${appOutDir}/${appName}.app`,
    appleId: "cemheren@gmail.com", //process.env.APPLEID, 
    appleIdPassword: "offl-rojz-xhxq-yujh" // "Patates55" // process.env.ITUNES_PASS
  });
};