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
using System.Media;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
       
        public ObservableCollection<MessageContent> _messageList = new ObservableCollection<MessageContent>();
        public ObservableCollection<HistoryData> _conversationList = new ObservableCollection<HistoryData>();
        public ObservableCollection<MessageContent> _historyConversation = new ObservableCollection<MessageContent>();

        public void OnPropertyChanged([CallerMemberName()] string name = null)
        {
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ConnectionHandler _connection;
        private HistoryHandler _historyHandler;
        //variables for I/O
        private String _messageToSend = "";
        private String _userName="";
        private String _listenPort="";
        private String _connectPort;
        private String _connectIP;
        private String _searchTerm;
        //Variables for styling the interface
        private String _listeningStatusColor = "Red";
        private String _connectionStatusColor = "Red";
        private String _messageBoxColor = "Black";
        private String _showHistory = "Hidden";
        //ButtonCommands
        private ICommand _connectCommand;
        private ICommand _disconnectCommand;
        private ICommand _pushCommand;
        private ICommand _listen;
        private ICommand _historyCommand;
        private ICommand _searchCommand;
        private ICommand _showConversationCommand;
        private ICommand _buzzCommand;
        //Status messages
        private String _listenStatus ="NOT LISTENING";
        private String _connectionStatus = "NOT CONNECTED";

        public ConnectionHandler Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
            }
        }
        public HistoryHandler HistoryHandler
        {
            get { return _historyHandler; }
            set
            {
                _historyHandler = value;
            }
        }

        public ObservableCollection<MessageContent> MessageList
        { 
            get { return _messageList; }
            set { _messageList = value; } 
        }

        public ObservableCollection<HistoryData> ConversationList
        {
            get { return _conversationList; }
            set { _conversationList = value; }
        }
        public ObservableCollection<MessageContent> HistoryConversation
        {
            get { return _historyConversation; }
            set { _historyConversation = value; }
        }
        public String MessageToSend
        {
            get { return _messageToSend; }
            set { _messageToSend = value;
                OnPropertyChanged();
            }
        }
        public String SearchTerm
        {
            get { return _searchTerm; }
            set { _searchTerm = value; }
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
        public String ListeningStatusColor
        {
            get { return _listeningStatusColor; }
            set { _listeningStatusColor = value;
                OnPropertyChanged();
            }
        }
        public String ConnectionStatusColor
        {
            get { return _connectionStatusColor; }
            set
            {
                _connectionStatusColor = value;
                OnPropertyChanged();
            }
        }
        public String MessageBoxColor
        {
            get { return _messageBoxColor; }
            set
            {
                _messageBoxColor = value;
                OnPropertyChanged();
            }
        }
        public String ShowHistory
        {
            get { return _showHistory; }
            set
            {
                _showHistory = value;
                OnPropertyChanged();
            }
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
        public String ListenStatus
        {
            get { return _listenStatus; }
            set { _listenStatus = value;
                OnPropertyChanged();
            }
        }
        public String ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                _connectionStatus = value;
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
        public ICommand DisconnectCommand
        {
            get { return _disconnectCommand; }
            set { _disconnectCommand = value; }
        }

        public ICommand HistoryCommand
        {
            get { return _historyCommand; }
            set { _historyCommand = value; }
        }

        public ICommand SearchCommand
        {
            get { return _searchCommand; }
            set { _searchCommand = value; }
        }

        public ICommand ShowConversationCommand
        {
            get { return _showConversationCommand; }
            set { _showConversationCommand = value; }
        }

        public ICommand BuzzCommand
        {
            get { return _buzzCommand; }
            set { _buzzCommand = value; }
        }

        public MainViewModel(ConnectionHandler connectionHandler, HistoryHandler h)
        {
            this.Connection = connectionHandler;
            this.HistoryHandler = h;
            Connection.PropertyChanged += EventFromModel;
            this.PushCommand = new SendMessageCommand(this);
            this.Listen = new StartListeningCommand(this);
            this.ConnectCommand = new RequestConnectionCommand(this);
            this.DisconnectCommand = new DisconnectCommand(this);
            this.HistoryCommand = new HistoryCommand(this);
            this.SearchCommand = new SearchCommand(this);
            this.ShowConversationCommand = new ShowConversationCommand(this);
            this.BuzzCommand = new BuzzCommand(this);
        }
        public void sendMessage()
        {
            if (MessageToSend != "")
            {
                if (Connection.ConnectionAccepted)
                {
                    Connection.sendMessage(MessageToSend);
                    MessageBoxColor = "Orange";
                    MessageList.Add(new MessageContent()
                    {
                        msg = MessageToSend,
                        sender = UserName,
                        date = " - [" + DateTime.Now.ToString("g") + "]: ",
                        boxColor = "#F4DB7D"
                    });

                }
                else
                {
                    MessageBox.Show("Could not send message: Not connected to user.");
                }
                MessageToSend = "";
            }
        }
        public void sendBuzz()
        {  
            if (Connection.ConnectionAccepted)
            {
                Connection.sendMessage();
            }
            else
            {
                MessageBox.Show("Could not send BUZZ: Not connected to user.");
            }
        }
        public void listen()
        {
            ListenStatus = "";
            ListenStatus = (UserName == "") ? "No name entered\n" : "";
            ListenStatus += (ListenPort == "") ? "No port entered" : "";
            
            if(Connection.ConnectionAccepted)
            {
                MessageBox.Show("Please disconnect first");
                return;
            }

            if(UserName != "" && ListenPort != "")
            {
                
                if ((Int32.Parse(ListenPort) > 0) && (Int32.Parse(ListenPort) < (65536)) && Connection.listen(ListenPort, UserName))
                {
                    ListenStatus = "Listening on PORT: \n" + ListenPort;
                    ListeningStatusColor = "Green";
                }
                else
                {
                    ListenStatus = "Invalid PORT";
                }
            }
        }

        public void requestConnection()
        {
            ConnectionStatus = "sending";
            Connection.requestConnection(ConnectIP, ConnectPort);
        }

        public void Disconnect()
        {    
            if(Connection.Disconnect())
                ConnectionStatus = "disconnecting";
        }
        public void EventFromModel(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MessageRecieved")
            { 
                if(Connection.MessageRecieved.type == MessageType.Buzz)
                {
                    SystemSounds.Beep.Play();
                    return;
                }             
                MessageList.Add(new MessageContent()
                {
                    msg = Connection.MessageRecieved.message,
                    sender = Connection.MessageRecieved.sender,
                    date = " - [" + Connection.MessageRecieved.date + "]: ",
                    boxColor = "#9DAAF2"
                });
            }
            if (e.PropertyName == "Disconnection")
            {
                if(Connection.Disconnection)
                {
                    HistoryHandler.AddHistory(MessageList, UserName, DateTime.Now.ToString("s"));
                    MessageList.Clear();
                    ConnectionStatus = "Disconnected";
                    ConnectionStatusColor = "Red";
                }
            }

            if (e.PropertyName == "ConnectionAccepted")
            {
                if(Connection.ConnectionAccepted)
                { 
                    ConnectionStatus = "Connected";
                    ConnectionStatusColor = "Green";
                    Connection.receiveMessages();
                }
                else
                {
                    MessageBox.Show("The user denied your request :(");
                    ConnectionStatus = "Request denied";
                }
            }
            if (e.PropertyName == "IncomingRequest")
            {
                //Fråga användaren om en connection
                MessageBoxResult msgResult = MessageBox.Show(Connection.IncomingRequest.sender + " want to chat with you!!", "Connection Request", MessageBoxButton.YesNo);
                if (msgResult == MessageBoxResult.Yes)
                {
                    Connection.sendResponse(true);
                    Connection.receiveMessages();
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
        public void ToggleHistory()
        {
            ConversationList.Clear();
            if (ShowHistory == "Hidden")
            {
                ShowHistory = "Visible";
                List<string> list = HistoryHandler.getConversations();
                foreach (string s in list)
                {
                    ConversationList.Add(new HistoryData
                    {
                        name = s,
                        command = ShowConversationCommand
                    });
                }
            }
                
            else
                ShowHistory = "Hidden";

        }
        public void SearchHistory()
        {
            ConversationList.Clear();
            List<string> list = HistoryHandler.getConversations(SearchTerm);
           
            foreach (string s in list)
            {
                ConversationList.Add(new HistoryData
                {
                    name = s,
                    command = ShowConversationCommand
                });
            }
        }
        public void ShowConversation(string filename)
        {
            HistoryConversation.Clear();
            List<MessageContent> list = HistoryHandler.getConversation(filename);
            foreach (MessageContent msg in list)
            {
                HistoryConversation.Add(msg);
            }

        }
    }

    public class MessageContent
    {
        public String msg { get; set; }
        public String sender { get; set; }
        public String date { get; set; }
        public String boxColor { get; set; }
    }
    public class HistoryData
    {
        public String name { get; set; }
        public ICommand command { get; set; }
    }
}
