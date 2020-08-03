using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tool.Manager.GUI;
using Tool.Manager.Interfaces;
using Microsoft.Extensions.DependencyInjection;

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

        private static string TableFilter = "";

        private static List<Documentation> Documentation { get; set; } = new List<Documentation>();

        // Public interface

        /// <summary>
        /// This needs to be run before starting up the manager.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="config"></param>
        public static void Configure(IServiceProvider serviceProvider, IConfigurationRoot config)
        {
            Settings = new ToolsSettings();
            if (File.Exists("documentation.json"))
            {
                var json = File.ReadAllText("documentation.json");
                Documentation = JsonConvert.DeserializeObject<Dictionary<string, List<Documentation>>>(json)["help"];
            }
            _tools ??= new List<ITool>();
            _tools.AddRange(serviceProvider.GetServices<ITool>());

            SetConfiguration(config);
        }

        /// <summary>
        /// Start the tool Manager. Allowing the user to select a tool to use.
        /// </summary>
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

            menu.Items.Add(new MenuItem()
            {
                Type = MenuItemType.Divider
            });

            menu.Items.Add(new MenuItem()
            {
                Color = ConsoleColor.DarkCyan,
                Text = "Help",
                Description = "Show help section.",
                Value = "HELP"
            });

            menu.Items.Add(new MenuItem()
            {
                Color = ConsoleColor.DarkCyan,
                Text = "Quit",
                Description = "Close application.",
                Value = "QUIT"
            });


            Display.DrawTop(Settings);

            ActivateTab(Tab.Menu);

            while (Runnig)
            {
                RunMenu(menu);
            }
        }

        /// <summary>
        /// Override current configuration. This will re-configure all available ITool instances.
        /// </summary>
        /// <param name="config"></param>
        public static void SetConfiguration(IConfigurationRoot config)
        {
            Config = config;
            Settings.Title = Config["application:name"] ?? "Title (set in config [application:name])";
            Settings.Description = Config["application:description"] ?? "Description (set in config [application:description])";
            Settings.Version = Config["application:version"] ?? "Version (set in config [application:version])";

            Display.Init(config.GetSection("display"));
            Display.DrawTop(Settings);

            foreach (var tool in _tools)
            {
                tool.Configure(config);
            }
        }

        /// <summary>
        /// Put program in User input mode. Allowing user to browse menues and entering commands. 
        /// </summary>
        /// <param name="menu"></param>
        public static void RunMenu(Menu menu)
        {
            if (menu is null || !menu.Items.Any(x => x.Type == MenuItemType.Item)) return;

            SelectedMenuItem = null;
            Display.ClearMenu();

            if (ActiveTool is object)
            {
                menu.Items.Add(new MenuItem()
                {
                    Type = MenuItemType.Divider
                });

                menu.Items.Add(new MenuItem()
                {
                    Color = ConsoleColor.DarkCyan,
                    Text = "Help",
                    Description = "Show help section.",
                    Value = "HELP"
                });

                menu.Items.Add(new MenuItem()
                {
                    Color = ConsoleColor.DarkCyan,
                    Text = "Back",
                    Description = "Close this menu and go back to previous one.",
                    Value = "BACK"
                });
            }

            Menu = menu;
            Display.DrawMenu(Menu);
            SelectedMenuItem = Menu.Items.First(x => x.Type == MenuItemType.Item);
            Display.MenuSelect(SelectedMenuItem, Menu.Items.IndexOf(SelectedMenuItem));


            while (true)
            {
                // This is to make sure we unload the tool when hitting the base menu.
                if (Menu?.Title == "Available Tools") ActiveTool = null;

                if (menu is object && SelectedMenuItem is null)
                {
                    SelectedMenuItem = Menu.Items.First(x => x.Type == MenuItemType.Item);
                    Display.MenuSelect(SelectedMenuItem, Menu.Items.IndexOf(SelectedMenuItem));
                }


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
                                if (SelectedMenuItem.Value == "BACK")
                                {
                                    Back();
                                    return;
                                }

                                if (SelectedMenuItem.Value == "HELP")
                                {
                                    Help();
                                    continue;
                                }

                                if (SelectedMenuItem.Value == "QUIT")
                                {
                                    Runnig = false;
                                    Back();
                                    return;
                                }

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
                        {
                            var info = new List<InfoRow>();
                            info.Add(new InfoRow("Global Data", "Global Data", InfoRowType.Title));

                            if (Settings.Info is object)
                            {

                                info.Add(new InfoRow("Information", "Information", InfoRowType.Section));
                                foreach (var item in Settings.Info)
                                {
                                    info.Add(new InfoRow(item.Key, item.Value));
                                }
                            }
                            if (Settings.Data is object)
                            {
                                info.Add(new InfoRow("Data", "Stored Data", InfoRowType.Section));
                                foreach (var item in Settings.Data)
                                {
                                    info.Add(new InfoRow(item.Key, item.Value.ToString()));
                                }
                            }

                            Display.DrawInfo(info);
                        }

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
                        if (ActiveTab == Tab.Data)
                            QuickSelect(key);
                        break;
                }
            }
        }

        /// <summary>
        /// Jump to first table row matching typed text.
        /// </summary>
        /// <param name="key"></param>
        private static void QuickSelect(ConsoleKeyInfo key)
        {
            if (char.IsLetterOrDigit(key.KeyChar) && Table != null)
            {
                TableFilter += key.KeyChar;
                
                var row = Table.Rows.FirstOrDefault(x => string.IsNullOrEmpty(x.GroupHeader) && x.Values.Any(r => r.ToLower().Contains(TableFilter.ToLower())));

                if (row is object)
                {
                    var maxRows = Display.GetMaxTableRows();
                    var index = Table.Rows.IndexOf(row);
                    var pageOfRow = index / maxRows;

                    if(pageOfRow == TablePage)
                    {
                        var oldSelection = SelectedTalbeRow;
                        if (oldSelection is object && pageOfRow == TablePage)
                            Display.RowSelect(oldSelection, Table.Rows.IndexOf(oldSelection), true);
                        SelectedTalbeRow = row;
                    }
                    else
                    {
                        SelectedTalbeRow = row;
                        TablePage = pageOfRow;
                        Display.DrawTable(Table, TablePage);
                    }                    
                    Display.RowSelect(SelectedTalbeRow, index - (maxRows * TablePage));
                }
            }
        }

        /// <summary>
        /// Use this to update menu and redraw it.
        /// </summary>
        /// <param name="menu"></param>
        public static void UpdateMenu(Menu menu)
        {
            Menu = menu;
            Display.DrawMenu(Menu);
        }

        /// <summary>
        /// Close current menu and jump back the the previous one.
        /// </summary>
        public static void Back()
        {
            Menu = null;
            Table = null;
            Display.ClearInfo();
            Display.ClearTable();
            SelectedMenuItem = null;
            SelectedTalbeRow = null;
        }

        /// <summary>
        /// Loads a table into the data view.
        /// </summary>
        /// <param name="table"></param>
        public static void LoadTable(Table table)
        {
            try
            {
                TableFilter = "";
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

        /// <summary>
        /// Writes a line to Console.
        /// </summary>
        /// <param name="text"></param>
        public static void Log(string text)
        {
            ConsoleLines.Add(text);
            Display.DrawConsole(ConsoleLines);
            if (ActiveTab == Tab.Console)
            {
                SetConsoleCurrsor();
            }
        }

        /// <summary>
        /// Shows help menu.
        /// </summary>
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
                new TableRow()
                {
                    Id = group.Key,
                    Values = new List<string>() { x.Key, x.Value },
                    Info = new Dictionary<string, string>() { { "Name", x.Key }, { "Description", x.Value }
                    }
                }));
            }

            LoadTable(table);
            ActivateTab(Tab.Data);
        }

        /// <summary>
        /// Write stuff in Info tab.
        /// </summary>
        /// <param name="info"></param>
        public static void ShowInfo(Dictionary<string, string> info)
        {
            Display.DrawInfo(info);
        }

        /// <summary>
        /// Ask user for input. 
        /// </summary>
        /// <param name="promptMessage"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Ask the user a yes or no question.
        /// </summary>
        /// <param name="promtMessge"></param>
        /// <returns></returns>
        public static bool PromtYesOrNo(string promtMessge)
        {
            var answer = PromtInput(promtMessge);
            while (true)
            {

                if (answer.ToLower().Contains("yes") || answer.ToLower() == "y")
                {
                    return true;
                }
                if (answer.ToLower().Contains("no") || answer.ToLower() == "n")
                {
                    return false;
                }
                answer = PromtInput(promtMessge + " (Yes/No)");
            }
        }

        // Private 
        private static bool MenuExecute(MenuItem item)
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
                        tool.Run();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return true;
            }

            return false;
        }

        private static void TableRowClick(TableRow row)
        {
            TableFilter = "";
            if (ActiveTool is object) ActiveTool.RowClick(row, Table);
        }

        private enum Direction { Up, Down }

        private static void MenuMove(Direction direction)
        {
            var oldSelection = SelectedMenuItem;

            if (direction == Direction.Up)
            {
                if (SelectedMenuItem == Menu.Items.First()) return;
                SelectedMenuItem = Menu.Items[Menu.Items.IndexOf(oldSelection) - 1];
            }
            else if (direction == Direction.Down)
            {
                if (SelectedMenuItem == Menu.Items.Last()) return;
                SelectedMenuItem = Menu.Items[Menu.Items.IndexOf(oldSelection) + 1];
            }

            if (SelectedMenuItem.Type == MenuItemType.Divider || SelectedMenuItem.Type == MenuItemType.Headline)
            {
                MenuMove(direction);
                if (SelectedMenuItem.Type == MenuItemType.Divider)
                {
                    SelectedMenuItem = oldSelection;
                    return;
                }
            }

            if (oldSelection != null) Display.MenuSelect(oldSelection, Menu.Items.IndexOf(oldSelection), true);
            if (oldSelection != null && oldSelection.Type == MenuItemType.Item) Display.MenuSelect(SelectedMenuItem, Menu.Items.IndexOf(SelectedMenuItem));
        }

        private static void DataMove(Direction direction)
        {
            TableFilter = "";
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
            TableFilter = "";
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

            if (command.ToLower() == "draw" || command.ToLower() == "redraw")
            {
                Display.Init(Config);
                Display.Highlight(ActiveTab);
                return true;
            }

            if (command.ToLower() == "back" || command.ToLower() == "..")
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

            var menuItem = Menu.Items.FirstOrDefault(x => x.Type == MenuItemType.Item && (x.Text.ToLower().StartsWith(command.ToLower()) || x.Value.ToLower().StartsWith(command.ToLower())));
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
