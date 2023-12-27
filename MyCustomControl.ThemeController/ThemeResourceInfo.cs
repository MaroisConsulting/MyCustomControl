namespace MyCustomControl.ThemeController
{
  using System.IO;
  using System.Linq;
  using System.Windows;

  public class ThemeResourceInfo
  {
    private const string MalformedFileNameFormatExceptionMessage = @"
Filename must end with using the following format: 
'<theme_target>.<theme_name>.ThemeResources.xaml', 
where <theme_target> can be 'Application' 
or the target type's name (including the namespace if required.)";

    public ThemeResourceInfo(string xamlFilePath, ResourceDictionary themeResourceDictionary)
    {
      this.ThemeResourceDictionary = themeResourceDictionary ?? throw new ArgumentNullException(nameof(themeResourceDictionary));

      string exceptionMessage = "String is null or empty.";
      this.XamlFilePath = string.IsNullOrWhiteSpace(xamlFilePath)
        ? throw new ArgumentNullException(nameof(xamlFilePath), exceptionMessage)
        : xamlFilePath;

      this.OriginalThemeResourceKey = Path.GetFileNameWithoutExtension(xamlFilePath);
      InitializeThemeInfo(this.OriginalThemeResourceKey);
    }

    public override string ToString() => this.OriginalThemeResourceKey ?? string.Empty;

    private void InitializeThemeInfo(string xamlFilePathWithoutFileExtension)
    {
      /* Another location nwhere the naming convention matters */

      string[] fileNameParts = xamlFilePathWithoutFileExtension.Split('.');
      if (fileNameParts.Length < 3
        || !fileNameParts.Last().Equals(ThemesResourceManager.ThemeFileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
      {
        throw new FormatException(MalformedFileNameFormatExceptionMessage);
      }

      int indexThemeFileId = xamlFilePathWithoutFileExtension.Length - (ThemesResourceManager.ThemeFileNameSuffix.Length + 1);
      int indexOfThemeName = xamlFilePathWithoutFileExtension.LastIndexOf(ThemesResourceManager.ThemeFileNameTargetHeaderSeparator, indexThemeFileId - 1);
      int indexOfThemeScope = xamlFilePathWithoutFileExtension.LastIndexOf(ThemesResourceManager.ThemeFileNameTargetHeaderSeparator, indexOfThemeName - 1);

      this.ThemeScope = xamlFilePathWithoutFileExtension[(indexOfThemeScope + 1)..indexOfThemeName];
      this.ThemeName = xamlFilePathWithoutFileExtension[(indexOfThemeName + 1)..indexThemeFileId];
    }

    public string ThemeName { get; private set; }
    public string ThemeScope { get; private set; }
    internal string OriginalThemeResourceKey { get; }
    internal ResourceDictionary ThemeResourceDictionary { get; set; }
    public string XamlFilePath { get; }
  }  
}
