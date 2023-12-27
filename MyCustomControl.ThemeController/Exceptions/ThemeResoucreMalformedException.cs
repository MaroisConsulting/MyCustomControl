namespace MyCustomControl.ThemeController.Exceptions
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Runtime.Serialization;
  using System.Text;
  using System.Threading.Tasks;

  internal class ThemeResoucreMalformedException : Exception
  {
    public ThemeResoucreMalformedException()
    {
    }

    public ThemeResoucreMalformedException(string? message) : base(message)
    {
    }

    public ThemeResoucreMalformedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ThemeResoucreMalformedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}
