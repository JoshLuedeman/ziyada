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
        // Verify all schemes have a Normal attribute set
        Assert.NotNull(Theme.Base.Normal);
        Assert.NotNull(Theme.Accent.Normal);
        Assert.NotNull(Theme.Button.Normal);
        Assert.NotNull(Theme.StatusBarScheme.Normal);
        Assert.NotNull(Theme.Table.Normal);
        Assert.NotNull(Theme.TabView.Normal);
        Assert.NotNull(Theme.Status.Normal);
    }

    [Fact]
    public void Theme_AllSchemesHaveFocusAttribute()
    {
        // Verify all schemes have a Focus attribute set
        Assert.NotNull(Theme.Base.Focus);
        Assert.NotNull(Theme.Accent.Focus);
        Assert.NotNull(Theme.Button.Focus);
        Assert.NotNull(Theme.StatusBarScheme.Focus);
        Assert.NotNull(Theme.Table.Focus);
        Assert.NotNull(Theme.TabView.Focus);
        Assert.NotNull(Theme.Status.Focus);
    }

    [Fact]
    public void Theme_AllSchemesHaveDisabledAttribute()
    {
        // Verify all schemes have a Disabled attribute set
        Assert.NotNull(Theme.Base.Disabled);
        Assert.NotNull(Theme.Accent.Disabled);
        Assert.NotNull(Theme.Button.Disabled);
        Assert.NotNull(Theme.StatusBarScheme.Disabled);
        Assert.NotNull(Theme.Table.Disabled);
        Assert.NotNull(Theme.TabView.Disabled);
        Assert.NotNull(Theme.Status.Disabled);
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
