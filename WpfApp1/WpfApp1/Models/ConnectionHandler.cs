using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1.Models
{
    public class ConnectionHandler
    {
        public void sendMessage(String message)
        {
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.
            MessageBox.Show(message);
        }
    }
}
