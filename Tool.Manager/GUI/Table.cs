using System;
using System.Collections.Generic;
using System.Text;

namespace Tool.Manager.GUI
{
    public class Table
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Columns { get; set; }
        public List<TableRow> Rows { get; set; } = new List<TableRow>();
    }
    public class TableRow
    {
        public string GroupHeader { get; set; }

        public List<string> Values { get; set; }

        public Dictionary<string, string> Info { get; set; }

        public string Id { get; set; }
    }
}
