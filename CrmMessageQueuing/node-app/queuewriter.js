var app = require('http').createServer(handler)
var fs = require('fs');
var amqp = require('amqp'); //load the amqp library

function handler (request, response) {
	switch(request.url) {
		case '/':
			response.writeHeader(200, {"Content-Type": "text/plain"});
			response.write("Hello");
			response.end();
			break;
		case '/rabbit_post_endpoint':
			response.writeHeader(200, {"Content-Type": "text/plain"});
			if (request.method == 'POST') {
				request.on('data', function(chunk) {
					//check if received data is valid json
					if(IsJsonString(chunk.toString())){
						//convert message to json object
						var requestobject = JSON.parse(chunk.toString());
						
						//connect to rabbitmq
						var connection = amqp.createConnection({ host: requestobject.endpoint
						, port: 5672 //assumes default port
						, login: requestobject.username
						, password: requestobject.password
						, connectionTimeout: 0
						, authMechanism: 'AMQPLAIN' 
						, vhost: '/' //assumes default vhost
						});
						
						//when connection is ready
						connection.on('ready', function () {
							//get the "message" property of the supplied request
							var message = JSON.stringify(requestobject.message);
							
							//post it to the exchange with the supplied routing key
							connection.exchange = connection.exchange(requestobject.exchange, {passive: true, confirm: true }, function(exchange) {
								exchange.publish(requestobject.routingkey, message, {mandatory: true, deliveryMode: 2}, function () {
									//if successful, write message to console
									console.log('Message published: ' + message);
									
									//send "success" back in response
									response.write('success');
									
									//close the rabbitmq connection and end the response
									connection.end();
									response.end();
								});
							});
						});
						
						//if an error occurs with rabbitmq
						connection.on('error', function () {
							//send error message back in response and end it
							response.write('failure writing message to exchange');
							response.end();
						});
					}
					else {
						//if request contains invalid json
						//send error message back in response and end it
						response.write("invalid JSON");
						response.end();
					}
				});
			}
			break;
	}
}
var port = process.env.PORT || 3000;

//start the app
app.listen(port, function(){
	console.log('listening on *:' + port);
});

//function to check for valid json
function IsJsonString(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}