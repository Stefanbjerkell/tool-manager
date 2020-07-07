using System;
using Tool.Client.Tools;
using Tool.Manager;

namespace Tool.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ToolsManager.Init(new ExampeTool());

            ToolsManager.Run();
        }
    }
}
