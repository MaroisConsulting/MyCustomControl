namespace MyCustomControl.ThemeController
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
  using System.DirectoryServices;
  using System.IO;
  using System.Reflection;
  using System.Resources;
  using System.Runtime.ConstrainedExecution;
  using System.Security.Cryptography.Pkcs;
  using System.Text;
  using System.Threading.Tasks;
  using System.Windows;
  using System.Windows.Baml2006;
  using System.Windows.Data;
  using System.Windows.Diagnostics;
  using System.Windows.Markup;
  using MyCustomControl.ThemeController.Exceptions;

  public static partial class ThemesResourceManager
  {
    internal const string ThemeFileNameSuffix = "ThemeResources";
    internal const string ThemeFileNameTargetHeaderSeparator = ".";
    internal const string XamlThemeResourceFileNameSuffix = $"{ThemeFileNameTargetHeaderSeparator}{ThemeFileNameSuffix}.xaml";

    private static HashSet<ThemeResourceInfo> RegisteredThemeResourceInfos { get; set; }
    private static HashSet<string> RegisteredAssemblies { get; set; }
    private static HashSet<string> RegisteredXamlFiles { get; set; }
    private static HashSet<ResourceDictionary> RegisteredThemeResourceDictionaries { get; set; }
    private static Assembly CurrentAssembly { get; }

    static ThemesResourceManager()
    {
      ThemesResourceManager.RegisteredThemeResourceInfos = new HashSet<ThemeResourceInfo>();
      ThemesResourceManager.RegisteredAssemblies = new HashSet<string>();
      ThemesResourceManager.RegisteredXamlFiles = new HashSet<string>();
      ThemesResourceManager.RegisteredThemeResourceDictionaries = new HashSet<ResourceDictionary>();
      ThemesResourceManager.CurrentAssembly = Assembly.GetAssembly(typeof(ThemesResourceManager))!;      
    }

    /// <summary>
    /// Get a list of <see cref="ThemeResourceInfo"/> objects.<br/>
    /// Use them to load a particular theme using a <see cref="TryLoadTheme(ThemeResourceInfo)"/> overload.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<ThemeResourceInfo> GetThemeInfos() 
      => ThemesResourceManager.RegisteredThemeResourceInfos;

    /// <summary>
    /// Register all .xaml theme ResourceDictionary files found embedded in an assembly.<br/>
    /// The file names must match the naming convention for theme files.
    /// </summary>
    /// <param name="assemblyContainingThemeResourceXamlFiles">The asssembly to scan for the .xaml files.</param>
    /// <returns>The <see cref="Task"/> of the <see langword="async"/>  operation.</returns>
    /// <exception cref="ThemeResourceNotFoundException">Assembly contains no theme filess or files names violate the naming convention.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="assemblyContainingThemeResourceXamlFiles"/> is <see langword="null"/>.</exception>
    public static async Task<bool> TryRegisterThemesAsync(Assembly assemblyContainingThemeResourceXamlFiles)
    {
      if (assemblyContainingThemeResourceXamlFiles is null)
      {
        throw new ArgumentNullException(nameof(assemblyContainingThemeResourceXamlFiles));
      }

      if (ThemesResourceManager.RegisteredAssemblies.Contains(assemblyContainingThemeResourceXamlFiles.FullName!))
      {
        return false;
      }

      bool hasRegisteredResources = false;
      await foreach (ResourceFinderResult<ResourceDictionary> searchResult in XamlResourceFinder.EnumerateXamlResourcesInAssemblyAsync<ResourceDictionary>(assemblyContainingThemeResourceXamlFiles, ThemesResourceManager.XamlThemeResourceFileNameSuffix))
      {
        hasRegisteredResources |= searchResult.HasResult;
        var themeResourceInfo = new ThemeResourceInfo(searchResult.ResourceName, searchResult.XamlObjectResource!);
        RegisterThemeResource(themeResourceInfo);
      }

      ThemesResourceManager.RegisteredAssemblies.Add(assemblyContainingThemeResourceXamlFiles.FullName!);

      return hasRegisteredResources;
    }

    /// <summary>
    /// Register a .xaml theme ResourceDictionary file.
    /// </summary>
    /// <param name="xamlResourceDictionaryFilePath">The path to the .xaml file that contains the <see cref="ResourceDictionary"/>.</param>
    /// <returns>The <see cref="Task"/> of the <see langword="async"/>  operation.</returns>
    /// <exception cref="ThemeResourceNotFoundException">Invalid file path.</exception>
    public static async Task<bool> TryRegisterThemeAsync(FileInfo themeResourceXamlFileInfo)
    {
      if (themeResourceXamlFileInfo is null)
      {
        throw new ArgumentNullException(nameof(themeResourceXamlFileInfo));
      }

      if (ThemesResourceManager.RegisteredXamlFiles.Contains(themeResourceXamlFileInfo.FullName))
      {
        return false;
      }

      await using FileStream resourceFile = File.OpenRead(themeResourceXamlFileInfo.FullName);
      using var streamReader = new StreamReader(resourceFile);
      string xamlFileContent = await streamReader.ReadToEndAsync();
      if (XamlConverter.TryConvertXamlContentToObject(ThemesResourceManager.CurrentAssembly, xamlFileContent, out ResourceDictionary? themeResources))
      {
        var themeResourceInfo = new ThemeResourceInfo(themeResourceXamlFileInfo.FullName, themeResources!);
        RegisterThemeResource(themeResourceInfo);

        return true;
      }

      return false;
    }

    /// <summary>
    /// Apply theme globally.
    /// </summary>
    /// <param name="resourceInfo">The resource info of the theme to apply.</param>
    /// <returns><c>true</c> if the theme was successfully applied. Otherwise <c>false</c>.</returns>
    public static bool TryLoadTheme(ThemeResourceInfo resourceInfo) 
      => resourceInfo is null 
        ? throw new ArgumentNullException(nameof(resourceInfo)) 
        : TryLoadThemeInternal(resourceInfo, null);

    /// <summary>
    /// Apply theme at control scope.
    /// </summary>
    /// <param name="resourceInfo">The resource info of the theme to apply.</param>
    /// <param name="frameworkElement">The <see cref="FrameworkElement"/> that defines the scope of the theme to apply.</param>
    /// <returns><c>true</c> if the theme was successfully applied. Otherwise <c>false</c>.</returns>
    public static bool TryLoadThemeForControl(ThemeResourceInfo resourceInfo, FrameworkElement frameworkElement) 
      => resourceInfo is null
        ? throw new ArgumentNullException(nameof(resourceInfo))
        : frameworkElement is null
          ? throw new ArgumentNullException(nameof(frameworkElement))
          : TryLoadThemeInternal(resourceInfo, frameworkElement);

    public static void ClearThemeForControl(FrameworkElement frameworkElement)
    {
      if (frameworkElement is null)
      {
        throw new ArgumentNullException(nameof(frameworkElement));
      }

      ClearThemes(frameworkElement.Resources);
    }

    private static bool TryLoadThemeInternal(ThemeResourceInfo resourceInfo, FrameworkElement? frameworkElement)
    {
      if (!RegisteredThemeResourceInfos.Contains(resourceInfo))
      {
        return false;
      }

      bool isThemeGlobal = frameworkElement is null;
      ResourceDictionary targetResourceDictionary = isThemeGlobal
        ? Application.Current.Resources
        : frameworkElement!.Resources;

      ApplyTheme(targetResourceDictionary, resourceInfo.ThemeResourceDictionary);

      return true;
    }

    private static void RegisterThemeResource(ThemeResourceInfo themeResourceInfo)
    {
      ThemesResourceManager.RegisteredXamlFiles.Add(themeResourceInfo.XamlFilePath);
      ThemesResourceManager.RegisteredThemeResourceInfos.Add(themeResourceInfo);
      ThemesResourceManager.RegisteredThemeResourceDictionaries.Add(themeResourceInfo.ThemeResourceDictionary);
    }

    private static void ApplyTheme(ResourceDictionary resourceDictionary, ResourceDictionary themeResources)
    {
      ClearThemes(resourceDictionary);
      resourceDictionary.MergedDictionaries.Add(themeResources);
    }

    private static void ClearThemes(ResourceDictionary resourceDictionary)
    {
      for (int dictionaryIndex = resourceDictionary.MergedDictionaries.Count - 1; dictionaryIndex >= 0; dictionaryIndex--)
      {
        ResourceDictionary mergedDictionary = resourceDictionary.MergedDictionaries[dictionaryIndex];
        if (ThemesResourceManager.RegisteredThemeResourceDictionaries.Contains(mergedDictionary))
        {
          resourceDictionary.MergedDictionaries.RemoveAt(dictionaryIndex);
        }
      }
    }
  }
}
