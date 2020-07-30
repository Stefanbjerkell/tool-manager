using System;
using System.Collections.Generic;
using System.Text;

namespace Tool.Manager.GUI
{
    public class InfoRow
    {

        public InfoRow(string key, string value, InfoRowType type = InfoRowType.Data, ConsoleColor? color = null)
        {
            Key = key;
            Value = value;
            Type = type;
            Color = color;
        }

        public string Key { get; set; }

        public string Value { get; set; }

        public InfoRowType Type { get; set; }

        public ConsoleColor? Color { get; set; } = null;
    }

    public enum InfoRowType
    {
        Title,
        Section,
        Data,
        Divider
    }
}
