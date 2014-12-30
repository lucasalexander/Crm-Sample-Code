using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;


namespace CliProvider
{
    class Send
    {
        private static string _brokerEndpoint;
        private static string _brokerUser;
        private static string _brokerPassword;
        private static string _exchange;
        private static string _routingKey;

        /// <summary>
        /// reads config data from CLI
        /// </summary>
        public static void GetConfig()
        {
            Console.Write("Enter the RabbitMQ endpoint: ");
            _brokerEndpoint = Console.ReadLine();
            Console.Write("Enter the RabbitMQ username: ");
            _brokerUser = Console.ReadLine();
            Console.Write("Enter the RabbitMQ password: ");
            _brokerPassword = ReadPassword();
            Console.Write("Enter the RabbitMQ exchange name: ");
            _exchange = Console.ReadLine();
            Console.Write("Enter the RabbitMQ routing key: ");
            _routingKey = Console.ReadLine();
        }

        public static void Main()
        {
            //get the config data
            GetConfig();

            //connect to rabbitmq
            Console.WriteLine("Connecting . . .");
            var factory = new ConnectionFactory();
            factory.HostName = _brokerEndpoint;
            factory.UserName = _brokerUser;
            factory.Password = _brokerPassword;
            factory.VirtualHost = "/"; //assumes we use the default vhost
            factory.Protocol = Protocols.DefaultProtocol; //assumes we use the default protocol
            factory.Port = AmqpTcpEndpoint.UseDefaultPort; //assumes we use the default port
            IConnection conn = factory.CreateConnection();
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //tell rabbitmq to send confirmation when messages are successfully published
                    channel.ConfirmSelect();
                    channel.WaitForConfirmsOrDie();
                    Console.WriteLine("Connected to " + _brokerEndpoint);
                    Console.WriteLine(" [*] Ready to send messages." +
                                             "To exit press CTRL+C");
                    while(true)
                    {
                        //loop for user to supply messages
                        ProcessMessages(channel);
                    }
                }
            }
        }

        public static void ProcessMessages(IModel channel)
        {
            //read the message from the cli
            Console.WriteLine(" [x] Type a message: ");
            string message = Console.ReadLine();
            
            //prepare message to write to queue
            var body = Encoding.UTF8.GetBytes(message);

            var properties = channel.CreateBasicProperties();
            properties.SetPersistent(true);

            //set a unique identifier we could use later to reference the message when it's processed
            string messageId = Guid.NewGuid().ToString();
            properties.CorrelationId = messageId;
            
            //publish the message to the exchange with the supplied routing key
            channel.BasicPublish(_exchange, _routingKey, properties, body);

            //write a confirmation to the cli
            Console.WriteLine(" [x] Sent {0} - {1}", message, messageId);
        }

        /// <summary>
        /// method to mask sensitive input - taken from http://rajeshbailwal.blogspot.com/2012/03/password-in-c-console-application.html
        /// </summary>
        /// <returns></returns>
        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write("*");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}
