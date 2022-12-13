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
        //Create the path to the folder in which we store the conversations.
        private string mainPath = AppDomain.CurrentDomain.BaseDirectory + @"history\";
        public HistoryHandler()
        {
            
        }
        public void AddHistory(ObservableCollection<MessageContent> list, String name, String date)
        {
            //Creates path to the conversation file that we want to write to. 
            string path = mainPath + date.Replace(':', '.') + name + ".json";

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (MessageContent msg in list)
                    {
                        string jsonString = JsonConvert.SerializeObject(msg);
                        
                        sw.WriteLine(jsonString);
                    }
                }
            }
            else
            {
                //If the file-name already exists we add a "(1)" to separate them.
                path = mainPath + date.Replace(':', '.') + name +"(1)" + ".json";
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (MessageContent msg in list)
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
            //Creates a list of all files in the history directory
            IEnumerable<System.IO.FileInfo> fileList = dir.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            List<string> list = new List<string>();

            //If a searchterm is entered.
            if (searchTerm != null)
            {
                //Selects all files containing the searchterm and sorts them in ascending order based on the date.
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
            //If no searchterm is entered
            else
            {
                //Selects all files and sorts them in ascending order based on date.
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
        public List<MessageContent> getConversation(string filename)
        {
            List<MessageContent> list = new List<MessageContent>();

            string path = mainPath + filename;

            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    MessageContent? jsonObject = JsonConvert.DeserializeObject<MessageContent>(line);
                    list.Add(jsonObject);
                }
            }
            return list;
        }
    }
}
