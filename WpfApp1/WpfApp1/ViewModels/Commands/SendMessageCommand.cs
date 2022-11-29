using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1.ViewModels.Commands
{
    internal class SendMessageCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private MainViewModel _parent;

        public MainViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public SendMessageCommand(MainViewModel parent)
        {
            this.Parent = parent;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.sendMessage();
        }
    }
}

namespace WpfApp1.ViewModels.Commands
{
    internal class StartListeningCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private MainViewModel _parent;

        public MainViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public StartListeningCommand(MainViewModel parent)
        {
            this.Parent = parent;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.listen();
        }
    }
}


namespace WpfApp1.ViewModels.Commands
{
    internal class RequestConnectionCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private MainViewModel _parent;

        public MainViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public RequestConnectionCommand(MainViewModel parent)
        {
            this.Parent = parent;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.requestConnection();
        }
    }
}
namespace WpfApp1.ViewModels.Commands
{
    internal class RequestDisconnectionCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private MainViewModel _parent;

        public MainViewModel Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public RequestDisconnectionCommand(MainViewModel parent)
        {
            this.Parent = parent;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            Parent.requestDisconnection();
        }
    }
}