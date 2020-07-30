using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tool.Manager;
using Tool.Manager.GUI;
using Tool.Manager.Interfaces;
using Tool.Manager.Tools;

namespace Tool.Client.Tools
{
    public class ExampeTool : ToolBase, ITool
    {
        private IConfiguration _config;

        public ExampeTool() : base("Example tool", "Tool to show of some of the features supported", "TEST")
        {

        }

        public List<Documentation> Documentation  { get; set; } = new List<Documentation>()
        {
            new Documentation()
            {
                Group = "Example Group",
                Value = "This is a help entry from the example tool",
                Key = "Example Docutmentation"
            }
        };

        public async Task<bool> ExecuteCommand(string command)
        {
            ToolsManager.Log($"{Name} cannot execute command [{command}]");
            return false;
        }

        public async Task<bool> ExecuteMenuItem(MenuItem item)
        {
            switch (item.Value)
            {
                case "TABLE":

                    var table = new Table()
                    {
                        Columns = new List<string>() { "Name", "Value", "Description", "Date" },
                    };

                    for (int i = 0; i < 100; i++)
                        table.Rows.Add(new TableRow() { 
                            Id = i.ToString(), 
                            GroupHeader = i % 10 == 0 ? "Section" : null, 
                            Color = i % 10 == 0 ? ConsoleColor.DarkCyan : i % 2 == 0 ? ConsoleColor.DarkYellow : ConsoleColor.Gray,  
                            Info = new Dictionary<string, string>() { { "Name", "Row: " + i}, { "Value", "Value " + i }, { "Extra data", "Some additional data for row " + i } }, 
                            Values = new List<string>() { "Row" + i,  i.ToString(), "use arrows of page up or page down to scroll",  DateTime.Now.AddDays(-i).ToShortDateString()  } } );

                    ToolsManager.LoadTable(table);

                    return true;
                case "TOP":

                    ToolsManager.Settings.AddInfo("Persistant info", "Is shown here");
                    ToolsManager.Settings.AddInfo("Other info", "Some other value");
                    ToolsManager.Settings.AddInfo("Last line", "Additiona lines will be hidden");
                    ToolsManager.Settings.AddInfo("Hidden info", "This wont show unless top height is increased");

                    Display.DrawTop(ToolsManager.Settings);

                    return true;
                case "CONFIG":
                    var file = ToolsManager.PromtInput("Select a new config file (json) to use");
                    var config = new ConfigurationBuilder().AddJsonFile(file).Build();
                    ToolsManager.SetConfiguration(config);
                    return true;
                default:
                    return false;
            }
        }

        public async Task InfoClick(KeyValuePair<string, string> value)
        {
            ToolsManager.Log("Info [" + value.Key + "] + [" + value.Value + "]");
        }

        public void Configure(IConfigurationRoot config)
        {
            _config = config;
        }

        public async Task Run()
        {
            ToolsManager.Log("Starting [" + Name + "]");

            ToolsManager.RunMenu(new Menu()
            {
                Title = "Example Commands",
                Items = new List<MenuItem>()
                {
                    new MenuItem() { Type = MenuItemType.Divider },
                    new MenuItem() { Text = "Table", Description = "Loads a dataset in to a scrollabel table.", Value = "TABLE" },
                    new MenuItem() { Text = "Top info", Description = "Show information in Top bar", Value = "TOP"},
                    new MenuItem() { Text = "Change configuration", Description = "Change the configration file to use other settings", Value = "CONFIG"}
                }
            });
        }

        public async Task RowClick(TableRow row, Table table)
        {
            ToolsManager.Log("[s]Row [" + row.Id + "] Clicked!");
        }
    }
}
