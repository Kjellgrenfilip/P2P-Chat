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
        private RequestData incomingRequest;
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
        public RequestData IncomingRequest
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

        public void requestConnection(String _ip, String port, String lPort)
        {
            byte[] ip = new byte[4] { 127, 0, 0, 1 };
            
            IPAddress address = new IPAddress(ip);
            IPEndPoint endPoint = new IPEndPoint(address, Int32.Parse(port));
            myPort = Int32.Parse(lPort);
     

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
                //MessageBox.Show(e.ToString());
                ConnectionError = true;
                return;
            }
           
            RequestData tmp1 = new RequestData
            {
                username = myUserName,
                port = myPort,
                ip = "127.0.0.1"
            };
            
            string jsonString = JsonConvert.SerializeObject(tmp1);
            byte[] msg = Encoding.ASCII.GetBytes(jsonString);
            sendSocket.Send(msg);

        }

        public bool listen(String port, String username)
        {
            myUserName = username;
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
            {//Den som får förfrågan kommer gå in i detta scope

                connectionSocket.Receive(buffer);

                string jsonString = Encoding.ASCII.GetString(buffer);
                RequestData? jsonObject = JsonConvert.DeserializeObject<RequestData>(jsonString);
                int tmp_p = jsonObject.port;
                byte[] ip = new byte[4] { 127, 0, 0, 1 };
                IPAddress address = new IPAddress(ip);
                IPEndPoint endPoint = new IPEndPoint(address, tmp_p);
                sendSocket.Connect(endPoint);

                IncomingRequest = jsonObject;

            }
            else//Om vi är den som väntar på respons på requesten, går vi in här
            {
                connectionSocket.Receive(buffer);
                string jsonString = Encoding.Default.GetString(buffer);
                ResponseData? jsonObject = JsonConvert.DeserializeObject<ResponseData>(jsonString);
                if (!jsonObject.accepted)
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
            string jsonString = JsonConvert.SerializeObject(new ResponseData
            {
                accepted = answer
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
        }

        public void sendMessage(String msg)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.

            string jsonString = JsonConvert.SerializeObject(new MessageData
            {
                sender = myUserName,
                message = msg,
                date = DateTime.Now.ToString("g")
            });
           
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
            MessageRecieved = jsonObject;
            Array.Clear(messageBuffer, 0, messageBuffer.Length);            
            receiveMessages();
        }



    }

    public class RequestData
    {
        public string username;
        public int port;
        public string ip;
    }
    public class ResponseData
    {
        public bool accepted;
    }
    public class MessageData
    {
        public string sender;
        public string message;
        public string date;
    }
}
