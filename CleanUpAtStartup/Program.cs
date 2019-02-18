﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

      // for each sub-directories, try to delete every files
      //try
      //{
      if (Directory.Exists(temporaryDirectory))
      {
        //foreach (string file in Directory.GetFiles(temporaryDirectory))
        //{
        //  //FileInfo flinfo = new FileInfo(file);
        //  try
        //  {
        //    File.Delete(file);
        //  }
        //  catch (Exception)
        //  {
        //  }

        //}

        //foreach (string directory in Directory.GetDirectories(temporaryDirectory))
        //{
        //  //DirectoryInfo drinfo = new DirectoryInfo(directory);
        //  try
        //  {
        //    Directory.Delete(directory);
        //  }
        //  catch (Exception)
        //  {
        //  }
        //}

        string pattern = "*.*";
        temporaryDirectory = "c:\\temp";
        DeleteFiles(temporaryDirectory, pattern);

        // Deleting thumbs.db
        pattern = "thumbs.db";
        temporaryDirectory = "C:\\";
        DeleteFiles(temporaryDirectory, pattern);

        Console.ForegroundColor = ConsoleColor.Blue;
        display("Press any key to exit:");
        Console.ReadKey();
      }
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
      display($"{numberOfFileDeleted} files have been deleted");
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
  }
}
