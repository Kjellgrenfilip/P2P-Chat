using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.ViewModels.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Interop;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
       
        public ObservableCollection<MessageTest> _testList = new ObservableCollection<MessageTest>();
        

        public void OnPropertyChanged([CallerMemberName()] string name = null)
        {
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ConnectionHandler _connection;
        private String _messageToSend;
        private String _userName;
        private String _listenPort;
        private String _connectPort;
        private String _connectIP;
                
        private ICommand _connectCommand;

        private ICommand _pushCommand;
        private ICommand _listen;
        private String _listenOK ="NOT TODAY";
        private String _requestOK = "NOT TODAY";

        public ConnectionHandler Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
            }

        }

        public ObservableCollection<MessageTest> TestList
        { 
            get { return _testList; }
            set { _testList = value; } 
        }

        public String MessageToSend
        {
            get { return _messageToSend; }
            set { _messageToSend = value; }
        }

        public String UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }
        public String ListenPort
        {
            get { return _listenPort; }
            set { _listenPort = value; }
        }

        public String ConnectPort
        {
            get { return _connectPort; }
            set { _connectPort = value; }
        }
        public String ConnectIP
        {
            get { return _connectIP; }
            set { _connectIP = value; }
        }

        public String ListenOK
        {
            get { return _listenOK; }
            set { _listenOK = value;
                OnPropertyChanged();
            }
        }
        public String RequestOK
        {
            get { return _requestOK; }
            set
            {
                _requestOK = value;
                OnPropertyChanged();
            }
        }

        public ICommand PushCommand
        {
            get { return _pushCommand; }
            set { _pushCommand = value; }
        }
        public ICommand Listen
        {
            get { return _listen; }
            set { _listen = value; }
        }
        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
            set { _connectCommand = value; }
        }
        public MainViewModel(ConnectionHandler connectionHandler)
        {
            this.Connection = connectionHandler;
            Connection.PropertyChanged += EventFromModel;
            this.PushCommand = new SendMessageCommand(this);
            this.Listen = new StartListeningCommand(this);
            this.ConnectCommand = new RequestConnectionCommand(this);

            TestList.Add(new MessageTest()
            {
                msg = "Tjo man!",
                sender = "Kalle",
                date = " - [" + DateTime.Now.ToString("g") + "]: "
            });
            TestList.Add(new MessageTest()
            {
                msg = "Tjabba Killen!",
                sender = "Peter",
                date = " - [" + DateTime.Now.ToString("g") + "]: "
            });  
            TestList.Add(new MessageTest()
            {
                msg = "VGD?",
                sender = "Kalle",
                date = " - ["+DateTime.Now.ToString("g")+"]: "
            });


        }
        public void sendMessage()
        {
            Connection.sendMessage(MessageToSend);
            TestList.Add(new MessageTest()
            {
                msg = MessageToSend,
                sender = UserName,
                date = " - [" + DateTime.Now.ToString("g") + "]: "
            });
        }
        public void listen()
        {
            
            if (Connection.listen(ListenPort))
            {
                ListenOK = "Listening on PORT: " + ListenPort;
                
            }
                
            else
                ListenOK = "NOT NICE WORK MAN";

        }

        public void requestConnection()
        {
            RequestOK = "sending";
            Connection.requestConnection(ConnectIP, ConnectPort, ListenPort, UserName);

        }

        public void EventFromModel(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MessageRecieved")
            {
                TestList.Add(new MessageTest()
                {
                    msg = Connection.MessageRecieved,
                    sender = "",
                    date = " - [" + DateTime.Now.ToString("g") + "]: "
                });
            }
            
            if (e.PropertyName == "ConnectionAccepted")
            {
                if(Connection.ConnectionAccepted)
                { RequestOK = "Connected"; }
                else
                {
                    MessageBox.Show("The user denied your request :(");
                    RequestOK = "Request denied";
                }
            }
            if (e.PropertyName == "IncomingRequest")
            {
                //Fråga användaren om en connection
                MessageBoxResult msgResult = MessageBox.Show(Connection.IncomingRequest.username + " want to chat with you!!", "Some Title", MessageBoxButton.YesNo);
                if (msgResult == MessageBoxResult.Yes)
                {
                    Connection.sendResponse(true);
                }
                else
                {
                    Connection.sendResponse(false);
                }
            }
            if(e.PropertyName == "ConnectionError")
            {
                if(Connection.ConnectionError)
                {
                    MessageBox.Show("Could not connect to the given port");
                }
            }
        }
    }

    public class MessageTest
    {
        public String msg { get; set; }
        public String sender { get; set; }
        public String date { get; set; }  
    }
}
