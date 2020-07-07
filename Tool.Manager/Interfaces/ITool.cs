using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tool.Manager.GUI;

namespace Tool.Manager.Interfaces
{
    public interface ITool
    {
        string Name { get; set; }

        string Id { get; set; }

        string Description { get; set; }

        List<Documentation> Documentation { get; set; }

        Task Init();

        Task<bool> ExecuteCommand(string command);

        Task<bool> ExecuteMenuItem(MenuItem item);

        Task RowClick(TableRow row, Table table);        

    }

    public class ToolsSettings
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public Dictionary<string, string> Info { get; set; }

        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public void AddInfo(string key, string value)
        {
            Info ??= new Dictionary<string, string>();

            if (!Info.ContainsKey(key))
            {
                Info.Add(key, value);
            }
            else
            {
                Info[key] = value;
            }
        }

        public void AddData(string key, object value)
        {
            if (!Data.ContainsKey(key))
            {
                Data.Add(key, value);
            }
            else
            {
                Data[key] = value;
            }
        }
    }
}
