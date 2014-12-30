using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CliConsumer
{
    class Receive
    {
        private static string _brokerEndpoint;
        private static string _brokerUser;
        private static string _brokerPassword;
        private static string _queue;
        
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
            Console.Write("Enter the RabbitMQ queue name: ");
            _queue = Console.ReadLine();
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
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //wait for some messages
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(_queue, false, consumer);
                    
                    Console.WriteLine(" [*] Waiting for messages." +
                                             "To exit press CTRL+C");
                    while (true)
                    {
                        //get the message from the queue
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        //write to cli
                        Console.WriteLine(" [x] Received message");
                        Console.WriteLine("     Body: {0}", message);

                        //IMPORTANT - tell the queue the message was processed successfully so it doesn't get requeued
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
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
