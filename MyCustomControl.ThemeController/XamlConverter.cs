namespace MyCustomControl.ThemeController
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Text.RegularExpressions;
  using System.Windows.Markup;

  internal static class XamlConverter
  {
    private static Regex XamlNamsespaceRegexParser { get; }

    static XamlConverter()
    { 
      // We have to parse the XAML files for custom namespaces (for example 'themes:' and 'local:' in our theme resources)
      // in order to map the types properly.
      // To do this, we have to extract the xaml namespace alias, the clr namespace it maps to and the assembly of the clr namespace.
      string regexPattern = @"xmlns:(?<xmlNamespace>.+)=""clr-namespace:(?<clrNamespace>[a-zA-Z0-9.]+?)(;assembly=(?<assembly>[a-zA-Z0-9.]+))?""";
      XamlConverter.XamlNamsespaceRegexParser = new Regex(
        regexPattern,
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
    }

    public static bool TryConvertXamlContentToObject<TRootElement>(Assembly assemblyOfXamlFile, string xamlFileContent, out TRootElement? xamlObject)
    {
      xamlObject = default;

      var parserContext = new ParserContext();
      var xamlTypeMapper = new XamlTypeMapper(Array.Empty<string>());
      IEnumerable<XamlNamespace> xamlNamespaces = GetXamlNamespaces(xamlFileContent, assemblyOfXamlFile);
      foreach (XamlNamespace xamlNamespaceInfo in xamlNamespaces)
      {
        xamlTypeMapper.AddMappingProcessingInstruction(xamlNamespaceInfo.Prefix, xamlNamespaceInfo.ClrNamespace, xamlNamespaceInfo.AssemblyName);
      }

      parserContext.XamlTypeMapper = xamlTypeMapper;
      object resource = XamlReader.Parse(xamlFileContent, parserContext);
      if (resource is TRootElement xamlRootElement)
      {
        xamlObject = xamlRootElement;
        return true;
      }

      return false;
    }

    private static IEnumerable<XamlNamespace> GetXamlNamespaces(string xamlFileContent, Assembly assembly)
    {
      MatchCollection matches = XamlConverter.XamlNamsespaceRegexParser.Matches(xamlFileContent);
      var xamlNamespaces = new List<XamlNamespace>();
      foreach (Match match in matches)
      {
        if (!match.Success && match.Groups.Count < 2)
        {
          continue;
        }

        string prefix = match.Groups["xmlNamespace"].Value;
        string clrNamespace = match.Groups["clrNamespace"].Value;
        string assemblyContainingClrNamespace = match.Groups.Keys.Contains("assembly")
          ? match.Groups["assembly"].Value
          : assembly?.GetName().Name;
        var xamlNamespaceMapping = new XamlNamespace(prefix, clrNamespace, assemblyContainingClrNamespace);
        xamlNamespaces.Add(xamlNamespaceMapping);
      }

      return xamlNamespaces;
    }
  }
}
