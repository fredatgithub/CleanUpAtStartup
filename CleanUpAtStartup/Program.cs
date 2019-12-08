using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace CleanUpAtStartup
{
  internal class Program
  {
    private static void Main()
    {
      Action<string> display = Console.WriteLine;
      string temporaryDirectory = "c:\\temp";

      if (!Directory.Exists(temporaryDirectory))
      {
        return;
      }

      RecursiveDelete(new DirectoryInfo(temporaryDirectory));

      Console.WriteLine($"Mode admin local {IsAdmin()}");
      int numberOfFileDeleted = 0;
      List<string> listOfFileDeleted = new List<string>();
      int numberOfFileNotDeleted = 0;
      List<string> listOfFileNotDeleted = new List<string>();

      foreach (var file in GetAllLargeFilesWithLinq(temporaryDirectory))
      {
        try
        {
          File.Delete(file.FullName);
          numberOfFileDeleted++;
          listOfFileDeleted.Add(file.FullName);
        }
        catch (Exception)
        {
          // do nothing and continue
          numberOfFileNotDeleted++;
          listOfFileNotDeleted.Add(file.FullName);
        }
      }

      display($"There have been {numberOfFileDeleted} files deleted");
      display(string.Empty);
      display($"There have been {numberOfFileNotDeleted} files not deleted");
      display(string.Empty);

      int numberOfDirectoriesDeleted = 0;
      List<string> listOfDirectoriesDeleted = new List<string>();
      int numberOfDirectoriesNotDeleted = 0;
      List<string> listOfDirectoriesNotDeleted = new List<string>();
      foreach (var dir in GetAllDirectoriesWithLinq(temporaryDirectory))
      {
        try
        {
          Directory.Delete(dir.FullName, true);
          numberOfDirectoriesDeleted++;
          listOfDirectoriesDeleted.Add(dir.FullName);
        }
        catch (Exception exception)
        {
          // do nothing and continue
          numberOfDirectoriesNotDeleted++;
          listOfDirectoriesNotDeleted.Add($"{dir.FullName} error: {exception.Message}");
        }
      }

      display($"there have been {numberOfDirectoriesDeleted} directories deleted");
      display(string.Empty);
      display($"there have been {numberOfDirectoriesNotDeleted} directories not deleted");
      display(string.Empty);

      // for each sub-directories, try to delete every files
      if (Directory.Exists(temporaryDirectory))
      {
        string pattern = "*.*";
        temporaryDirectory = "c:\\temp";
        DeleteFiles(temporaryDirectory, pattern);

        // Deleting thumbs.db
        pattern = "thumbs.db";
        temporaryDirectory = "C:\\";
        DeleteFiles(temporaryDirectory, pattern);

        // Deleting sub-directories in c:\temp
        pattern = "*.*";
        temporaryDirectory = "c:\\temp";
        DeleteDirectories(temporaryDirectory);

        // Delete MRU in registry
        // TODO add code

        Console.ForegroundColor = ConsoleColor.White;
        display("Press any key to exit:");
        //Console.ReadKey();
      }
    }

    private static void DeleteDirectories(string temporaryDirectory)
    {
      int numberOfDirectoryDeleted = 0;
      int numberOfDirectoryNotDeleted = 0;
      Action<string> display = Console.WriteLine;
      foreach (string directory in Directory.GetDirectories(temporaryDirectory))
      {
        try
        {
          Directory.Delete(directory);
          numberOfDirectoryDeleted++;
        }
        catch (Exception)
        {
          numberOfDirectoryNotDeleted++;
        }
      }

      Console.ForegroundColor = ConsoleColor.White;
      display("Directory deletion");
      Console.ForegroundColor = ConsoleColor.Green;
      display($"Directories deleted: {numberOfDirectoryDeleted}");
      Console.ForegroundColor = ConsoleColor.Red;
      display($"Directories not deleted: {numberOfDirectoryNotDeleted}");
    }

    private static void DeleteFiles(string temporaryDirectory, string pattern)
    {
      Action<string> display = Console.WriteLine;
      int numberOfFileDeleted = 0;
      int numberOfFileNotDeleted = 0;
      foreach (var file in GetFiles(temporaryDirectory, pattern))
      {
        try
        {
          File.Delete(file);
          numberOfFileDeleted++;
          Console.ForegroundColor = ConsoleColor.Green;
          display($"File deleted: {file}");
        }
        catch (Exception)
        {
          // do nothing, just go on to the next file
          Console.ForegroundColor = ConsoleColor.Red;
          display($"File cannot be deleted: {file}");
          numberOfFileNotDeleted++;
        }
      }

      Console.ForegroundColor = ConsoleColor.White;
      display($"pattern to delete: {pattern}");
      Console.ForegroundColor = ConsoleColor.Green;
      display($"{numberOfFileDeleted} files have been deleted");
      Console.ForegroundColor = ConsoleColor.Red;
      display($"{numberOfFileNotDeleted} files have not been deleted");
    }

    public static List<FileInfo> SearchFiles(List<string> patternsList)
    {
      var files = new List<FileInfo>();
      foreach (DriveInfo drive in DriveInfo.GetDrives().Where(drive => drive.DriveType != DriveType.CDRom).Where(drive => drive.DriveType != DriveType.Network).Where(drive => drive.DriveType != DriveType.Removable))
      {
        var dirs = from dir in drive.RootDirectory.EnumerateDirectories()
                   select new
                   {
                     ProgDir = dir,
                   };

        foreach (var di in dirs)
        {
          try
          {
            foreach (string patternItem in patternsList)
            {
              foreach (var fi in di.ProgDir.EnumerateFiles(patternItem, SearchOption.AllDirectories))
              {
                try
                {
                  files.Add(fi);
                }
                catch (UnauthorizedAccessException)
                {
                  // ignore
                }
              }
            }
          }
          catch (UnauthorizedAccessException)
          {
            // ignore
          }
        }
      }

      return files;
    }

    public static List<object> GetFilesAsObject(string initialDirectory)
    {
      DirectoryInfo DIRINF = new DirectoryInfo(initialDirectory);
      List<FileInfo> FINFO = DIRINF.GetFiles("*.*").ToList();
      List<object> Data = new List<object>();
      foreach (FileInfo FoundFile in FINFO)
      {
        var Name = FoundFile.Name; // Gets the name, MasterPlan.docx
        var Path = FoundFile.FullName; // Gets the full path C:\STAIRWAYTOHEAVE\GODSBACKUPPLANS\MasterPlan.docx
        var Extension = FoundFile.Extension; // Gets the extension .docx
        var Length = FoundFile.Length; // Used to get the file size in bytes, divide by the appropriate number to get actual size.

        // Make it into an object to store it into a list!
        var Item = new { Name = FoundFile.Name, Path = FoundFile.FullName, Size = FoundFile.Length, Extension = FoundFile.Extension };
        Data.Add(Item); // Store the item for use outside the loop.
      }

      return Data;
    }

    public static List<string> GetFiles(string initialDirectory, string pattern = "*.*")
    {
      DirectoryInfo DirectoryInfo = new DirectoryInfo(initialDirectory);
      List<FileInfo> listOfFileInfo = DirectoryInfo.GetFiles(pattern, SearchOption.TopDirectoryOnly).ToList();
      try
      {
        listOfFileInfo = DirectoryInfo.GetFiles(pattern, SearchOption.AllDirectories).ToList();
      }
      catch (Exception)
      {
      }

      List<string> listOfFiles = new List<string>();
      foreach (FileInfo FoundFile in listOfFileInfo)
      {
        string Item = $"{FoundFile.FullName}{FoundFile.Name}";
        listOfFiles.Add(Item);
      }

      return listOfFiles;
    }

    public static IEnumerable<FileInfo> GetAllLargeFilesWithLinq(string path)
    {
      var query = new DirectoryInfo(path).GetFiles()
                      .OrderByDescending(file => file.Length);
      return query;
    }

    public static IEnumerable<DirectoryInfo> GetAllDirectoriesWithLinq(string path)
    {
      return new DirectoryInfo(path).GetDirectories();
    }

    public static bool IsAdmin()
    {
      return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void RecursiveDelete(DirectoryInfo baseDir)
    {
      if (!baseDir.Exists)
      {
        return;
      }

      foreach (var dir in baseDir.EnumerateDirectories())
      {
        RecursiveDelete(dir);
      }

      var files = baseDir.GetFiles();
      foreach (var file in files)
      {
        try
        {
          file.IsReadOnly = false;
          file.Delete();
        }
        catch (Exception)
        {
        }
      }

      try
      {
        baseDir.Delete();
      }
      catch (Exception)
      {
      }
    }
  }
}
