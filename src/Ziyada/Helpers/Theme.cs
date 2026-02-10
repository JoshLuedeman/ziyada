using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace Ziyada.Helpers;

public static class Theme
{
    // Accent colors
    public static readonly Color Bg = new(0, 0, 0);           // Pure black
    public static readonly Color Fg = new(200, 200, 200);     // Soft white
    public static readonly Color Cyan = new(0, 215, 215);     // Neon cyan
    public static readonly Color Green = new(0, 215, 135);    // Neon green
    public static readonly Color Magenta = new(215, 0, 135);  // Hot pink
    public static readonly Color DimGray = new(80, 80, 80);   // Muted gray
    public static readonly Color Yellow = new(215, 215, 0);   // Bright yellow
    public static readonly Color BrightWhite = new(255, 255, 255);

    public static ColorScheme Base => new()
    {
        Normal = new Attribute(Fg, Bg),
        Focus = new Attribute(BrightWhite, new Color(30, 30, 50)),
        HotNormal = new Attribute(Cyan, Bg),
        HotFocus = new Attribute(Green, new Color(30, 30, 50)),
        Disabled = new Attribute(DimGray, Bg),
    };

    public static ColorScheme Accent => new()
    {
        Normal = new Attribute(Cyan, Bg),
        Focus = new Attribute(BrightWhite, new Color(0, 80, 80)),
        HotNormal = new Attribute(Green, Bg),
        HotFocus = new Attribute(BrightWhite, new Color(0, 80, 80)),
        Disabled = new Attribute(DimGray, Bg),
    };

    public static ColorScheme Button => new()
    {
        Normal = new Attribute(Bg, Cyan),
        Focus = new Attribute(Bg, Green),
        HotNormal = new Attribute(Bg, Cyan),
        HotFocus = new Attribute(Bg, Green),
        Disabled = new Attribute(DimGray, new Color(30, 30, 30)),
    };

    public static ColorScheme StatusBarScheme => new()
    {
        Normal = new Attribute(Cyan, new Color(20, 20, 20)),
        Focus = new Attribute(Green, new Color(20, 20, 20)),
        HotNormal = new Attribute(Green, new Color(20, 20, 20)),
        HotFocus = new Attribute(BrightWhite, new Color(20, 20, 20)),
        Disabled = new Attribute(DimGray, new Color(20, 20, 20)),
    };

    public static ColorScheme Table => new()
    {
        Normal = new Attribute(Fg, Bg),
        Focus = new Attribute(BrightWhite, new Color(0, 60, 60)),
        HotNormal = new Attribute(Cyan, Bg),
        HotFocus = new Attribute(Green, new Color(0, 60, 60)),
        Disabled = new Attribute(DimGray, Bg),
    };

    public static ColorScheme TabView => new()
    {
        Normal = new Attribute(DimGray, Bg),
        Focus = new Attribute(Cyan, Bg),
        HotNormal = new Attribute(Cyan, Bg),
        HotFocus = new Attribute(Green, Bg),
        Disabled = new Attribute(DimGray, Bg),
    };

    public static ColorScheme Status => new()
    {
        Normal = new Attribute(Yellow, Bg),
        Focus = new Attribute(Yellow, Bg),
        HotNormal = new Attribute(Yellow, Bg),
        HotFocus = new Attribute(Yellow, Bg),
        Disabled = new Attribute(DimGray, Bg),
    };
}
