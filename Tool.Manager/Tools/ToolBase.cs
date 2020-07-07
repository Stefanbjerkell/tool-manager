using System;
using System.Collections.Generic;
using System.Text;

namespace Tool.Manager.Tools
{
    public abstract class ToolBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }

        public ToolBase(string name, string description, string id)
        {
            Name = name;
            Description = description;
            Id = id;
        }
    }
}
