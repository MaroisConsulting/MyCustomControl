namespace MyCustomControl.ThemeController
{
  internal class ResourceFinderResult<TResource>
  {
    private ResourceFinderResult() : this(default, string.Empty)
    {
    }

    public ResourceFinderResult(TResource? resource, string resourceName)
    {
      this.XamlObjectResource = resource;
      this.ResourceName = resourceName ?? string.Empty;
    }

    public static ResourceFinderResult<TResource> Invalid { get; } = new ResourceFinderResult<TResource>();
    public bool HasResult => !this.XamlObjectResource?.Equals(default) ?? false;
    public TResource XamlObjectResource { get; }
    public string ResourceName { get; }
  }
}
