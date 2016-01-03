/*
 * This script must be run as root because it requires access to the physical memory. 
 * Recurring distance checking logic based on https://github.com/clebert/r-pi-usonic/blob/master/examples/surveyor.js
 */

var statistics = require('math-statistics');
var usonic = require('r-pi-usonic');
var http = require('http');

//gpio pin connected to echo pin on HC-SR04 sensor
var	echoPin = 27;

//gpio pin connected to trigger pin on HC-SR04 sensor
var	triggerPin = 17;

//milliseconds to wait before we consider it a measurement timeout
var	timeout = 750;

//milliseconds between each ping
var	delay = 50;

//how many samples to take for a single distance measurement
var	rate = 5;

//host, port, path to streaming notification page
var reqOptions = {
    host: 'localhost',
    port: 3000,
    path: '/check_plate_stream'
  };

//flag for whether motion stop notification has been sent already
var notified = false;

//for tracking the last time motion was detected
var lastmovement = Date.now();

//the last distance detected
//this default value and the lastmovement default value will result in a notification being sent at startup
var lastdistance = 0;

var print = function (distances) {
	//get the median of all the last N distance readings
    var distance = statistics.median(distances);

	//check the delta between the current distance and the last one
    var distancechange = Math.abs(lastdistance - distance);

	//if distance < 0, then we have a measurement timeout
    if (distance < 0) {
        console.log('Error: Measurement timeout.');
    } else {
		//if the distance has changed by 1 cm or more, consider it movement
		if(distancechange >= 1) {
			//update the last time motion was detected
			lastmovement = Date.now();
			
			//reset the notification flag so that we can send a notification when movement stops
			notified = false;
			
			//log info
			console.log('Motion detected.');
			console.log('  Distance: ' + distance.toFixed(1) + ' cm ');
		} else {
			//no motion, so get the delta between now and the last time motion was detected in milliseconds
			var timestill = Date.now() - lastmovement;
			
			//if it has been at least 1000 milliseconds and notified flag is false
			if (timestill >= 1000 && !notified) {
				//log info
				console.log('Motion stopped.');
				console.log('  Distance: ' + distance.toFixed(1) + ' cm ');
				
				//call the notifier page
				var req = http.get(reqOptions, function(response) {
					var res_data = '';
					response.on('data', function(chunk) {
						res_data += chunk;
					});
					response.on('end', function() {
						console.log(res_data);
					});
				});
				req.on('error', function(err) {
					console.log("Request error: " + err.message);
				});

				//reset the notification flag
				notified = true;
			}
		}
		
		//update the lastdistance value to the current distance value
		lastdistance = distance;
    }
};

var initSensor = function (config) {
    var sensor = usonic.createSensor(config.echoPin, config.triggerPin, config.timeout);
    //console.log('Config: ' + JSON.stringify(config));

    var distances;

    (function measure() {
        if (!distances || distances.length === config.rate) {
            if (distances) {
                print(distances);
            }
            distances = [];
        }

        setTimeout(function () {
            distances.push(sensor());
            measure();
        }, config.delay);
    }());
};

usonic.init(function (error) {
	if (error) {
		console.log(error);
	} else {
		initSensor({
			echoPin: echoPin,
			triggerPin: triggerPin,
			timeout: timeout,
			delay: delay,
			rate: rate
		});
	}
});
