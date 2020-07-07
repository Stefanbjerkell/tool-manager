using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Tool.Manager.Interfaces;

namespace Tool.Manager.GUI
{
    public static class Display
    {
        public const ConsoleColor HeaderColor = ConsoleColor.White;
        public const ConsoleColor MenuSelectColor = ConsoleColor.Yellow;

        public static int Height = 60;
        public static int Width = 210;

        private static int MenuWidth = 30;
        private static int MenuHeight = 45;
        private static int DataWidth = 120;
        private static int TopHeight = 4;


        private static int[] columnWidths;

        public static void Init(IConfiguration configuration)
        {
            Console.SetWindowSize(Width, Height);

            Highlight(Tab.Top, false);
            Highlight(Tab.Menu, false);
            Highlight(Tab.Data, false);
            Highlight(Tab.Info, false);
            Highlight(Tab.Console, false);
        }

        public static void Highlight(Tab tab, bool value = true)
        {
            var color = value ? ConsoleColor.Yellow : ConsoleColor.Gray;

            switch (tab)
            {
                case Tab.Top:
                    DrawBox(0, 0, TopHeight, Width, color, value);
                    break;
                case Tab.Menu:
                    DrawBox(0, TopHeight + 1, MenuHeight, MenuWidth, color, value);
                    break;
                case Tab.Data:
                    DrawBox(MenuWidth + 1, TopHeight + 1, MenuHeight, DataWidth, color, value);
                    break;
                case Tab.Info:
                    DrawBox(MenuWidth + DataWidth + 2, TopHeight + 1, MenuHeight, Width - MenuWidth - DataWidth - 2, color, value);
                    break;
                case Tab.Console:
                    DrawBox(0, TopHeight + MenuHeight + 2, Height - TopHeight - MenuHeight - 3, Width, color, value);
                    break;
            }
        }

        public static Point GetConsolePosition()
        {
            return new Point(2, Height - 2);
        }

        public static int GetMaxTableRows()
        {
            return MenuHeight - 5;
        }

        public static void MenuSelect(MenuItem menuItem, int index, bool deselect = false)
        {
            var pos = new Point(2, TopHeight + 2);

            Console.SetCursorPosition(pos.X, pos.Y + 2 + index);
            Console.ForegroundColor = deselect ? ConsoleColor.Gray : MenuSelectColor;
            Console.Write(menuItem.Text);

            if (!deselect)
            {
                DrawInfo(new Dictionary<string, string> {
                    { "Command", menuItem.Text },
                    { "Description", menuItem.Description },
                    { "Value", menuItem.Value } });
            }

        }

        public static void RowSelect(TableRow row, int index, bool deselect = false)
        {
            var pos = new Point(MenuWidth + 4, TopHeight + 4);

            Console.SetCursorPosition(pos.X, pos.Y + 2 + index);
            Console.ForegroundColor = deselect ? string.IsNullOrEmpty(row.GroupHeader) ? ConsoleColor.Gray : ConsoleColor.DarkCyan : MenuSelectColor;

            var line = "";

            if (!string.IsNullOrEmpty(row.GroupHeader))
            {
                line = row.GroupHeader.ToUpper();
            }
            else
            {
                for (int i = 0; i < row.Values.Count; i++)
                {
                    var value = row.Values[i] ?? "";
                    if (value.Length > columnWidths[i])
                    {
                        value = value.Substring(0, columnWidths[i] - 5) + "...";
                    }

                    line += value + new string(' ', columnWidths[i] - value.Length);
                }
            }

            Console.SetCursorPosition(pos.X, pos.Y + index);
            Console.Write(line);

            if (!deselect)
            {
                DrawInfo(row.Info);
            }
            else
            {
                ClearInfo();
            }
        }

        public static void MenuDeSelect(MenuItem menuItem, int index)
        {
            MenuSelect(menuItem, index, true);
        }

        public static void DrawMenu(Menu menu)
        {
            ClearMenu();

            var pos = new Point(2, TopHeight + 2);
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(pos.X, pos.Y);
            Console.Write(menu.Title);
            Console.SetCursorPosition(pos.X, pos.Y + 1);
            Console.Write(new string('-', MenuWidth - 4));

            Console.ForegroundColor = ConsoleColor.Gray;

            for (int i = 0; i < menu.Items.Count; i++)
            {
                Console.SetCursorPosition(pos.X, pos.Y + 2 + i);
                Console.Write(menu.Items[i].Text);
            }
        }

        public static void ClearMenu()
        {
            var pos = new Point(1, TopHeight + 2);
            for (int i = 0; i < MenuHeight - 1; i++)
            {
                Console.SetCursorPosition(pos.X, pos.Y + i);
                Console.Write(new string(' ', MenuWidth - 2));
            }
        }

        public static void DrawTable(Table table, int page)
        {
            var maxRows = GetMaxTableRows();

            ClearTable();

            var pos = new Point(MenuWidth + 4, TopHeight + 2);

            // Start by calculating the column widths needed.
            columnWidths = new int[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columnWidths[i] = table.Columns[i].Length;
            }

            foreach (var row in table.Rows.Where(x => string.IsNullOrEmpty(x.GroupHeader)))
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if ((row.Values[i] is null ? 0 : row.Values[i].Length) > columnWidths[i])
                        columnWidths[i] = row.Values[i].Length;
                }
            }

            // TODO. Make adjustments to create space if possible.
            for (int i = 0; i < table.Columns.Count; i++)
            {
                columnWidths[i] += 2;
            }

            var headerLength = columnWidths.Sum();
            var headerMaxLength = DataWidth - 6;


            for (int i = 0; i < table.Columns.Count; i++)
            {
                var longets = columnWidths.Max();

                if (columnWidths[i] == longets)
                {
                    columnWidths[i] -= headerLength - headerMaxLength;
                    break;
                }
            }

            // Draw header
            var header = "";
            Console.ForegroundColor = HeaderColor;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                header += table.Columns[i] + new string(' ', columnWidths[i] - table.Columns[i].Length);
            }
            Console.SetCursorPosition(pos.X, pos.Y);
            Console.Write(header);
            Console.SetCursorPosition(pos.X, pos.Y + 1);
            Console.Write(new string('-', DataWidth - 6));

            // Draw Rows.



            var index = 0;
            foreach (var row in table.Rows.Where(x => table.Rows.IndexOf(x) >= (maxRows * page)))
            {
                var line = "";

                if (!string.IsNullOrEmpty(row.GroupHeader))
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    line = row.GroupHeader.ToUpper();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        var value = row.Values[i] ?? "";
                        if (value.Length > columnWidths[i])
                        {
                            value = value.Substring(0, columnWidths[i] - 5) + "...";
                        }

                        line += value + new string(' ', columnWidths[i] - value.Length);
                    }
                }

                Console.SetCursorPosition(pos.X, pos.Y + index + 2);
                Console.Write(line);

                index++;

                if (index > maxRows)
                {
                    break;
                }
            }

            if (table.Rows.Count > maxRows)
            {
                var maxPages = Convert.ToInt32(table.Rows.Count / maxRows);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.SetCursorPosition(pos.X + DataWidth - 18, pos.Y + MenuHeight - 2);
                Console.Write($"Page [{page}/{maxPages}]");
                Console.ResetColor();
                return;
            }
        }

        public static void ClearTable()
        {
            var pos = new Point(MenuWidth + 4, TopHeight + 2);
            for (int i = 0; i < MenuHeight - 1; i++)
            {
                Console.SetCursorPosition(pos.X, pos.Y + i);
                Console.Write(new string(' ', DataWidth - 6));
            }
        }

        public static void DrawInfo(Dictionary<string, string> info)
        {
            ClearInfo();

            Console.ForegroundColor = ConsoleColor.White;

            var pos = new Point(MenuWidth + DataWidth + 4, TopHeight + 1);

            var column1Width = info.Max(x => x.Key.Length);
            var column2Width = Width - MenuWidth - DataWidth - 5 - column1Width - 2;
            var row = 0;



            foreach (var item in info)
            {
                row++;

                Console.ForegroundColor = ConsoleColor.White;

                Console.SetCursorPosition(pos.X, pos.Y + row);
                Console.Write(item.Key);

                Console.ForegroundColor = ConsoleColor.DarkCyan;

                var text = item.Value ?? "";
                while (text.Length > column2Width)
                {
                    Console.SetCursorPosition(pos.X + column1Width + 2, pos.Y + row);
                    Console.Write(text.Substring(0, column2Width));
                    text = text.Substring(column2Width);
                    row++;
                }
                Console.SetCursorPosition(pos.X + column1Width + 2, pos.Y + row);
                Console.Write(text);
            }
            Console.ResetColor();
        }

        public static void ClearInfo()
        {
            var pos = new Point(MenuWidth + DataWidth + 4, TopHeight + 2);
            for (int i = 0; i < MenuHeight - 1; i++)
            {
                Console.SetCursorPosition(pos.X, pos.Y + i);
                Console.Write(new string(' ', Width - MenuWidth - DataWidth - 5));
            }
        }

        public static void DrawTop(ToolsSettings settings)
        {
            var pos = new Point(1, 1);

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.SetCursorPosition(pos.X + 1, pos.Y);
            Console.Write("╔═╗ +");
            Console.SetCursorPosition(pos.X + 1, pos.Y + 1);
            Console.Write("╚═╬═╗");
            Console.SetCursorPosition(pos.X + 1, pos.Y + 2);
            Console.Write("+ ╚═╝");

            Console.SetCursorPosition(pos.X + Width - 8, pos.Y);
            Console.Write("+ ╔═╗");
            Console.SetCursorPosition(pos.X + Width - 8, pos.Y + 1);
            Console.Write("╔═╬═╝");
            Console.SetCursorPosition(pos.X + Width - 8, pos.Y + 2);
            Console.Write("╚═╝ +");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(pos.X + 8, pos.Y);
            Console.Write(settings.Title);

            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(pos.X + 8, pos.Y + 1);
            Console.Write(settings.Description);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.SetCursorPosition(pos.X + 8, pos.Y + 2);
            Console.Write("Version " + settings.Version);

            if(settings.Info != null && settings.Info.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                var count = 0;
                foreach(var info in settings.Info)
                {
                    Console.SetCursorPosition(pos.X + Width - (info.Key.Length + info.Value.Length) - 13, pos.Y + count);
                    Console.Write($"{info.Key} [{info.Value}]");

                    if (count > TopHeight - 3) break;
                    count++;
                }
            }

        }

        public static void DrawConsole(List<string> lines)
        {
            var pos = GetConsolePosition();
            Console.SetCursorPosition(pos.X, pos.Y);
            Console.Write(new string(' ', Width - 4));

            for (int i = 0; i < 6 && i < lines.Count; i++)
            {
                var color = ConsoleColor.White;

                Console.SetCursorPosition(pos.X, pos.Y - i - 1);
                var text = lines[lines.Count - 1 - i];
                var space = Width - 4 - text.Length;

                if (text.StartsWith("!"))
                {
                    color = ConsoleColor.Red;
                    text = text.Substring(1);
                }
                if (text.StartsWith(">>"))
                {
                    color = ConsoleColor.Gray;
                }
                if (text.StartsWith("?"))
                {
                    color = ConsoleColor.Magenta;
                    text = text.Substring(1);
                }
                if (text.StartsWith("[s]"))
                {
                    color = ConsoleColor.DarkGreen;
                    text = text.Substring(3);
                }

                Console.ForegroundColor = color;

                foreach (var c in text.ToArray())
                {
                    if (c == '[') Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(c);
                    if (c == ']') Console.ForegroundColor = color;
                }

                Console.Write(new string(' ', space));
            }

            Console.ForegroundColor = ConsoleColor.White;

        }

        private static void DrawBox(int x, int y, int height, int width, ConsoleColor color, bool doubbleLine = false)
        {
            var charSet = doubbleLine ?
                //       0    1    2    3    4    5    6    7    8    9    10
                new[] { '╔', '╗', '╚', '╝', '═', '║', '╠', '╦', '╩', '╣', '╬' } :
                new[] { '┌', '┐', '└', '┘', '─', '│', '├', '┬', '┴', '┤', '┼' };


            Console.ForegroundColor = color;

            Console.SetCursorPosition(x, y);
            Console.Write(charSet[0] + new String(charSet[4], width - 2) + charSet[1]);
            for (int i = 1; i < height; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(charSet[5]);
                Console.SetCursorPosition(x + width - 1, y + i);
                Console.Write(charSet[5]);
            }
            Console.SetCursorPosition(x, y + height);
            Console.Write(charSet[2] + new String(charSet[4], width - 2) + charSet[3]);

            Console.ResetColor();
        }

    }
}
