using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;

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
            byte[] ip = new byte[4];
            ip[0] = 127;
            ip[1] = byte.MinValue;
            ip[3] = 1;

            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, 8041);
            sendSocket.Connect(endPoint);
        }

        public bool listen(String port)
        {
             
            byte[] ip = new byte[4]{127,0,0,1 };
            
            int tmp_p = Int32.Parse(port);

            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, tmp_p);
            listeningSocket.Bind(endPoint);
            listeningSocket.Listen(100);
            return true;
        }

        public void sendMessage(String message)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.
          
        }
       
    }
}
