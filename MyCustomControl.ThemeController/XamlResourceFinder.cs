namespace MyCustomControl.ThemeController
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.IO;
  using System.Reflection;
  using System.Resources;
  using System.Threading.Tasks;
  using System.Windows.Baml2006;
  using System.Windows.Markup;

  internal static class XamlResourceFinder
  {
    public static async IAsyncEnumerable<ResourceFinderResult<TResource>> EnumerateXamlResourcesInAssemblyAsync<TResource>(Assembly assembly, string xamlResourceFileName)
    {
      string[] resourceNames = assembly.GetManifestResourceNames();
      foreach (string resourceName in resourceNames)
      {
        if (resourceName.EndsWith("g.resources"))
        {
          await foreach (ResourceFinderResult<TResource> searchResult in EnumerateCompiledXamlResourcesAsync<TResource>(assembly, resourceName, xamlResourceFileName))
          {
            if (searchResult.HasResult)
            {
              yield return searchResult;
            }
          }
        }
        // This is the place why following the file name convention,
        // that the filename must end with ".ThemeResources.",
        // is crucial to identifying theme files.
        else if (resourceName.EndsWith(xamlResourceFileName))
        {
          ResourceFinderResult<TResource> searchResult = await GetEmbeddedXamlResourceAsync<TResource>(assembly, resourceName);
          if (searchResult.HasResult)
          {
            yield return searchResult;
          }
        }
      }
    }

    private static async IAsyncEnumerable<ResourceFinderResult<TResource>> EnumerateCompiledXamlResourcesAsync<TResource>(Assembly assembly, string resourceName, string xamlResourceFileName)
    {
      await using Stream? resourceStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceStream is null)
      {
        yield return ResourceFinderResult<TResource>.Invalid;
      }

      string xamlFileName = xamlResourceFileName switch
      {
        _ when xamlResourceFileName.Equals("*.xaml", StringComparison.OrdinalIgnoreCase) => ".baml",
        _ when xamlResourceFileName.Equals("*.baml", StringComparison.OrdinalIgnoreCase) => ".baml",
        _ when Path.GetExtension(xamlResourceFileName).Equals(".baml", StringComparison.OrdinalIgnoreCase) => xamlResourceFileName,
        _ when Path.GetExtension(xamlResourceFileName).Equals(".xaml", StringComparison.OrdinalIgnoreCase) => Path.ChangeExtension(xamlResourceFileName, ".baml"),
        _ => Path.ChangeExtension(xamlResourceFileName, ".baml"),
      };

      var resourceReader = new ResourceReader(resourceStream!);
      foreach (DictionaryEntry entry in resourceReader)
      {
        // This is the place why following the file name convention,
        // that the filename must end with ".ThemeResources.",
        // is crucial to identifying theme files.
        if (entry.Key is string xamlResourceName
          && xamlResourceName.EndsWith(xamlFileName, StringComparison.OrdinalIgnoreCase))
        {
          var bamlStream = entry.Value as Stream;
          var bamlReader = new Baml2006Reader(bamlStream);
          object resource = XamlReader.Load(bamlReader);
          if (resource is not TResource xamlObject)
          {
            continue;
          }

          string xamlFileContent = XamlWriter.Save(xamlObject);
          if (XamlConverter.TryConvertXamlContentToObject(assembly, xamlFileContent, out xamlObject!))
          {
            var searchResult = new ResourceFinderResult<TResource>(xamlObject!, xamlResourceName);
            if (searchResult.HasResult)
            {
              yield return searchResult;
            }
          }
        }
      }
    }

    private static async Task<ResourceFinderResult<TResource>> GetEmbeddedXamlResourceAsync<TResource>(Assembly assembly, string resourceName)
    {
      await using Stream? resourceFileStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceFileStream is null)
      {
        return ResourceFinderResult<TResource>.Invalid;
      }

      using var streamReader = new StreamReader(resourceFileStream);
      string xamlFileContent = await streamReader.ReadToEndAsync();
      return XamlConverter.TryConvertXamlContentToObject(assembly, xamlFileContent, out TResource? xamlObject)
        ? new ResourceFinderResult<TResource>(xamlObject!, resourceName)
        : ResourceFinderResult<TResource>.Invalid;
    }
  }
}