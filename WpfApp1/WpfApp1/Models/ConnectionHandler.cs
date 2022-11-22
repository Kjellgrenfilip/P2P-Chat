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

        public void requestConnection(String _ip, String port, String lPort, String username, AsyncCallback tmpCB)
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };
            
            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, Int32.Parse(port));

            
            try
            {
                sendSocket.BeginConnect(endPoint, new AsyncCallback(onRequestSent), lPort);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

        }



        public void onRequestSent(IAsyncResult result)
        {
            byte[] msg = Encoding.ASCII.GetBytes((String)result.AsyncState);
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
            try
            {
                listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
                
            return true;
        }

        public void onAccept(IAsyncResult result)
        {
            byte[] buffer = new byte[1024];
            connectionSocket = listeningSocket.EndAccept(result);

            //Om vår sendSocket inte är ansluten: Ta emot portnummer och anslut
            if(!sendSocket.Connected)
            {
                connectionSocket.Receive(buffer);
                

                int tmp_p = Int32.Parse(System.Text.Encoding.Default.GetString(buffer));

                byte[] ip = new byte[4] { 127, 0, 0, 1 };
                IPAddress address = new IPAddress(ip);
                IPEndPoint endPoint = new IPEndPoint(address, tmp_p);
                sendSocket.Connect(endPoint);
            }
            while (true)
            {
                connectionSocket.Receive(buffer);
                //listeningSocket.Receive(buffer);
                MessageBox.Show(System.Text.Encoding.Default.GetString(buffer));
            }
            }

        public void sendMessage(String message)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.
            byte[] buf = Encoding.ASCII.GetBytes(message);
            sendSocket.Send(buf);
          
        }


       
    }
}
