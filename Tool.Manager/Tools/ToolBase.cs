using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tool.Manager.GUI;
using Tool.Manager.Interfaces;

namespace Tool.Manager.Tools
{
    public abstract class ToolBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }

        public virtual List<ToolsAction> Actions { get; set; }

        public virtual List<Documentation> Documentation { get; set; }

        public virtual async Task<bool> ExecuteCommand(string command)
        {
            ToolsManager.Log($"{Name} cannot execute command [{command}]");
            return false;
        }

        public virtual async Task<bool> ExecuteMenuItem(MenuItem item)
        {
            return false;
        }

        public ToolBase(string name, string description, string id)
        {
            Name = name;
            Description = description;
            Id = id;
        }

        public virtual async Task Run()
        {
            ToolsManager.ListActions(Actions);
        }
    }
}
