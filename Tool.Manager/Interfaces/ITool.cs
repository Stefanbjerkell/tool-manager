using Microsoft.Extensions.Configuration;
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

        List<ToolsAction> Actions { get; set; }

        Task Run();

        void Configure(IConfigurationRoot config);

        Task<bool> ExecuteCommand(string command);

        Task<bool> ExecuteMenuItem(MenuItem item);

        Task RowClick(TableRow row, Table table);        

    }

    public delegate Task ToolsActionMethod(Dictionary<string, string> options);

    public class ToolsAction
    {
        public ToolsAction(string name, string command, ToolsActionMethod action, string description = "")
        {
            Name = name;
            Command = command;
            Action = action;
            Description = description;
        }

        public ToolsAction AddOption(string name, string shortName = "", bool required = false, string promt = null)
        {
            Options.Add(new ToolsActionOption()
            {
                Option = name,
                Short = shortName,
                Required = required,
                Promt = promt
            });

            return this;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Command { get; set; }

        public ToolsActionMethod Action { get; set; }

        public List<ToolsActionOption> Options { get; set; } = new List<ToolsActionOption>();
    }

    public class ToolsActionOption
    {
        public string Option { get; set; }

        public string Short { get; set; }

        public bool Required { get; set; }

        public string Promt { get; set; }
    }

    public class ToolsGlobalSettings
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
