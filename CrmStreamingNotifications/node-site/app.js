var app = require('http').createServer(handler)
var io = require('socket.io')(app);
var fs = require('fs');

function handler (request, response) {
	switch(request.url) {
		case '/':
			response.writeHeader(200, {"Content-Type": "text/plain"});
			response.write("Hello");
			response.end();
			break;
		case '/post_endpoint':
			response.writeHeader(200, {"Content-Type": "text/plain"});
			if (request.method == 'POST') {
				request.on('data', function(chunk) {
					io.emit('message',chunk.toString());
					response.write(chunk.toString());
					console.log('message received: ' + chunk.toString());
				});
			}
			response.end();
			break;
	}
}
var port = process.env.PORT || 3000;

app.listen(port, function(){
console.log('listening on *:' + port);
});