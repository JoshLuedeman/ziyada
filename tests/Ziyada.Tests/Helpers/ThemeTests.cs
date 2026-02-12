namespace Ziyada.Tests.Helpers;

using Terminal.Gui;
using Ziyada.Helpers;

public class ThemeTests
{
    [Fact]
    public void Theme_Bg_IsPureBlack()
    {
        Assert.Equal(0, Theme.Bg.R);
        Assert.Equal(0, Theme.Bg.G);
        Assert.Equal(0, Theme.Bg.B);
    }

    [Fact]
    public void Theme_Fg_IsSoftWhite()
    {
        Assert.Equal(200, Theme.Fg.R);
        Assert.Equal(200, Theme.Fg.G);
        Assert.Equal(200, Theme.Fg.B);
    }

    [Fact]
    public void Theme_Cyan_IsNeonCyan()
    {
        Assert.Equal(0, Theme.Cyan.R);
        Assert.Equal(215, Theme.Cyan.G);
        Assert.Equal(215, Theme.Cyan.B);
    }

    [Fact]
    public void Theme_Green_IsNeonGreen()
    {
        Assert.Equal(0, Theme.Green.R);
        Assert.Equal(215, Theme.Green.G);
        Assert.Equal(135, Theme.Green.B);
    }

    [Fact]
    public void Theme_Magenta_IsHotPink()
    {
        Assert.Equal(215, Theme.Magenta.R);
        Assert.Equal(0, Theme.Magenta.G);
        Assert.Equal(135, Theme.Magenta.B);
    }

    [Fact]
    public void Theme_DimGray_IsMutedGray()
    {
        Assert.Equal(80, Theme.DimGray.R);
        Assert.Equal(80, Theme.DimGray.G);
        Assert.Equal(80, Theme.DimGray.B);
    }

    [Fact]
    public void Theme_Yellow_IsBrightYellow()
    {
        Assert.Equal(215, Theme.Yellow.R);
        Assert.Equal(215, Theme.Yellow.G);
        Assert.Equal(0, Theme.Yellow.B);
    }

    [Fact]
    public void Theme_BrightWhite_IsPureWhite()
    {
        Assert.Equal(255, Theme.BrightWhite.R);
        Assert.Equal(255, Theme.BrightWhite.G);
        Assert.Equal(255, Theme.BrightWhite.B);
    }

    [Fact]
    public void Theme_Base_IsNotNull()
    {
        Assert.NotNull(Theme.Base);
    }

    [Fact]
    public void Theme_Accent_IsNotNull()
    {
        Assert.NotNull(Theme.Accent);
    }

    [Fact]
    public void Theme_Button_IsNotNull()
    {
        Assert.NotNull(Theme.Button);
    }

    [Fact]
    public void Theme_StatusBarScheme_IsNotNull()
    {
        Assert.NotNull(Theme.StatusBarScheme);
    }

    [Fact]
    public void Theme_Table_IsNotNull()
    {
        Assert.NotNull(Theme.Table);
    }

    [Fact]
    public void Theme_TabView_IsNotNull()
    {
        Assert.NotNull(Theme.TabView);
    }

    [Fact]
    public void Theme_Status_IsNotNull()
    {
        Assert.NotNull(Theme.Status);
    }

    [Fact]
    public void Theme_AllSchemesHaveNormalAttribute()
    {
        // Verify all schemes have Normal attributes with valid foreground/background colors
        // Attribute is a struct (value type), so we test that it has non-default color values
        Assert.NotEqual(default, Theme.Base.Normal.Foreground);
        Assert.NotEqual(default, Theme.Accent.Normal.Foreground);
        Assert.NotEqual(default, Theme.Button.Normal.Foreground);
        Assert.NotEqual(default, Theme.StatusBarScheme.Normal.Foreground);
        Assert.NotEqual(default, Theme.Table.Normal.Foreground);
        Assert.NotEqual(default, Theme.TabView.Normal.Foreground);
        Assert.NotEqual(default, Theme.Status.Normal.Foreground);
    }

    [Fact]
    public void Theme_AllSchemesHaveFocusAttribute()
    {
        // Verify all schemes have Focus attributes with valid foreground/background colors
        // Attribute is a struct (value type), so we test that it has non-default color values
        Assert.NotEqual(default, Theme.Base.Focus.Foreground);
        Assert.NotEqual(default, Theme.Accent.Focus.Foreground);
        Assert.NotEqual(default, Theme.Button.Focus.Foreground);
        Assert.NotEqual(default, Theme.StatusBarScheme.Focus.Foreground);
        Assert.NotEqual(default, Theme.Table.Focus.Foreground);
        Assert.NotEqual(default, Theme.TabView.Focus.Foreground);
        Assert.NotEqual(default, Theme.Status.Focus.Foreground);
    }

    [Fact]
    public void Theme_AllSchemesHaveDisabledAttribute()
    {
        // Verify all schemes have Disabled attributes with valid foreground/background colors
        // Attribute is a struct (value type), so we test that it has non-default color values
        Assert.NotEqual(default, Theme.Base.Disabled.Foreground);
        Assert.NotEqual(default, Theme.Accent.Disabled.Foreground);
        Assert.NotEqual(default, Theme.Button.Disabled.Foreground);
        Assert.NotEqual(default, Theme.StatusBarScheme.Disabled.Foreground);
        Assert.NotEqual(default, Theme.Table.Disabled.Foreground);
        Assert.NotEqual(default, Theme.TabView.Disabled.Foreground);
        Assert.NotEqual(default, Theme.Status.Disabled.Foreground);
    }

    [Fact]
    public void Theme_ColorSchemes_AreConsistent()
    {
        // Multiple calls should return same color scheme (testing property getter)
        var base1 = Theme.Base;
        var base2 = Theme.Base;
        
        // Note: These create new instances each time, so we test the values are consistent
        Assert.Equal(base1.Normal.Foreground, base2.Normal.Foreground);
        Assert.Equal(base1.Normal.Background, base2.Normal.Background);
    }
}
