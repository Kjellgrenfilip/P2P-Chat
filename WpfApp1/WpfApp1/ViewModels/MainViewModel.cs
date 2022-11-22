using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.ViewModels.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
            this.PushCommand = new SendMessageCommand(this);
            this.Listen = new StartListeningCommand(this);
            this.ConnectCommand = new RequestConnectionCommand(this);
        }
        public void sendMessage()
        {
            Connection.sendMessage(MessageToSend);
        }
        public void listen()
        {
            
            if (Connection.listen(ListenPort, new AsyncCallback(onAcceptResult)))
            {
                ListenOK = "Listening on PORT: " + ListenPort;
                
            }
                
            else
                ListenOK = "NOT NICE WORK MAN";

        }

        public void requestConnection()
        {
            RequestOK = "sending";
            Connection.requestConnection(ConnectIP, ConnectPort, ListenPort, UserName, new AsyncCallback(onRequestResult));

        }

        
        public void onRequestResult(IAsyncResult result)
        {
           if((bool)result.AsyncState)
                RequestOK = "Success!";
           else
                RequestOK = "Not Success!";
        }
        public void onAcceptResult(IAsyncResult result)
        {
            if ((bool)result.AsyncState)
                RequestOK = "Accept Success!";
            else
                RequestOK = "Not accept Success!";
        }

    }
}
