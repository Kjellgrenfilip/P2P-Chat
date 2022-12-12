using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using WpfApp1.ViewModels;
using Newtonsoft.Json;
using System.Windows;


namespace WpfApp1.Models { 
    public class HistoryHandler
    {
        private string mainPath = AppDomain.CurrentDomain.BaseDirectory + @"history\";
        public HistoryHandler()
        {
            
        }

        public void AddHistory(ObservableCollection<MessageTest> list, String name, String date)
        {
            string path = mainPath + date.Replace(':', '.') + name + ".json";

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (MessageTest msg in list)
                    {
                        string jsonString = JsonConvert.SerializeObject(msg);
                        
                        sw.WriteLine(jsonString);
                    }
                }
            }
            else
            {
                path = mainPath + date.Replace(':', '.') + name +"(1)" + ".json";
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (MessageTest msg in list)
                    {
                        string jsonString = JsonConvert.SerializeObject(msg);
                       
                        sw.WriteLine(jsonString);
                    }
                }
            }
        }
        public List<string> getConversations(string searchTerm = null)
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(mainPath);
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            List<string> list = new List<string>();

            if (searchTerm != null)
            {
                var tmplist =
                from file in fileList
                where file.Name.Contains(searchTerm)
                orderby file.Name ascending
                select file;

                foreach (var f in tmplist)
                {
                    list.Add(f.Name);
                }
            }
            else
            {
                var tmplist =
                from file in fileList
                orderby file.Name ascending
                select file;

                foreach (var f in tmplist)
                {
                    list.Add(f.Name);
                }
            }
            
            return list;
        }
        public List<MessageTest> getConversation(string filename)
        {
            List<MessageTest> list = new List<MessageTest>();

            string path = mainPath + filename;

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    MessageTest? jsonObject = JsonConvert.DeserializeObject<MessageTest>(line);
                    list.Add(jsonObject);
                }
            }
            return list;
        }
    }
}
