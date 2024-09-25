using System.Drawing;

using Terminal.Gui;

using Color = Terminal.Gui.Color;

namespace VegaExpress.Agent.Constants
{
    internal static class Styles
    {   
        public static Terminal.Gui.ColorScheme ColorBase = new Terminal.Gui.ColorScheme
        {
            Normal = Colors.Dialog.Normal, //ColorNormal,
            Focus = Colors.Dialog.Focus,//ColorFocus,
            HotNormal = Colors.Dialog.HotNormal,//ColorHotNormal,
            HotFocus = Colors.Dialog.HotFocus,//ColorHotFocus,
            Disabled = Colors.Dialog.Disabled,//ColorDisabled
        };

        public static Terminal.Gui.ColorScheme ColorView = new Terminal.Gui.ColorScheme
        {
            Normal = Terminal.Gui.Attribute.Make(ConvertHexToColor("#E5E6E7"), ConvertHexToColor("#282C34")),
            Focus = Terminal.Gui.Attribute.Make(ConvertHexToColor("#F9FAFB"), ConvertHexToColor("#212529")),
            //HotNormal = Terminal.Gui.Attribute.Make(ConvertHexToColor("#F44336"), ConvertHexToColor("#282C34")),
            HotNormal = Terminal.Gui.Attribute.Make(Terminal.Gui.Color.Black, Terminal.Gui.Color.Gray),
            //HotFocus = Terminal.Gui.Attribute.Make(ConvertHexToColor("#FFEBEE"), ConvertHexToColor("#212529")),
            HotFocus = Terminal.Gui.Attribute.Make(Terminal.Gui.Color.Black, Terminal.Gui.Color.Gray),
            Disabled = Terminal.Gui.Attribute.Make(ConvertHexToColor("#36393F"), ConvertHexToColor("#565961"))
        };

        public static ColorScheme colorSchemeScrollBar = new ColorScheme()
        {
            Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            HotNormal = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            HotFocus = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray)
        };
        public static ColorScheme ColorSchemeListView = new ColorScheme()
        {
            Normal = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            Focus = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray),
            HotNormal = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            HotFocus = Terminal.Gui.Attribute.Make(Color.White, Color.Gray),
            Disabled = Terminal.Gui.Attribute.Make(Color.Black, Color.Gray)
        };
        public static Terminal.Gui.Color ConvertHexToColor(string hexColor)
        {
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            int r = int.Parse(hexColor.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(hexColor.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(hexColor.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            int index = (r > 128 | g > 128 | b > 128) ? 8 : 0; // Bright bit
            index |= (r > 64) ? 4 : 0; // Red bit
            index |= (g > 64) ? 2 : 0; // Green bit
            index |= (b > 64) ? 1 : 0; // Blue bit
            return (Terminal.Gui.Color)index;            
        }

        public static Terminal.Gui.Attribute ColorNormal = Terminal.Gui.Attribute.Make(Styles.ConvertHexToColor("#ffffff"), Styles.ConvertHexToColor("#282c34"));
        public static Terminal.Gui.Attribute ColorFocus = Terminal.Gui.Attribute.Make(Styles.ConvertHexToColor("#84ffff"), Styles.ConvertHexToColor("#282c34"));
        public static Terminal.Gui.Attribute ColorHotNormal = Terminal.Gui.Attribute.Make(Styles.ConvertHexToColor("#ffa500"), Styles.ConvertHexToColor("#282c34"));
        public static Terminal.Gui.Attribute ColorHotFocus = Terminal.Gui.Attribute.Make(Styles.ConvertHexToColor("#ff7f00"), Styles.ConvertHexToColor("#282c34"));
        public static Terminal.Gui.Attribute ColorDisabled = Terminal.Gui.Attribute.Make(Styles.ConvertHexToColor("#cccccc"), Styles.ConvertHexToColor("#282c34"));

        public static Table ColorTable = new Table();
        public static Dialog ColorDialog = new Dialog();
        public static StatusBar ColorStatusBar = new StatusBar();
        public static MenuHeader ColorMenuHeader = new MenuHeader();
        public static TextBoxNormal ColorTextBox = new TextBoxNormal();

        public class Table
        {
            public Terminal.Gui.Color GridLines = Styles.ConvertHexToColor("#f2f2f2");
            public Terminal.Gui.Color HeaderText = Styles.ConvertHexToColor("#84ffff");
            public Terminal.Gui.Color CellText = Styles.ConvertHexToColor("#ffffff");
        }
        public class Dialog
        {
            public Terminal.Gui.Color Background = Styles.ConvertHexToColor("#607d8b");
            public Terminal.Gui.Color Border = Styles.ConvertHexToColor("#cccccc");
            public Terminal.Gui.Color Title = Styles.ConvertHexToColor("#ffa500");
            public Terminal.Gui.Color Text = Styles.ConvertHexToColor("#ffffff");
        }
        public class StatusBar
        {
            public Terminal.Gui.Color Foreground = Styles.ConvertHexToColor("#cccccc");
            public Terminal.Gui.Color Background = Styles.ConvertHexToColor("#282c34");
        }
        public class MenuHeader
        {
            public Terminal.Gui.Color Foreground = Styles.ConvertHexToColor("#ffa500");
            public Terminal.Gui.Color Background = Styles.ConvertHexToColor("#282c34");
        }
        public class TextBoxNormal
        {
            public Terminal.Gui.Color Foreground = Styles.ConvertHexToColor("#ffffff");
            public Terminal.Gui.Color Background = Styles.ConvertHexToColor("#f2f2f2");
        }
        public class TextBoxFocus
        {
            public Terminal.Gui.Color Foreground = Styles.ConvertHexToColor("#000000");
            public Terminal.Gui.Color Background = Styles.ConvertHexToColor("#cccccc");
        }
    }  
}