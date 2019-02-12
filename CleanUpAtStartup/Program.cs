using System;

namespace CleanUpAtStartup
{
  internal class Program
  {
    private static void Main()
    {
      Action<string> display = Console.WriteLine;
      string temporaryDirectory = "c:\\temp";
      
      // for each sub-directories, try to delete every files
      try
      {

      }
      catch (Exception)
      {
        // skip file and try next one
      }
    }
  }
}
