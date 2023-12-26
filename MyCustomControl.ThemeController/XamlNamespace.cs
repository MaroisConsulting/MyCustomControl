namespace MyCustomControl.ThemeController
{
  using System;
  using System.Diagnostics.CodeAnalysis;

  public readonly struct XamlNamespace : IEquatable<XamlNamespace>
  {
    public XamlNamespace(string prefix, string clrNamespace, string assemblyName)
    {
      string exceptionMessage = "String is null or empty.";
      this.Prefix = string.IsNullOrWhiteSpace(prefix) 
        ? throw new ArgumentNullException(nameof(prefix), exceptionMessage) 
        : prefix;
      this.ClrNamespace = string.IsNullOrWhiteSpace(clrNamespace) 
        ? throw new ArgumentNullException(nameof(clrNamespace), exceptionMessage) 
        : clrNamespace;
      this.AssemblyName = string.IsNullOrWhiteSpace(assemblyName) 
        ? throw new ArgumentNullException(nameof(assemblyName), exceptionMessage) 
        : assemblyName;
    }

    public string Prefix { get; }
    public string ClrNamespace { get; }
    public string AssemblyName { get; }

    public bool Equals(XamlNamespace other) => other.Prefix.Equals(this.Prefix, StringComparison.OrdinalIgnoreCase)
      && other.ClrNamespace.Equals(this.ClrNamespace, StringComparison.OrdinalIgnoreCase) 
      && other.AssemblyName.Equals(this.AssemblyName, StringComparison.OrdinalIgnoreCase);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is XamlNamespace xamlNamespace && Equals(xamlNamespace);
    public override int GetHashCode() => HashCode.Combine(this.Prefix, this.ClrNamespace, this.AssemblyName);
    public override string? ToString() => $@"xmlns:{this.Prefix}=""{this.ClrNamespace}"";assembly={this.AssemblyName}";

    public static bool operator ==(XamlNamespace left, XamlNamespace right) => left.Equals(right);
    public static bool operator !=(XamlNamespace left, XamlNamespace right) => !left.Equals(right);
  }
}
