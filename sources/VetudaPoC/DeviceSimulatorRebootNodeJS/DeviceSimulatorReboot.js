'use strict';

var Client = require('azure-iot-device').Client;
var Protocol = require('azure-iot-device-mqtt').Mqtt;

var connectionString = 'HostName=vetuda.azure-devices.net;DeviceId=Vehicle_0;SharedAccessKey=dbXqDmDOU9903d4xseWUTH7pazl2Z41igUaFKC1Pl28=';
var client = Client.fromConnectionString(connectionString, Protocol);

client.open(function (err) {
    if (err) {
        console.error('Could not open IotHub client');
    } else {
        console.log('Client opened.  Waiting for reboot or firmware update method.');
        client.onDeviceMethod('reboot', onReboot);
        //client.onDeviceMethod('firmwareUpdate', onFirmwareUpdate);

    }
});

var reportFWUpdateThroughTwin = function (twin, firmwareUpdateValue) {
    var patch = {
        iothubDM: {
            firmwareUpdate: firmwareUpdateValue
        }
    };

    twin.properties.reported.update(patch, function (err) {
        if (err) throw err;
        console.log('twin state reported')
    });
};



var onReboot = function (request, response) {

    // Respond the cloud app for the direct method
    response.send(200, 'Reboot started', function (err) {
        if (!err) {
            console.error('An error occured when sending a method response:\n' + err.toString());
        } else {
            console.log('Response to method \'' + request.methodName + '\' sent successfully.');
        }
    });

    // Report the reboot before the physical restart
    var date = new Date();
    var patch = {
        iothubDM: {
            reboot: {
                lastReboot: date.toISOString(),
            }
        }
    };

    // Get device Twin
    client.getTwin(function (err, twin) {
        if (err) {
            console.error('could not get twin');
        } else {
            console.log('twin acquired');
            twin.properties.reported.update(patch, function (err) {
                if (err) throw err;
                console.log('Device reboot twin state reported')
            });
        }
    });

    // Add your device's reboot API for physical restart.
    console.log('Rebooting!');
};

var onFirmwareUpdate = function(request, response) {

  // Respond the cloud app for the direct method
  response.send(200, 'FirmwareUpdate started', function(err) {
    if (!err) {
      console.error('An error occured when sending a method response:\n' + err.toString());
    } else {
      console.log('Response to method \'' + request.methodName + '\' sent successfully.');
    }
  });

  // Get the parameter from the body of the method request
  var fwPackageUri = JSON.parse(request.payload).fwPackageUri;

  // Obtain the device twin
  client.getTwin(function(err, twin) {
    if (err) {
      console.error('Could not get device twin.');
    } else {
      console.log('Device twin acquired.');

      // Start the multi-stage firmware update
      waitToDownload(twin, fwPackageUri, function() {
        downloadImage(twin, fwPackageUri, function(imageData) {
          applyImage(twin, imageData, function() {});    
        });  
      });

    }
  });
}

var downloadImage = function (twin, fwPackageUriVal, callback) {
    var now = new Date();

    reportFWUpdateThroughTwin(twin, {
        status: 'downloading',
    });

    setTimeout(function () {
        // Simulate download
        simulateDownloadImage(fwPackageUriVal, function (err, image) {

            if (err) {
                reportFWUpdateThroughTwin(twin, {
                    status: 'downloadfailed',
                    error: {
                        code: error_code,
                        message: error_message,
                    }
                });
            } else {
                reportFWUpdateThroughTwin(twin, {
                    status: 'downloadComplete',
                    downloadCompleteTime: now.toISOString(),
                });

                setTimeout(function () {
                    callback(image);
                }, 4000);
            }
        });

    }, 4000);
}

var applyImage = function(twin, imageData, callback) {
  var now = new Date();   

  reportFWUpdateThroughTwin(twin, {
    status: 'applying',
    startedApplyingImage : now.toISOString()
  });

  setTimeout(function() {

    // Simulate apply firmware image
    simulateApplyImage(imageData, function(err) {
      if (err) {
        reportFWUpdateThroughTwin(twin, {
          status: 'applyFailed',
          error: {
            code: err.error_code,
            message: err.error_message,
          }
        });
      } else { 
        reportFWUpdateThroughTwin(twin, {
          status: 'applyComplete',
          lastFirmwareUpdate: now.toISOString()
        });    

      }
    });

    setTimeout(callback, 4000);

  }, 4000);
}

var simulateDownloadImage = function (imageUrl, callback) {
    var error = null;
    var image = "[fake image data]";

    console.log("Downloading image from " + imageUrl);

    callback(error, image);
}

var simulateApplyImage = function (imageData, callback) {
    var error = null;

    if (!imageData) {
        error = {
            message: 'Apply image failed because of missing image data.'
        };
    }

    callback(error);
}

var waitToDownload = function (twin, fwPackageUriVal, callback) {
    var now = new Date();

    reportFWUpdateThroughTwin(twin, {
        fwPackageUri: fwPackageUriVal,
        status: 'waiting',
        error: null,
        startedWaitingTime: now.toISOString()
    });
    setTimeout(callback, 4000);
};