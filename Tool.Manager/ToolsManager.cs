using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool.Manager.GUI;
using Tool.Manager.Interfaces;

namespace Tool.Manager
{
    public static class ToolsManager
    {
        private static List<ITool> _tools;

        public static IConfigurationRoot Config;
        public static ToolsSettings Settings;

        private static bool Runnig = true;

        private static Tab ActiveTab;
        private static List<string> ConsoleLines = new List<string>();

        private static ITool ActiveTool;

        public static Menu Menu;
        public static Table Table;
        public static int TablePage;

        public static MenuItem SelectedMenuItem;
        public static TableRow SelectedTalbeRow;

        private static List<Documentation> Documentation { get; set; } = new List<Documentation>();

        // Public interface

        public static void Init(params ITool[] tools)
        {
            if (!File.Exists("appsettings.json")) throw new Exception("Please make sure there is a configurationfile called appsettings.json with all the basic confguration needed.");

            if (File.Exists("documentation.json"))
            {
                var json = File.ReadAllText("documentation.json");
                Documentation = JsonConvert.DeserializeObject<Dictionary<string,List<Documentation>>>(json)["help"];
            }

            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Settings = new ToolsSettings()
            {
                Title = Config["application:name"],
                Description = Config["application:description"],
                Version = Config["application:version"]
            };

            _tools ??= new List<ITool>();
            _tools.AddRange(tools);
        }

        public static void Run()
        {
            var menu = new Menu()
            {
                Title = "Available Tools",
                Items = _tools.Select(x => new MenuItem()
                {
                    Description = x.Description,
                    Text = x.Name,
                    Value = x.Id,
                }).ToList()
            };

            Display.Init(Config.GetSection("display"));
            Display.DrawTop(Settings);

            ActivateTab(Tab.Menu);

            while (true)
            {
                RunMenu(menu);
            }
        }

        public static void RunMenu(Menu menu)
        {
            SelectedMenuItem = null;
            Display.ClearMenu();

            Menu = menu;
            Display.DrawMenu(Menu);

            while (Runnig)
            {
                // This is to make sure we unload the tool when hitting the base menu.
                if (Menu.Title == "Available Tools") ActiveTool = null;

                if (ActiveTab == Tab.Console)
                {
                    Display.DrawConsole(ConsoleLines);
                    SetConsoleCurrsor(); 

                    if (ProcessCommand(Console.ReadLine()))
                    {
                        continue;
                    }
                    else
                    {
                        Back();
                        return;
                    }
                }

                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    // Commands
                    case ConsoleKey.Escape:
                        Back();
                        return;
                    case ConsoleKey.Enter:
                        if (ActiveTab == Tab.Menu)
                            if (SelectedMenuItem is object)
                            {
                                if (MenuExecute(SelectedMenuItem))
                                {
                                    Menu = menu;
                                    SelectedMenuItem = null;
                                    Display.DrawMenu(Menu);
                                }
                            }
                        if (ActiveTab == Tab.Data)
                            if (SelectedTalbeRow is object)
                            {
                                TableRowClick(SelectedTalbeRow);
                            }
                        if (ActiveTab == Tab.Top)
                            Display.DrawInfo(Settings.Info);

                        break;

                    //Menu Interaction
                    case ConsoleKey.UpArrow:
                        if (ActiveTab == Tab.Menu)
                            MenuMove(Direction.Up);
                        if (ActiveTab == Tab.Data)
                            DataMove(Direction.Up);
                        break;
                    case ConsoleKey.DownArrow:
                        if (ActiveTab == Tab.Menu)
                            MenuMove(Direction.Down);
                        if (ActiveTab == Tab.Data)
                            DataMove(Direction.Down);
                        break;

                    case ConsoleKey.PageUp:
                        if (ActiveTab == Tab.Data)
                            DataScroll(Direction.Up);
                        break;
                    case ConsoleKey.PageDown:

                        if (ActiveTab == Tab.Data)
                            DataScroll(Direction.Down);
                        break;

                    // Tab switching
                    case ConsoleKey.RightArrow:
                        if (ActiveTab < Tab.Info)
                            ActivateTab((Tab)((int)ActiveTab + 1));
                        break;
                    case ConsoleKey.LeftArrow:
                        if (ActiveTab != Tab.Top)
                            ActivateTab((Tab)((int)ActiveTab - 1));
                        break;

                    // Move to console tab.
                    case ConsoleKey.Spacebar:
                        ActivateTab(Tab.Console);
                        SetConsoleCurrsor();
                        break;
                    default:
                        break;
                }
            }
        }

        public static void Back()
        {
            Menu = null;
            Table = null;
            Display.ClearInfo();
            Display.ClearTable();
            SelectedMenuItem = null;
            SelectedTalbeRow = null;
        }

        public static bool MenuExecute(MenuItem item)
        {
            try
            {
                if (ActiveTool is object)
                {

                    if (ActiveTool.ExecuteMenuItem(item).Result)
                        return true;
                }

                foreach (var tool in _tools)
                {
                    if (item.Value == tool.Id)
                    {
                        ActiveTool = tool;
                        tool.Init();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return false;
        }

        public static void TableRowClick(TableRow row)
        {
            if (ActiveTool is object) ActiveTool.RowClick(row, Table);
        }

        public static void LoadTable(Table table)
        {
            try
            {
                Table = table;
                TablePage = 0;

                SelectedTalbeRow = null;

                Display.DrawTable(table, TablePage);
                if (ActiveTab == Tab.Console)
                {
                    SetConsoleCurrsor();
                }
                else
                {
                    ActivateTab(Tab.Data);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public static void Log(string text)
        {
            ConsoleLines.Add(text);
            Display.DrawConsole(ConsoleLines);
            if (ActiveTab == Tab.Console)
            {
                SetConsoleCurrsor();
            }
        }

        public static void Help()
        {
            var documentation = Documentation;
            if (ActiveTool?.Documentation is object)
            {
                documentation.AddRange(ActiveTool.Documentation);
            }

            var grouped = documentation.GroupBy(x => x.Group);
            var table = new Table()
            {
                Title = "Help section",
                Columns = new List<string>() { "Name", "Description" },
                Rows = new List<TableRow>()
            };
            foreach (var group in grouped)
            {
                table.Rows.Add(new TableRow() { Id = group.Key, GroupHeader = group.Key, Info = new Dictionary<string, string>() { { "Group name", group.Key }, { "Description", "This is a section header for grouping together help entries." } } });
                table.Rows.AddRange(group.Select(x =>
                new TableRow() { 
                    Id = group.Key, 
                    Values = new List<string>() {  x.Key, x.Value }, Info = new Dictionary<string, string>() { { "Name", x.Key }, { "Description", x.Value }
                    } }));
            }

            LoadTable(table);
            ActivateTab(Tab.Data);
        }

        // Private 

        private enum Direction { Up, Down }

        private static void MenuMove(Direction direction)
        {
            var oldSelection = SelectedMenuItem;

            if (SelectedMenuItem is null)
            {
                SelectedMenuItem = Menu.Items.First();

            }
            else if (direction == Direction.Up)
            {
                if (SelectedMenuItem == Menu.Items.First()) return;
                SelectedMenuItem = Menu.Items[Menu.Items.IndexOf(oldSelection) - 1];
            }
            else if (direction == Direction.Down)
            {
                if (SelectedMenuItem == Menu.Items.Last()) return;
                SelectedMenuItem = Menu.Items[Menu.Items.IndexOf(oldSelection) + 1];
            }

            if (oldSelection != null) Display.MenuSelect(oldSelection, Menu.Items.IndexOf(oldSelection), true);
            Display.MenuSelect(SelectedMenuItem, Menu.Items.IndexOf(SelectedMenuItem));
        }

        private static void DataMove(Direction direction)
        {
            if (Table is null) return;

            var oldSelection = SelectedTalbeRow;

            var maxRows = Display.GetMaxTableRows();

            if (SelectedTalbeRow is null)
            {
                SelectedTalbeRow = Table.Rows[maxRows * TablePage];

            }
            else if (direction == Direction.Up)
            {
                if (SelectedTalbeRow == Table.Rows.First()) return;

                var index = Table.Rows.IndexOf(oldSelection) - 1;

                if (index < TablePage * maxRows)
                {
                    TablePage--;
                    oldSelection = null;
                    Display.DrawTable(Table, TablePage);
                }

                SelectedTalbeRow = Table.Rows[index];
            }
            else if (direction == Direction.Down)
            {
                if (SelectedTalbeRow == Table.Rows.Last()) return;

                var index = Table.Rows.IndexOf(oldSelection) + 1;
                if (index > (TablePage + 1) * maxRows)
                {
                    TablePage++;
                    oldSelection = null;
                    Display.DrawTable(Table, TablePage);
                }

                SelectedTalbeRow = Table.Rows[index];
            }

            if (oldSelection != null) Display.RowSelect(oldSelection, Table.Rows.IndexOf(oldSelection) - (TablePage * maxRows), true);
            Display.RowSelect(SelectedTalbeRow, Table.Rows.IndexOf(SelectedTalbeRow) - (TablePage * maxRows));
        }

        private static void DataScroll(Direction direction)
        {
            if (direction == Direction.Up && TablePage > 0)
            {
                TablePage--;
                SelectedTalbeRow = null;
                Display.DrawTable(Table, TablePage);
            }

            if (direction == Direction.Down && TablePage < Convert.ToInt32(Table.Rows.Count / Display.GetMaxTableRows()))
            {
                TablePage++;
                SelectedTalbeRow = null;
                Display.DrawTable(Table, TablePage);
            }
        }

        private static bool ProcessCommand(string command)
        {

            if (string.IsNullOrEmpty(command))
            {
                ActivateTab(Tab.Menu);
                return true;
            }

            ConsoleLines.Add(">> " + command);
            
            if (command.ToLower() == "help")
            {
                Help();
                return true;
            }

            if (command.ToLower() == "back" || command.ToLower() == "quit")
            {
                Back();
                Display.DrawConsole(ConsoleLines);
                SetConsoleCurrsor();
                return false;
            }


            if (ActiveTool is object)
            {
                try
                {
                    if (ActiveTool.ExecuteCommand(command).Result)
                        return true;
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }

            var menuItem = Menu.Items.FirstOrDefault(x => x.Text.ToLower().StartsWith(command.ToLower()));
            if (menuItem is object)
            {
                ConsoleLines.Add("Executing: [" + menuItem.Text + "]");
                if (MenuExecute(menuItem))
                {
                    Display.DrawConsole(ConsoleLines);
                    SetConsoleCurrsor();
                    return true;
                }
            }

            

            ConsoleLines.Add("!Could not recognize command! Try again");

            Display.DrawConsole(ConsoleLines);
            SetConsoleCurrsor();

            return true;
        }

        private static void ActivateTab(Tab tab)
        {
            if (ActiveTab == tab) return;

            Display.Highlight(ActiveTab, false);
            ActiveTab = tab;
            Display.Highlight(tab, true);
        }

        public static string PromtInput(string promptMessage)
        {
            var recentTab = ActiveTab;

            ActivateTab(Tab.Console);

            ConsoleLines.Add("?" + promptMessage);
            Display.DrawConsole(ConsoleLines);

            SetConsoleCurrsor();
            var response = Console.ReadLine();
            ConsoleLines.Add(">> " + response);
            Display.DrawConsole(ConsoleLines);

            ActivateTab(recentTab);

            return response;

        }

        private static void SetConsoleCurrsor()
        {
            var pos = Display.GetConsolePosition();
            Console.SetCursorPosition(pos.X, pos.Y);
            Console.Write(">> ");
        }

        private static void HandleException(Exception ex)
        {
            ConsoleLines.Add("!" + ex.Message);
            Display.DrawConsole(ConsoleLines);
        }


    }
}
