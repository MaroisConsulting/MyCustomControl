namespace MyCustomControl.Demo
{
  using System;
  using System.CodeDom;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using System.Windows.Documents;
  using System.Windows.Input;
  using System.Windows.Media;
  using System.Windows.Media.Imaging;
  using System.Windows.Navigation;
  using System.Windows.Shapes;
  using MyCustomControl.ThemeController;
  using MyCustomControl.ThemeController.Exceptions;
  using MyCustomControl.Widget;

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    protected override async void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      /* 
       * You probably want to move the initialization to the App.xaml.cs 
       * You can use the FileSystemWatcher class to observe a directory for changes, 
       * for example when a user drops a '.xaml' file containing theme resources into a specified directory.
       *
       * The following code shows three ways how you can initilaize the applications themes or register new themes. 
       * The relevant API calls are commented out. The example uses the 'MyCustomControl.Widget' assembly to register all contained themes
       * and sets the application level theme to the light theme.
       * 
       * The event handlers at the end of this file, that are registered with the RadioButtons in the MainWindow.xaml, 
       * show how you can dynamically load a registered theme on application level (global) and control level (local).
       * When you run the application and play with the theme select buttons you can see that local themes override application level themes
       * and that by disbaling the local theme the application theme immediately takes over. 
       * This is achieved by adding themes to the MergedDictionary of either the FrameworkElement.Resources or the Application.Resources (App.xaml).
       * Be aware that using element ResourceDictionaries impact the application performance negatively. 
       * That's why it is recommended to add resources to the Application.Resources or MainWIndow.Resources dictionary (when possible).
       * 
       * It's also important to understand that you must reference Color resources in your themes for the Brush resources with StaticResource. 
       * As a consequence, to change the Colror of a Brush, you must override the Brush and not the predefined Color.
       * This is the same behavior that Microsoft implements with predefined SystemColors. 
       * DynamicResource reference in this case would mess with the resource lookup. For example when keys are overridden in Application.Resources AND locally.
       */
      
      // Register from Assembly (you can load assemblies during runtime e.g., from dll file or by assembly name):
      Assembly assemblyContainingThemeResourceDictionaries = Assembly.GetAssembly(typeof(WackyWidget));
      await RegisterAllThemesFoundInAssembly(assemblyContainingThemeResourceDictionaries);
      
      // Register themes found in an external DLL:
      string dllFilePath = "SomeExternalAssembly.dll";
      var dllFileInfo = new FileInfo(dllFilePath);
      //await RegisterAllThemesFoundInDllAsync(dllFileInfo);

      // Register all themes found in a directory:
      Assembly assemblyContainingThemeFiles = Assembly.GetAssembly(typeof(WackyWidget));
      var xamlFilesDirectoryInfo = new DirectoryInfo($"../../../../{assemblyContainingThemeFiles.GetName().Name}/Themes/Themes");
      //await RegisterAllThemesFoundInXamlFilesInDirectoryAsync(xamlFilesDirectoryInfo);
    }

    private async Task RegisterAllThemesFoundInAssembly(Assembly assemblyContainingThemeFiles)
    {
      await ThemesResourceManager.TryRegisterThemesAsync(assemblyContainingThemeFiles);

      LoadDefaultTheme();
    }

    private async Task RegisterAllThemesFoundInDllAsync(FileInfo dllFileInfo)
    {
      if (dllFileInfo.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
      {
        throw new ArgumentException("Wrong file type. File must be a DLL.", nameof(dllFileInfo));
      }

      Assembly assemblyContainingThemeFiles = Assembly.LoadFile(dllFileInfo.FullName);
      await RegisterAllThemesFoundInAssembly(assemblyContainingThemeFiles);
    }

    private async Task RegisterAllThemesFoundInXamlFilesInDirectoryAsync(DirectoryInfo xamlFilesDirectoryInfo)
    {
      var enumerationOptions = new EnumerationOptions()
      {
        IgnoreInaccessible = true,
        RecurseSubdirectories = true,
      };

      foreach (FileInfo fileInfo in xamlFilesDirectoryInfo.EnumerateFiles("*.xaml", enumerationOptions))
      {
        await ThemesResourceManager.TryRegisterThemeAsync(fileInfo);
      }

      LoadDefaultTheme();
    }

    private void LoadDefaultTheme()
    {
      // Load the default theme (the global light theme)
      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();

      ThemeResourceInfo? lightApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals("Application", StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Light", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadTheme(lightApplicationThemeInfo);
    }

    private async void OnLoadLightApplicationThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Light" theme that targets the "Application" globally (WackyApplication.Light.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? lightApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals("Application", StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Light", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadTheme(lightApplicationThemeInfo);
    }

    private async void OnLoadDarkApplicationThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Dark" theme that targets the "Application" globally (WackyApplication.Dark.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? darkApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals("Application", StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Dark", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadTheme(darkApplicationThemeInfo);
    }

    private async void OnLoadLightWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Light" theme that targets the type named "WackyWidget" exclusively (WackyWidget.Light.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? lightApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals(this.WackyWidgetControl.GetType().Name, StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Light", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadThemeForControl(lightApplicationThemeInfo, this.WackyWidgetControl);
    }

    private async void OnLoadDarkWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Dark" theme that targets the type named "WackyWidget" exclusively (WackyWidget.Dark.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? darkApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals(this.WackyWidgetControl.GetType().Name, StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Dark", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadThemeForControl(darkApplicationThemeInfo, this.WackyWidgetControl);
    }

    private async void OnLoadLightAnotherWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Light" theme that targets the type named "WackyWidget" exclusively (WackyWidget.Light.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? lightApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals(this.AnotherWackyWidgetControl.GetType().Name, StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Light", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadThemeForControl(lightApplicationThemeInfo, this.AnotherWackyWidgetControl);
    }

    private async void OnLoadDarkAnotherWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e)
    {
      /* Apply the "Dark" theme that targets the type named "WackyWidget" exclusively (WackyWidget.Dark.ThemeResources.xaml) */

      IEnumerable<ThemeResourceInfo> themeInfos = ThemesResourceManager.GetThemeInfos();
      ThemeResourceInfo? darkApplicationThemeInfo = themeInfos
        .FirstOrDefault(themeInfo =>
          themeInfo.ThemeScope.Equals(this.AnotherWackyWidgetControl.GetType().Name, StringComparison.OrdinalIgnoreCase)
          && themeInfo.ThemeName.Equals("Dark", StringComparison.OrdinalIgnoreCase));
      ThemesResourceManager.TryLoadThemeForControl(darkApplicationThemeInfo, this.AnotherWackyWidgetControl);
    }

    private void OnClearWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e) 
      => ThemesResourceManager.ClearThemeForControl(this.WackyWidgetControl);

    private void OnClearAnotherWackyWidgetThemeButtonClicked(object sender, RoutedEventArgs e) 
      => ThemesResourceManager.ClearThemeForControl(this.AnotherWackyWidgetControl);
  }
}
