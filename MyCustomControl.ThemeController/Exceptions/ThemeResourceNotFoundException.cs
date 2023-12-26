namespace MyCustomControl.ThemeController.Exceptions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Text;
  using System.Threading.Tasks;

  public class ThemeResourceNotFoundException : Exception
  {
    public ThemeResourceNotFoundException()
    {
    }

    public ThemeResourceNotFoundException(string? message) : base(message)
    {
    }

    public ThemeResourceNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ThemeResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
