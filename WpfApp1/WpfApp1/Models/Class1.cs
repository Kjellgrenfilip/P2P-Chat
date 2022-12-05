using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using WpfApp1.ViewModels;

namespace WpfApp1.Models { 
    public class HistoryHandler
    {
        public HistoryHandler()
        {
            string path = @"X:\TDDD49\MyTest.txt";
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Hello");
                    sw.WriteLine("And");
                    sw.WriteLine("Welcome");
                }
            }
        }

        public void AddHistory(ObservableCollection<MessageTest> list, String name, String date)
        {
            string path = @"X:\TDDD49\" + name + date + ".txt";
            if(!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    for_each(list obj)
                        append(converted_to_json(obj));


                    sw.WriteLine("Hello");
                    sw.WriteLine("And");
                    sw.WriteLine("Welcome");
                }
            }
        }
        public void ConvertToJson()
        {

        }
        public void getHistory()
        {

        }
    }
}
