var http = require('http');
var express = require('express'),
    app = module.exports.app = express();
var server = http.createServer(app);
var io = require('socket.io').listen(server, {log:false, origins:'*:*'})
var uuid = require('node-uuid');
var sys = require('sys'),
    exec = require('child_process').exec;

app.use('/captures', express.static('captures'));

app.get('/', function (req, res) {
	res.send('home page');
});

app.get('/check_plate', function (req, res) {
	var uuid1 = uuid.v1();
	exec('fswebcam -r 1280x720 --no-banner --quiet ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		if (error !== null) {
		  console.log('exec error: ' + error);
		}
	});

	exec('alpr -j ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		//io.emit('message',stdout.toString())
		var plateOutput = JSON.parse(stdout.toString());
		plateOutput.image = '/captures/' + uuid1 + '.jpg';
		res.header("Access-Control-Allow-Origin", "*");
		res.header("Access-Control-Allow-Headers", "X-Requested-With");
		res.json(plateOutput);
		//res.send(stdout.toString());
		console.log('alpr response: ' + stdout.toString());
		if (error !== null) {
		  console.log('exec error: ' + error);
		}
	});
});

app.get('/check_plate_stream', function (req, res) {
	var uuid1 = uuid.v1();
	exec('fswebcam -r 1280x720 --no-banner --quiet ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		if (error !== null) {
		  console.log('exec error: ' + error);
		}
	});

	exec('alpr -j ./captures/' + uuid1 + '.jpg',
	  function (error, stdout, stderr) {
		var plateOutput = JSON.parse(stdout.toString());
		plateOutput.image = '/captures/' + uuid1 + '.jpg';
		//io.emit('message',plateOutput.toString());
		io.emit('message', plateOutput);
		res.send('message sent');
		console.log('alpr response: ' + stdout.toString());
		if (error !== null) {
		  console.log('exec error: ' + error);
		}
	});
});

server.listen(3000, function () {
	console.log('App listening');
});