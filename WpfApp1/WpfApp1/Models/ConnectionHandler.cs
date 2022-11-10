using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.Threading;


namespace WpfApp1.Models
{
    public class ConnectionHandler
    {
        private Socket sendSocket;
        private Socket listeningSocket;


        public ConnectionHandler()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp); 
            
            sendSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        }

        public void requestConnection()
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };

            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, 8041);
            sendSocket.Connect(endPoint);
        }

        public void listen()
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };

            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, 8041);
            listeningSocket.Bind(endPoint);
            listeningSocket.Listen(100);




        }

        public void sendMessage(String message)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.
            if(message == "listen")
            {
                listen();
            }
            else if(message == "connect")
            {
                requestConnection();
            }
            else if(message == "accept")
            {
                Socket socket = listeningSocket.Accept();
                byte[] buffer = new byte[1024];
                socket.Receive(buffer);
                MessageBox.Show(Encoding.Default.GetString(buffer));
            }
            else if (message == "send")
            {
                sendByte();
            }
            else if (message == "recieve")
            {
                requestConnection();
            }
        }
    
        public void sendByte()
        {
            byte[] msg = Encoding.Default.GetBytes("Make Love, not WAR");
            sendSocket.Send(msg);
        }
    }
}
