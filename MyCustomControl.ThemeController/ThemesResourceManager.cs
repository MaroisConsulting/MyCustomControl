namespace MyCustomControl.ThemeController
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.ComponentModel;
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
    internal const string BamlThemeResourceFileNameSuffix = $"{ThemeFileNameTargetHeaderSeparator}{ThemeFileNameSuffix}.baml";
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

      if (await TryCollectResourceDictionaryStreamsAsync(assemblyContainingThemeResourceXamlFiles))
      {
        ThemesResourceManager.RegisteredAssemblies.Add(assemblyContainingThemeResourceXamlFiles.FullName!);

        return true;
      }

      return false;
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
        ThemesResourceManager.RegisteredXamlFiles.Add(themeResourceXamlFileInfo.FullName);
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

    private static async Task<bool> TryCollectResourceDictionaryStreamsAsync(Assembly assembly)
    {
      string[] resourceNames = assembly.GetManifestResourceNames();
      bool hasFoundThemeResource = false;
      foreach (string resourceName in resourceNames)
      {
        if (resourceName.EndsWith("g.resources"))
        {
          hasFoundThemeResource |= await TryCollectCompiledThemeResourceDictionariesAsync(assembly, resourceName);
        }
        // This is the place why following the file name convention,
        // that the filename must end with ".ThemeResources.",
        // is crucial to identifying theme files.
        else if (resourceName.EndsWith(ThemesResourceManager.XamlThemeResourceFileNameSuffix))
        {
          hasFoundThemeResource |= await TryCollectEmbeddedThemeResourceDictionariesAsync(assembly, resourceName);
        }
      }      

      return hasFoundThemeResource;
    }

    private static async Task<bool>TryCollectEmbeddedThemeResourceDictionariesAsync(Assembly assembly, string resourceName)
    {
      await using Stream? resourceFileStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceFileStream is null)
      {
        return false;
      }

      using var streamReader = new StreamReader(resourceFileStream);
      string xamlFileContent = await streamReader.ReadToEndAsync();
      if (XamlConverter.TryConvertXamlContentToObject(assembly, xamlFileContent, out ResourceDictionary? themeResources))
      {
        var themeResourceInfo = new ThemeResourceInfo(resourceName, themeResources!);
        RegisterThemeResource(themeResourceInfo);

        return true;
      }

      return false;
    }

    private static async Task<bool> TryCollectCompiledThemeResourceDictionariesAsync(Assembly assembly, string resourceName)
    {
      bool hasFoundThemeResource = false;
      await using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceStream is null)
      {
        return false;
      }

      var resourceReader = new ResourceReader(resourceStream);
      foreach (DictionaryEntry entry in resourceReader)
      {
        // This is the place why following the file name convention,
        // that the filename must end with ".ThemeResources.",
        // is crucial to identifying theme files.
        if (entry.Key is string xamlResourceName
          && xamlResourceName.EndsWith(ThemesResourceManager.BamlThemeResourceFileNameSuffix, StringComparison.OrdinalIgnoreCase))
        {
          var bamlStream = entry.Value as Stream;
          var bamlReader = new Baml2006Reader(bamlStream);
          object resource = XamlReader.Load(bamlReader);
          if (resource is not ResourceDictionary themeResourceDictionary)
          {
            continue;
          }

          string xamlFileContent = XamlWriter.Save(themeResourceDictionary);
          if (XamlConverter.TryConvertXamlContentToObject(assembly, xamlFileContent, out ResourceDictionary? themeResources))
          {
            hasFoundThemeResource = true;
            var themeResourceInfo = new ThemeResourceInfo(xamlResourceName, themeResources!);
            RegisterThemeResource(themeResourceInfo);
          }
        }
      }

      return hasFoundThemeResource;
    }

    private static void RegisterThemeResource(ThemeResourceInfo themeResourceInfo)
    {
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
