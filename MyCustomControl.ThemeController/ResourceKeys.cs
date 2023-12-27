namespace MyCustomControl.ThemeController
{
  using System.Windows;

  public static class ResourceKeys
  {
    public static ComponentResourceKey ControlBackgroundColorKey =
      new ComponentResourceKey(typeof(ResourceKeys), "ControlBackgroundColor");

    public static ComponentResourceKey ControlBackgroundBrushKey =
      new ComponentResourceKey(typeof(ResourceKeys), "ControlBackgroundBrush");

    public static ComponentResourceKey ControlBorderColorKey =
      new ComponentResourceKey(typeof(ResourceKeys), "ControlBorderColor");

    public static ComponentResourceKey ControlBorderBrushKey =
      new ComponentResourceKey(typeof(ResourceKeys), "ControlBorderBrush");

    public static ComponentResourceKey TextColorKey =
      new ComponentResourceKey(typeof(ResourceKeys), "TextColor");

    public static ComponentResourceKey TextBrushKey =
      new ComponentResourceKey(typeof(ResourceKeys), "TextBrush");

    public static ComponentResourceKey CaptionForegroundColorKey =
      new ComponentResourceKey(typeof(ResourceKeys), "CaptionForegroundColor");

    public static ComponentResourceKey CaptionForegroundBrushKey =
      new ComponentResourceKey(typeof(ResourceKeys), "CaptionForegroundBrush");
  }
}
