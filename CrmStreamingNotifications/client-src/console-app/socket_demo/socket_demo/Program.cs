using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.SocketIoClientDotNet.Client;

namespace socket_demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = IO.Socket("http://lucas-ajax.cloudapp.net:3000/");
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                socket.On("message", (data) =>
                {
                    Console.WriteLine(data);
                    //socket.Disconnect();
                });
            });
            Console.ReadLine();
        }
    }
}
