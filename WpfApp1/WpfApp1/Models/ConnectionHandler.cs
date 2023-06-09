﻿using System;
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
        private String exception;

        public String ExceptionMessage
        {
            get { return exception; }
            set
            {
                exception = value;
                OnPropertyChanged();
            }
        }
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
            if (_ip == "")
                _ip = "127.0.0.1";

            System.Net.IPAddress address;
            IPEndPoint endPoint;

            try 
            {
                address = System.Net.IPAddress.Parse(_ip);
                endPoint = new IPEndPoint(address, Int32.Parse(port));
                
            }
            catch(Exception e)
            {
                ExceptionMessage = "PORT number or IP not valid";
                return;
            }
            sendSocket.BeginConnect(endPoint, new AsyncCallback(onRequestSent), sendSocket);
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
            sendSocket.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(onSent), sendSocket);

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
                ExceptionMessage = e.Message;
                return false;
            }

            listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);

            return true;
        }

        //Callback for accepting incoming connections
        public void onAccept(IAsyncResult result)
        {
             
            byte[] buffer = new byte[1024];
            //Try ending the asynchronous operation
            try
            {
                connectionSocket = listeningSocket.EndAccept(result);
            }
            catch(Exception e)
            {
                ExceptionMessage = e.Message;
            }


            //If our sendSocket is not connected: Receive portnumber and connect back
            if(!sendSocket.Connected)
            {
                //The client that receives the request will enter this scope
                connectionSocket.Receive(buffer);

                string jsonString = Encoding.ASCII.GetString(buffer);
                MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
                int port = Int32.Parse(jsonObject.message.Substring(jsonObject.message.IndexOf(":")+1));
                byte[] ip = new byte[4] { 127, 0, 0, 1 };
                IPAddress address = new IPAddress(ip);
                IPEndPoint endPoint = new IPEndPoint(address, port);
                sendSocket.Connect(endPoint);
                //Raise IncomingRequest event to view model
                IncomingRequest = jsonObject;
            }
            else
            {
                //The client that initiated the request will enter this scope
                connectionSocket.Receive(buffer);
                string jsonString = Encoding.Default.GetString(buffer);
                MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
                if (jsonObject.message == "denied")
                {
                    //Raise event to view model
                    ConnectionAccepted = false;
                    sendSocket.Shutdown(SocketShutdown.Both);
                    sendSocket.Close();
                    sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
                    return;
                }
                //Raise event to view model
                ConnectionAccepted = true;

            }
        }

        public void sendResponse(bool answer)
        {
            //Pack message into a MessageData object and convert to json
            string jsonString = JsonConvert.SerializeObject(new MessageData
            {
                type = MessageType.Response,
                sender = myUserName,
                message = (answer)? "accepted" : "denied",
                date = DateTime.Now.ToString("g")
            }); 
            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            //Send message asynchronously and use callback below
            sendSocket.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(onSent), sendSocket);
            //If we chose to refuse the connection request
            if (answer == false)
            {
                //Cleanup and create new socket
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
                sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Begin accepting new connections from queue
                listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);
            }
            else
            {
                ConnectionAccepted = true;
            }
           
        }

        public void sendMessage(String message = null)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.

            string jsonString;

            if(message != null)
            {
                jsonString = JsonConvert.SerializeObject(new MessageData
                {
                    type = MessageType.Message,
                    sender = myUserName,
                    message = message,
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


            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            //Send message asynchronously and use callback below
            sendSocket.BeginSend(msg, 0, msg.Length, 0, new AsyncCallback(onSent), sendSocket);
        }

        public void onSent(IAsyncResult result)
        {
            //Nothing useful to do here
            Socket tmp = (Socket)result.AsyncState;
            try 
            {
                tmp.EndSend(result);
            }
            catch(Exception e)
            {
                ExceptionMessage = "Could not send message: " + e.Message;
            }

        }

        public void receiveMessages()
        {
            //Receive incoming message asynchronously and use the callback below
            connectionSocket.BeginReceive(messageBuffer, 0, 1024, 0, new AsyncCallback(onReceive), connectionSocket);
        }

        public void onReceive(IAsyncResult result)
        {
            string jsonString = Encoding.ASCII.GetString(messageBuffer);
         
            //Try to interpret messages as a MessageData object
            MessageData? jsonObject = JsonConvert.DeserializeObject<MessageData>(jsonString);
            if(jsonObject != null)
            {
                //Raise messager eceived event to view model
                MessageRecieved = jsonObject;
                Array.Clear(messageBuffer, 0, messageBuffer.Length);
                //Receive more messages
                receiveMessages();
            }
            //If interpreting failed we assume disconnection
            else if(sendSocket.Connected)
            {
                //Cleanup and create new socket
                sendSocket.Shutdown(SocketShutdown.Both);
                sendSocket.Close();
                sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Raise disconnect event to view model
                Disconnection = true;
                //Begin accepting new connections from queue
                listeningSocket.BeginAccept(new AsyncCallback(onAccept), listeningSocket);                  
            }
        }
        public bool Disconnect()
        {
            if (!sendSocket.Connected)
                return false;
            //Cleanup and create new socket
            sendSocket.Shutdown(SocketShutdown.Both);
            sendSocket.Close();
            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Raise disconnect event to view model
            Disconnection = true;
            //Begin accepting new connections from queue
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
