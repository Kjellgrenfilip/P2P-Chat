using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Net.WebSockets;

namespace WpfApp1.Models
{
    public class ConnectionHandler : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName()] string name = null)
        {
            Application.Current.Dispatcher.BeginInvoke(
                
                new Action(() => {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }));
            
        }
        //Client properties
        private Socket sendSocket;
        private Socket listeningSocket;
        private Socket connectionSocket;
        //User data
        private int myPort;
        private String myUserName;
        private byte[] messageBuffer = new byte[1024];
        //Event triggers
        private bool connectionError = false;
        private bool connectionAccepted = false;
        private bool disconnection = false;
        private MessageData incomingRequest;
        private MessageData messageRecieved;

        public bool ConnectionError
        {
            get { return connectionError; }
            set
            {
                connectionError = value;
                OnPropertyChanged();
            }
        }
        public MessageData IncomingRequest
        {
            get { return incomingRequest; }
            set
            {
                incomingRequest = value;
                OnPropertyChanged();
            }
        }
        public bool ConnectionAccepted
        {
            get { return connectionAccepted; }
            set
            {
                connectionAccepted = value;
                OnPropertyChanged();
            }
        }
        public bool Disconnection
        {
            get { return disconnection; }
            set
            {
                disconnection = value;
                connectionAccepted = false;
                connectionError = false;
                OnPropertyChanged();
            }
        }
        public MessageData MessageRecieved
        {
            get { return messageRecieved; }
            set
            {
                messageRecieved = value;
                OnPropertyChanged();
            }
        }

        public ConnectionHandler()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            sendSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);

            connectionSocket = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
        }

        public void requestConnection(String _ip, String port)
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };
            
            IPAddress address = new IPAddress(ip);
           
            try 
            {
                IPEndPoint endPoint = new IPEndPoint(address, Int32.Parse(port));
                sendSocket.BeginConnect(endPoint, new AsyncCallback(onRequestSent), sendSocket);
            }
            catch(Exception e)
            {
                MessageBox.Show("PORT number or IP not valid");
            }
    
           
        }

        public void onRequestSent(IAsyncResult result)
        {
            Socket tmpSocket = (Socket)result.AsyncState;
            try
            {
                tmpSocket.EndConnect(result);
            }
            catch(Exception e)
            {
                //Kan säga att result är ogiltigt, kolla in ManualResetEvent
                //MessageBox.Show(e.ToString());
                ConnectionError = true;
                return;
            }
           
            MessageData request = new MessageData
            {
                type = MessageType.Request,
                sender = myUserName,
                message = "127.0.0.1" + ":" + myPort
            };
            
            string jsonString = JsonConvert.SerializeObject(request);
            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            sendSocket.Send(msg);

        }

        public bool listen(String port, String username)
        {
            myUserName = username;
            myPort = Int32.Parse(port);
            byte[] ip = new byte[4]{127,0,0,1};
            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, myPort);

            try
            {
                listeningSocket.Bind(endPoint);
                listeningSocket.Listen(100);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }

            listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);

            return true;
        }

        public void onAccept(IAsyncResult result)
        {
            byte[] buffer = new byte[1024];
            try
            {
                connectionSocket = listeningSocket.EndAccept(result);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }


            //Om vår sendSocket inte är ansluten: Ta emot portnummer och anslut
            if(!sendSocket.Connected)
            {
                //Den som får förfrågan kommer gå in i detta scope
                connectionSocket.Receive(buffer);

                string jsonString = Encoding.ASCII.GetString(buffer);
                MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
                int port = Int32.Parse(jsonObject.message.Substring(jsonObject.message.IndexOf(":")+1));
                byte[] ip = new byte[4] { 127, 0, 0, 1 };
                IPAddress address = new IPAddress(ip);
                IPEndPoint endPoint = new IPEndPoint(address, port);
                sendSocket.Connect(endPoint);

                IncomingRequest = jsonObject;

            }
            else
            {
                //Om vi är den som väntar på respons på requesten, går vi in här
                connectionSocket.Receive(buffer);
                string jsonString = Encoding.Default.GetString(buffer);
                MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
                if (jsonObject.message == "denied")
                {
                    ConnectionAccepted = false;
                    sendSocket.Shutdown(SocketShutdown.Both);
                    sendSocket.Close();
                    sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
                    return;
                }

                ConnectionAccepted = true;

            }
        }

        public void sendResponse(bool answer)
        {
            string jsonString = JsonConvert.SerializeObject(new MessageData
            {
                type = MessageType.Response,
                sender = myUserName,
                message = (answer)? "accepted" : "denied",
                date = DateTime.Now.ToString("g")
            }); 
            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            sendSocket.Send(msg);
            if(answer == false)
            {
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
                sendSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
                listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
            }
            else
            {
                ConnectionAccepted = true;
            }
           
        }

        public void sendMessage(String msg = null)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.

            string jsonString;

            if(msg != null)
            {
                jsonString = JsonConvert.SerializeObject(new MessageData
                {
                    type = MessageType.Message,
                    sender = myUserName,
                    message = msg,
                    date = DateTime.Now.ToString("g")
                });
            }

            else
            {
                jsonString = JsonConvert.SerializeObject(new MessageData
                {
                    type = MessageType.Buzz,
                    sender = myUserName,
                    message = "",
                    date = DateTime.Now.ToString("g")
                });
            }


            byte[] buf = Encoding.ASCII.GetBytes(jsonString);
            sendSocket.Send(buf);
        }

        public void receiveMessages()
        {
            connectionSocket.BeginReceive(messageBuffer, 0, 1024, 0, new AsyncCallback(onReceive), connectionSocket);
        }

        public void onReceive(IAsyncResult result)
        {
            string jsonString = Encoding.ASCII.GetString(messageBuffer);
         
            MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
            if(jsonObject != null)
            {
                MessageRecieved = jsonObject;
                Array.Clear(messageBuffer, 0, messageBuffer.Length);
                receiveMessages();
            }
            else if(sendSocket.Connected)
            {
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
                sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Disconnection = true;
                listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);                  
            }
        }
        public bool Disconnect()
        {
            if (!sendSocket.Connected)
                return false;
            sendSocket.Shutdown(SocketShutdown.Both);
            sendSocket.Close();
            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Disconnection = true;
            listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
            return true;
        }


    }

   
  
    public class MessageData
    {
        public MessageType type;
        public string sender;
        public string message;
        public string date;
    }
   
    public enum MessageType
    {
        Request,    //0
        Response,   //1
        Message,    //2
        Buzz        //3
    }
}
