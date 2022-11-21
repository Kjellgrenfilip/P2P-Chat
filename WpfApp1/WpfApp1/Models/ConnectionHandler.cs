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
        private Socket connectionSocket;

        public ConnectionHandler()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp); 
            
            sendSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);


            connectionSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        }

        public void requestConnection(String _ip, String port, AsyncCallback tmpCB)
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };
            
            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, Int32.Parse(port));
            
            sendSocket.BeginConnect(endPoint, new AsyncCallback(shitCallback), sendSocket.Connected);
            //Start task
            //
        }

        public void shitCallback(IAsyncResult result)
        {
            byte[] msg = new byte[5] {72,73,74,75,76 };
            sendSocket.Send(msg);
        }

        public bool listen(String port, AsyncCallback a)
        {
             
            byte[] ip = new byte[4]{127,0,0,1 };
            
            int tmp_p = Int32.Parse(port);

            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, tmp_p);
            listeningSocket.Bind(endPoint);
            listeningSocket.Listen(100);
            listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
            return true;
        }

        public void onAccept(IAsyncResult result)
        {
            byte[] buffer = new byte[1024];
            //connectionSocket = listeningSocket.EndAccept(result);
            //connectionSocket.Receive(buffer);
            listeningSocket.Receive(buffer);
            MessageBox.Show(System.Text.Encoding.Default.GetString(buffer));
        }

        public void sendMessage(String message)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.
          
        }


       
    }
}
