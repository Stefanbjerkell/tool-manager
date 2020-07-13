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
