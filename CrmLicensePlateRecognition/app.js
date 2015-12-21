var http = require('http');
var express = require('express'),
    app = module.exports.app = express();
var server = http.createServer(app);
var io = require('socket.io').listen(server, {log:false, origins:'*:*'})
var uuid = require('node-uuid');
var sys = require('sys'),
    exec = require('child_process').exec;

//allow clients to directly view the images in the captures directory
app.use('/captures', express.static('captures'));

//route for the home page
app.get('/', function (req, res) {
	res.send('home page');
});

//route to handle a client calling node to check a plage
app.get('/check_plate', function (req, res) {
	//generate a guid to use in the captured image file name
	var uuid1 = uuid.v1();
	
	//tell the webcam to take a picture and store it in the captures directory using the guid as the name
	exec('fswebcam -r 1280x720 --no-banner --quiet ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		if (error !== null) {
		  //log any errors
		  console.log('exec error: ' + error);
		}
	});

	//now that we have a picture saved, execute parse it with openalpr and return the results as json (the -j switch) 
	exec('alpr -j ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		//create a json object based on the alpr output
		var plateOutput = JSON.parse(stdout.toString());
		
		//add an "image" attribute to the alpr json that has a path to the captured image
		//this is so the client can view the license plage picture to verify alpr parsed it correctly
		plateOutput.image = '/captures/' + uuid1 + '.jpg';
		
		//set some headers to deal with CORS
		res.header("Access-Control-Allow-Origin", "*");
		res.header("Access-Control-Allow-Headers", "X-Requested-With");
		
		//send the json back to the client
		res.json(plateOutput);
		
		//log the response from alpr
		console.log('alpr response: ' + stdout.toString());
		
		if (error !== null) {
		  //log any errors
		  console.log('exec error: ' + error);
		}
	});
});

//route to handle a request for a license plate capture to be written to a socket.io interface
//basically the same as the non-streaming interface except the output gets written somewhere different
app.get('/check_plate_stream', function (req, res) {
	//generate a guid to use in the captured image file name
	var uuid1 = uuid.v1();
	
	//tell the webcam to take a picture and store it in the captures directory using the guid as the name
	exec('fswebcam -r 1280x720 --no-banner --quiet ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		if (error !== null) {
		  //log any errors
		  console.log('exec error: ' + error);
		}
	});
	
	//now that we have a picture saved, execute parse it with openalpr and return the results as json (the -j switch) 
	exec('alpr -j ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		//create a json object based on the alpr output
		var plateOutput = JSON.parse(stdout.toString());
		
		//add an "image" attribute to the alpr json that has a path to the captured image
		//this is so the client can view the license plage picture to verify alpr parsed it correctly
		plateOutput.image = '/captures/' + uuid1 + '.jpg';

		//write the json to the socket.io interface
		io.emit('message', plateOutput);

		//return a response to the caller that the message was sent
		res.send('message sent');

		//log the response from alpr
		console.log('alpr response: ' + stdout.toString());
		
		if (error !== null) {
		  //log any errors
		  console.log('exec error: ' + error);
		}
	});
});

//start the server listening on port 3000
server.listen(3000, function () {
	console.log('App listening');
});