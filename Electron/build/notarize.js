const { notarize } = require('electron-notarize');

exports.default = async function notarizing(context) {
  const { electronPlatformName, appOutDir } = context;  
  if (electronPlatformName !== 'darwin') {
    return;
  }

  const appName = context.packager.appInfo.productFilename;
  console.log("appName: " + appName);

  console.log("appleid:", process.env.APPLEID);
  console.log("pass:", process.env.ITUNES_PASS);

  return await notarize({
    appBundleId: 'com.heren.librarian',
    appPath: `${appOutDir}/${appName}.app`,
    appleId: process.env.APPLEID, 
    appleIdPassword: process.env.ITUNES_PASS
  });
};