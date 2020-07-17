# tool-manager

Tool Manager is a helper class used to visualize your .net core console tools. You register your tools with the ITool interface and then run the Manager. It will show your tools in a nice little menu and let the user select one of them. Your tool will then run and you can use the Tool Manager to show new menues, fetch user input or display data in a scrollable table view that have support for multiple pages and row selection that you can have your tool act upon.

# Menu system.

It comes witha a menu system that you can easy use to have the user select something. The user can navigate through your menues by either typing a command or using arrow keys to move up and down.

# Tab system

The Tab system splits the sceen into 5 diffrent areas that the user can jump between.

There is the top bar that will show the name and description of your application and some optional information. 

You have a menu section where the user can navigate through your tools functions.

A data tab where you can display tables or lists with data that the user can scoll through and select rows. 

Info box for showing various information.

And lastly there is a console section where the user can type commands and you can show messages for the user.

# Installation / Usage

Download the nuget package Console.Tool.Manager from nuget.org or clone this repo. 

To get the manager up and running you first need to run the static method ToolsManager.Configure(IServiceProvider, IConfiguration) that takes a serviceCollection and a configuration file as parameters. 

After that you just call ToolsManager.Run() and it will draw out the diffrent tabs with a menu listing all your tools. 

To have your tools show up in the menu you need to register them in the serviceProvider with the interface ITool. ITool is a interface used by the ToolManager to interact with your code. The main method of your tool is the void Run() method. This will be called when the user select the tool in the menu. There are also other methods that you can use to have your tool answer to user input.

# ITool Interface

Here is a more detailed description of the ITool interface.

- void Configure(IConfigurationRoot config) - This will be called when you run the ToolsManager.Configure() method to setup configuration values. 

- void Run() - This is the entry point for your tool. This will be called when your tool is beeing selected from the "Main menu".

- async <Task>bool ExecuteMenuItem(MenuItem item) - This will be called when the user selects anything from a menu and will pass along the selected menu item. It should return true if the tool can execute the selected command or false if not.

- async <Task>bool ExecuteCommand(string command) - This will be called when the user writes a command in the command line. It should return true if the tool can execute the command and false if not.
  
- async Task RowClick(TableRow row, Table table) - This will be called if the user selects a row in a printed table along with the tableRow and the table as paramters.

- List<Documentation> Documentation - here you can provide documentation for your tool. It will be shown if the user runs the help command.

# ToolsManager

Here are the helpers that the tools manager can help out with.

- ToolsManager.RunMenu(Menu menu) - This will print a menu and let the user select something from it. It will also alow the user to enter commands and switch tabs to interact with tables and lists at the same time.

- ToolsManager.LoadTable(Table table) - This will print a table in the data section of the application. It will paginate the result if the table wont fit on screen and let the users scroll through results with arrow keys (or page up page down keys to scroll entire pages). The colums will be automatically adjusted to fit the screen. To long texts will be cut of but they will be visible in the information tab when the row is highlighted.

- ToolsManager.ShowInfo(Dictionary<string,string> info) - This will print information in key value format in the info tab.

ToolsManager.Back() - This will close the current menu and go back the prevoius one.

- ToolsManager.Help() - This will draw a help section showing the user the available commands and functions. It will also ask the current loaded ITool for a documentation that will also be shown here if provied.

- ToolsManager.SetConfiguration(IConfigurationRoot confif) - This will load a new configuration file. For example if you need to switch back and forth between environments.

- ToolsManager.Settings - This part needs some polish still. But you can use this object to store global data with ToolsManager.Settings.AddData(string key, object value). And access it on the ToolsManager.Settings.Data Dictionary. There is also ToolsManager.Settings.AddInfo(string key, string value) that you can use to show persistant data in the top menu. For example you can show information about a logged in user or other state you wish to have visible to the user at all times. (Currently you will need to run Display.DrawTop(ToolsManager.Settings) after you add information to have it show. This will be fixed in next version)

# Warning

This is still in early stages and breaking changes might be part of future versions.

