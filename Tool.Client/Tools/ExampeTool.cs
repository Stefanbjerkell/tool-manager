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
            Actions = new List<ToolsAction>()
            {
                new ToolsAction("Table", "table", Table, "Draws paginated table that you can scroll through"),
                new ToolsAction("Draw Top", "top", DrawTop, "Shows some data up in the header section"),
                new ToolsAction("Config", "config", Config, "This will load a new configruation file").AddOption("config", "f", true, "Please select a file for the new configuration")
            };
            Documentation = new List<Documentation>()
            {
                new Documentation()
                {
                    Group = "Example Tool",
                    Value = "This is a help entry from the example tool",
                    Key = "Example Docutmentation"
                }
            };
        }

        public async Task InfoClick(KeyValuePair<string, string> value)
        {
            ToolsManager.Log("Info [" + value.Key + "] + [" + value.Value + "]");
        }

        public void Configure(IConfigurationRoot config)
        {
            _config = config;
        }

        public async Task RowClick(TableRow row, Table table)
        {
            ToolsManager.Log("[s]Row [" + row.Id + "] Clicked!");
        }

        // Actions

        private static async Task TestAction(Dictionary<string, string> options)
        {
            options.TryGetValue("file", out string value);
            options.TryGetValue("message", out string message);
            options.TryGetValue("text", out string text);

            ToolsManager.Log($"Value: {value} | Text: {text} | Message: {message}");
        }

        private static async Task Table(Dictionary<string, string> options)
        {
            var table = new Table()
            {
                Columns = new List<string>() { "Name", "Value", "Description", "Date" },
            };

            for (int i = 0; i < 100; i++)
                table.Rows.Add(new TableRow()
                {
                    Id = i.ToString(),
                    GroupHeader = i % 10 == 0 ? "Section" : null,
                    Color = i % 10 == 0 ? ConsoleColor.DarkCyan : i % 2 == 0 ? ConsoleColor.DarkYellow : ConsoleColor.Gray,
                    Info = new Dictionary<string, string>() { { "Name", "Row: " + i }, { "Value", "Value " + i }, { "Extra data", "Some additional data for row " + i } },
                    Values = new List<string>() { "Row" + i, i.ToString(), "use arrows of page up or page down to scroll", DateTime.Now.AddDays(-i).ToShortDateString() }
                });

            ToolsManager.LoadTable(table);
        }

        private static async Task DrawTop(Dictionary<string, string> options)
        {
            ToolsManager.Settings.AddInfo("Persistant info", "Is shown here");
            ToolsManager.Settings.AddInfo("Other info", "Some other value");
            ToolsManager.Settings.AddInfo("Last line", "Additiona lines will be hidden");
            ToolsManager.Settings.AddInfo("Hidden info", "This wont show unless top height is increased");

            Display.DrawTop(ToolsManager.Settings);
        }

        private static async Task Config(Dictionary<string, string> options)
        {
            options.TryGetValue("config", out string file);
            ToolsManager.Log($"[s]Loading: {file}");
            var config = new ConfigurationBuilder().AddJsonFile(file).Build();
            ToolsManager.SetConfiguration(config);
        }
    }
}
