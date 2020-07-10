using System;
using System.Collections.Generic;
using System.Text;

namespace Tool.Manager.GUI
{
    public class Menu
    {
        public string Title { get; set; }

        public List<MenuItem> Items { get; set; }
    }

    public class MenuItem
    {
        public MenuItemType Type { get; set; } = MenuItemType.Item;

        public string Description { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int Nesting { get; set; }
    }

    public enum MenuItemType
    {
        Item,
        Divider,
        Headline,
    }
}
