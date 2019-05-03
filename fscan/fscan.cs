﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using WMPLib;
using System.Collections;
using System.Security.Cryptography;

namespace fscan
{
    class fscan
    {
        private static void print_help()
        {
            Console.WriteLine("");
            Console.WriteLine(" +===[ ABOUT ]");
            Console.WriteLine(" | ABOUT.....: file scanner/searcher");
            Console.WriteLine(" | AUTHOR....: 0xC0LD");
            Console.WriteLine(" | BUILT IN..: C# .NET");
            Console.WriteLine(" | VERSION...: 13");
            Console.WriteLine(" | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...");
            Console.WriteLine("");
            Console.WriteLine(" +===[ STANDARD OPTIONS ]");
            Console.WriteLine(" | -c = find duplicate files (by name)");
            Console.WriteLine(" | -e = find files that have same names but different extensions");
            Console.WriteLine(" | -i = find duplicate files (by md5 checksum hash)");
            Console.WriteLine(" | -v = find corrupt videos (uses ffmpeg)");
            Console.WriteLine(" | -s = find video files that have (no) sound/audio (uses ffmpeg)");
            Console.WriteLine(" |      (t = file with sound, f = file without sound)");
            Console.WriteLine(" |      (use -t to only print files with sound)");
            Console.WriteLine(" |      (use -f to only print files without sound)"); 
            Console.WriteLine(" |      (printed files will be sent to runtime options if specified)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]");
            Console.WriteLine(" | -a = also scan subdirectories"); 
            Console.WriteLine(" | -d = send the found file to recycle bin");
            Console.WriteLine(" | -m = move the found file to a folder (fscan_dir)");
            Console.WriteLine(" |   *if -d and -m are specified, in this case, the first option will be chosen (-d)");
            Console.WriteLine(" |   *if -m and -d are specified, in this case, the first option will be chosen (-m)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ PRINT (only) OPTIONS ]");
            Console.WriteLine(" | -la = print file sizes (ascending order)");
            Console.WriteLine(" | -ld = print file sizes (descending order)");
            Console.WriteLine(" | -f  = print files with over 260 characters in file path (too long)");
            Console.WriteLine(" | -ta = print video length (ascending order)");
            Console.WriteLine(" | -td = print video length (descending order)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ EXAMPLES ]");
            Console.WriteLine(" | > fscan -s -a -f -d");
            Console.WriteLine(" | (scan all directories (dirs + subdirs) and delete files with no sound)");
            Console.WriteLine(" | > fscan -c -a -d");
            Console.WriteLine(" | (scan all directories (dirs + subdirs) and delete duplicate files)");
            Console.WriteLine("");
        }

        private static bool DELETE = false;
        private static bool MOVE = false;
        private readonly static string MOVE_DIR = "fscan_dir";
        public static readonly List<string> VideoTypes = new List<string> { ".mp4", ".webm", ".avi", ".mov", ".mkv", ".flv", ".mpeg", ".mpg", ".wmv", ".mp3", ".ogg" };
        private static System.IO.SearchOption mode = System.IO.SearchOption.TopDirectoryOnly;

        //mute option
        private static bool ONLY_TRUE = false;
        private static bool ONLY_FALSE = false;

        private static int ScannedCount = 0;

        struct aFile
        {
            public string numb;
            public string desc;
            public string path;
        }
        
        static int Main(string[] args)
        {
            if (args.Length == 0) { print_help(); return 1; }
            if (args.Length == 1)
            {
                if (args[0] == "--help"
                || args[0] == "-help"
                || args[0] == "help"
                || args[0] == "--h"
                || args[0] == "-h"
                || args[0] == "h"
                || args[0] == "/?"
                )
                {
                    print_help(); return 0;
                }

                if (File.Exists(args[0]))
                {
                    scan_single_file(args[0]);
                    return 1;
                }
            }
            
            //check for process options
            bool ignore = false;
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-a": { mode = System.IO.SearchOption.AllDirectories; break; }
                    case "-d": { if (!ignore) { ignore = true; DELETE = true; } break; }
                    case "-m": { if (!ignore) { ignore = true; MOVE = true; } break; }
                }
            }

            //scan options
            foreach (string arg in args)
            {
                switch (arg)
                {
                    case "-c": { return option_find_dupes(); }
                    case "-e": { return option_find_dupes_noext(); }
                    case "-i": { return option_find_dupes_md5hash(); }
                    case "-v":
                        {
                            foreach (string arg_ in args)
                            {
                                switch (arg_)
                                {
                                    case "-t": { ONLY_TRUE = true;  break; }
                                    case "-f": { ONLY_FALSE = true; break; }
                                }
                            }

                            return option_find_unplayablevideos();
                        }
                    case "-s":
                        {
                            foreach (string arg_ in args)
                            {
                                switch (arg_)
                                {
                                    case "-t": { ONLY_TRUE = true;  break; }
                                    case "-f": { ONLY_FALSE = true; break; }
                                }
                            }

                            return option_find_mutes();
                        }
                    case "-la": { return option_print_size(false); }
                    case "-ld": { return option_print_size(true); }
                    case "-f":  { return option_print_longnames(); }
                    case "-ta": { return option_print_duration(false); }
                    case "-td": { return option_print_duration(true); }
                }
            }

            return 0;
        }
        
        private static int scan_single_file(string file)
        {
            if (file.Length >= 260)
            {
                Console.WriteLine("file name too long... must be less than 260 characters...");
                return 1;
            }
            else
            {
                FileInfo fi = new FileInfo(file);

                Console.WriteLine("");
                Console.WriteLine("> file name.........: " + fi.Name);
                Console.WriteLine("> file path.........: " + fi.FullName);
                Console.WriteLine("> directory.........: " + fi.Directory);
                Console.WriteLine("> file size.........: " + ROund(fi.Length) + " (" + fi.Length + " bytes)");
                Console.WriteLine("> creation time.....: " + fi.CreationTime);
                Console.WriteLine("> last access time..: " + fi.LastAccessTime);
                Console.WriteLine("> last write time...: " + fi.LastWriteTime);
                
                switch (fi.Extension)
                {
                    case ".mp4": 
                    case ".webm":
                    case ".avi": 
                    case ".mov": 
                    case ".mkv": 
                    case ".flv": 
                    case ".mpeg":
                    case ".mpg": 
                    case ".wmv":
                        {
                            Process ffmpeg = new Process();
                            ffmpeg.StartInfo.UseShellExecute = false;
                            ffmpeg.StartInfo.RedirectStandardOutput = true;
                            ffmpeg.StartInfo.RedirectStandardError = true;
                            ffmpeg.StartInfo.FileName = "ffmpeg.exe";
                            ffmpeg.StartInfo.Arguments = "-v error -i " + "\"" + file + "\"" + " -f null -";
                            ffmpeg.Start();


                            string ffmpeg_output = ffmpeg.StandardError.ReadToEnd();
                            ffmpeg.WaitForExit();

                            //empty = no errors
                            if (string.IsNullOrEmpty(ffmpeg_output))
                            {
                                Console.WriteLine("> is playable.......: true");
                            }
                            else
                            {
                                Console.WriteLine("> is playable.......: false");
                            }

                            Process ffprobe = new Process();
                            ffprobe.StartInfo.UseShellExecute = false;
                            ffprobe.StartInfo.RedirectStandardOutput = true;
                            ffprobe.StartInfo.RedirectStandardError = true;
                            ffprobe.StartInfo.FileName = "ffprobe.exe";
                            ffprobe.StartInfo.Arguments = "-i " + "\"" + file + "\"" + " -show_streams -select_streams a -loglevel error";
                            ffprobe.Start();

                            string ffprobe_output = ffprobe.StandardOutput.ReadToEnd();
                            ffprobe.WaitForExit();

                            //empty = no sound
                            if (string.IsNullOrEmpty(ffprobe_output))
                            {
                                Console.WriteLine("> has audio.........: false");
                            }
                            else
                            {
                                Console.WriteLine("> has audio.........: true");
                            }

                            break;
                        }
                }
            }
            return 0;
        }
        
        private static int option_find_dupes()
        {
            Console.WriteLine("> scan option: -c");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);
            List<string> files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode).ToList();
            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();
            
            int count = 0;
            Hashtable ht_filenames = new Hashtable();
            foreach (string file1 in files)
            {
                string filename1 = GetFileName(file1);
                if (ht_filenames.ContainsKey(filename1))
                {
                    //get file2 from hash table (file that has been added earlier)
                    string file2 = string.Empty;
                    foreach(string k in ht_filenames.Keys) { if (k == filename1) { file2 = ht_filenames[k].ToString(); break; } }

                    count++;
                    Console.WriteLine(count + ": " + file2);
                    Console.WriteLine(count + ": " + file1);
                    if      (DELETE) { file_del(file1); }
                    else if (MOVE)   { file_mov(file1); }
                    Console.WriteLine("");

                } else { ht_filenames.Add(filename1, file1); }

                ScannedCount++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_noext()
        {
            Console.WriteLine("> scan option: -e");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);
            List<string> files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode).ToList();
            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable ht_filenames = new Hashtable();
            foreach (string file1 in files)
            {
                FileInfo fi1 = new FileInfo(file1);
                string filename1 = fi1.Name.Replace(fi1.Extension, ""); //get name without extension
                if (ht_filenames.ContainsKey(filename1))
                {
                    //get file2 from hash table (file that has been added earlier)
                    string file2 = string.Empty;
                    foreach (string k in ht_filenames.Keys) { if (k == filename1) { file2 = ht_filenames[k].ToString(); break; } }

                    count++;
                    Console.WriteLine(count + ": " + file2);
                    Console.WriteLine(count + ": " + file1);
                    if (DELETE) { file_del(file1); }
                    else if (MOVE) { file_mov(file1); }
                    Console.WriteLine("");

                }
                else { ht_filenames.Add(filename1, file1); }

                ScannedCount++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_md5hash()
        {
            Console.WriteLine("> scan option: -i");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);
            
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            
            Console.WriteLine("> found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            Console.WriteLine("> starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable ht_filenames = new Hashtable();
            foreach (string file1 in files)
            {
                string hash1 = CalculateMD5(file1);
                if (ht_filenames.ContainsKey(hash1))
                {
                    //get file2 from hash table (file that has been added earlier)
                    string file2 = string.Empty;
                    foreach (string k in ht_filenames.Keys) { if (k == hash1) { file2 = ht_filenames[k].ToString(); break; } }

                    count++;
                    Console.WriteLine(count + ": " + file2 + " (" + hash1 + ")");
                    Console.WriteLine(count + ": " + file1 + " (" + hash1 + ")");
                    if (DELETE) { file_del(file1); }
                    else if (MOVE) { file_mov(file1); }
                    Console.WriteLine("");

                }
                else { ht_filenames.Add(hash1, file1); }

                ScannedCount++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }

        private static int option_find_mutes()
        {
            Console.WriteLine("> scan option: -s");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);

            //get files
            List<string> files = new List<string>();
            foreach (string type in VideoTypes)
            { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*" + type, mode)) { files.Add(file); } }

            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> T = true (video has sound)");
            Console.WriteLine("> F = false (video doesn't have sound)");
            Console.WriteLine("> scanning for audio...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            foreach (string file in files)
            {
                Process ffprobe = new Process();

                try
                {
                    ffprobe.StartInfo.UseShellExecute = false;
                    ffprobe.StartInfo.RedirectStandardOutput = true;
                    ffprobe.StartInfo.RedirectStandardError = true;
                    ffprobe.StartInfo.FileName = "ffprobe.exe";
                    ffprobe.StartInfo.Arguments = "-i " + "\"" + file + "\"" + " -show_streams -select_streams a -loglevel error";
                    ffprobe.Start();
                }
                catch (System.ComponentModel.Win32Exception) { Console.WriteLine("ffprobe.exe not found..."); return 1; }
                catch (Exception)                            { Console.WriteLine("ERR[Exception]"); return 1; }

                string output = ffprobe.StandardOutput.ReadToEnd();
                ffprobe.WaitForExit();

                //empty = no sound
                if (!string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file);
                        if (DELETE)
                        {
                            file_del(file);
                        }
                        else if (MOVE)
                        {
                            file_mov(file);
                        }
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file);
                        if (DELETE)
                        {
                            file_del(file);
                        }
                        else if (MOVE)
                        {
                            file_mov(file);
                        }
                    }
                }

                ScannedCount++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_unplayablevideos()
        {
            Console.WriteLine("> scan option: -v");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);

            //get files
            List<string> files = new List<string>();
            foreach (string type in VideoTypes)
            { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*" + type, mode)) { files.Add(file); } }

            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> T = true (video is playable)");
            Console.WriteLine("> F = false (video is unplayable)");
            Console.WriteLine("> scanning for playable/corrupted videos...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            foreach (string file in files)
            {

                Process ffmpeg = new Process();

                try
                {
                    // load whole thing, slow = ffmpeg -v error -i FILENAME.mp4 -f null -
                    // load last 60 s, fast   = ffmpeg -v error -sseof -60 -i FILENAME.mp4 -f null -
                    ffmpeg.StartInfo.UseShellExecute = false;
                    ffmpeg.StartInfo.RedirectStandardOutput = true;
                    ffmpeg.StartInfo.RedirectStandardError = true;
                    ffmpeg.StartInfo.FileName = "ffmpeg.exe";
                    ffmpeg.StartInfo.Arguments = "-v error -sseof -60 -i " + "\"" + file + "\"" + " -f null -";
                    ffmpeg.Start();
                }
                catch (System.ComponentModel.Win32Exception) { Console.WriteLine("ffmpeg.exe not found...");  return 1; }
                catch (Exception) { Console.WriteLine("ERR[Exception]"); return 1; }
                
                string output = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
                
                //empty = no errors
                if (string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file);
                        if (DELETE)
                        {
                            file_del(file);
                        }
                        else if (MOVE)
                        {
                            file_mov(file);
                        }
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file);
                        if (DELETE)
                        {
                            file_del(file);
                        }
                        else if (MOVE)
                        {
                            file_mov(file);
                        }
                    }
                }

                ScannedCount++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        
        private static void print_info()
        {
            while (true)
            {
                Console.ReadKey(true);
                Console.WriteLine(": tested " + ScannedCount + " file(s)");
            }
            
        }
        private static void print_info_end()
        {
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("> scanned " + ScannedCount + " file(s)");
        }
        

        private static int option_print_longnames()
        {
            Console.WriteLine("> print option: -l");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);
            List<string> files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode).ToList();
            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> searching for \"path too long\" file names...");
            Console.WriteLine("");

            int count = 0;
            foreach (string file in files)
            {
                if (file.Length >= 260)
                {
                    count++;
                    Console.WriteLine(count + ": " +file);
                }
}

            if (count == 0)
            {
                Console.WriteLine("... everything seems to be alright...");
            }

            return 0;
        }
        private static int option_print_size(bool descend = false)
        {
            if (descend)
            {
                Console.WriteLine("> print option: -ld");
            }
            else
            {
                Console.WriteLine("> print option: -la");
            }
            
            Console.WriteLine("> path: " + Environment.CurrentDirectory);

            string[] files = Directory.EnumerateFiles(Environment.CurrentDirectory, "*.*", mode).ToArray();

            Console.WriteLine("> found " + files.Length + " file(s)");
            Console.WriteLine("> sorting by size....");
            Console.WriteLine("");

            try
            {
                if (descend)
                {
                    files = files.OrderByDescending(f => new FileInfo(f).Length).ToArray();
                }
                else
                {
                    files = files.OrderBy(f => new FileInfo(f).Length).ToArray();
                }
            }
            catch (PathTooLongException)
            {
                Console.WriteLine("There are files with names over 260 characters...");
                Console.WriteLine("... use the -l option to find them");
                return 1;
            }
            catch (ArgumentNullException) { Console.WriteLine("ERR[ArgumentNullException]"); return 1; }
            catch (System.Security.SecurityException) { Console.WriteLine("ERR[SecurityException]"); return 1; }
            catch (ArgumentException) { Console.WriteLine("ERR[ArgumentException]"); return 1; }
            catch (UnauthorizedAccessException) { Console.WriteLine("ERR[UnauthorizedAccessException]"); return 1; }
            catch (NotSupportedException) { Console.WriteLine("ERR[NotSupportedException]"); return 1; }
            catch (Exception) { Console.WriteLine("ERR[Exception]"); return 1; }

            int longest_numb = 4;
            int longest_desc = 4;
            int count = 0;
            List<aFile> files_ = new List<aFile>();
            foreach (string file in files)
            {
                count++;

                //get file info
                FileInfo fi = new FileInfo(file);

                //make file
                aFile fi_ = new aFile() { path = fi.FullName, numb = count.ToString() + ".", desc = ROund(fi.Length) + " (" + fi.Length + " bytes)" };

                //add
                files_.Add(fi_);

                //get longest size char len
                if (fi_.desc.Length > longest_desc) { longest_desc = fi_.desc.Length; }
                if (fi_.numb.Length > longest_numb) { longest_numb = fi_.numb.Length; }
            }
            
            string format = "{0,-" + longest_numb + "} {1,-"+ longest_desc + "} {2,0}";

            Console.WriteLine(format, "Numb", "Size", "Path");
            
            foreach (aFile file in files_)
            {
                Console.WriteLine(format, file.numb, file.desc, file.path);
            }
            
            return 0;
        }
        private static int option_print_duration(bool descend = false)
        {
            Console.WriteLine("> scan option: -td");
            Console.WriteLine("> path: " + Environment.CurrentDirectory);

            //get files
            List<string> files = new List<string>();
            foreach (string type in VideoTypes)
            { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*" + type, mode)) { files.Add(file); } }

            Console.WriteLine("> found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("> sorting by video length...");
            Console.WriteLine("");
            
            try
            {
                if (descend)
                {
                    files = files.OrderByDescending(f => new WindowsMediaPlayer().newMedia(f).duration).ToList();
                }
                else
                {
                    files = files.OrderBy(f => new WindowsMediaPlayer().newMedia(f).duration).ToList();
                }
            }
            catch (Exception e)
            {
                Console.Write("ERR[Exception]: " + e.Message);
                return 1;
            }
            
            int longest_numb = 4;
            int longest_desc = 6;
            int count = 0;
            List<aFile> files_ = new List<aFile>();
            foreach (string file in files)
            {
                count++;
                
                //make file
                aFile fi_ = new aFile() { path = file, numb = count.ToString() + ".", desc = ROund_time(new WindowsMediaPlayer().newMedia(file).duration) };

                //add
                files_.Add(fi_);

                //get longest size char len
                if (fi_.desc.Length > longest_desc) { longest_desc = fi_.desc.Length; }
                if (fi_.numb.Length > longest_numb) { longest_numb = fi_.numb.Length; }
            }
            
            string format = "{0,-" + longest_numb + "} {1,-" + longest_desc + "} {2,0}";

            Console.WriteLine(format, "Numb", "length", "Path");

            foreach (aFile file in files_)
            {
                Console.WriteLine(format, file.numb, file.desc, file.path);
            }

            return 0;
        }
        

        private static void file_mov(string file)
        {
            try
            {
                FileInfo fi = new FileInfo(file);

                string move_here = fi.FullName.Replace(fi.Name, "") + MOVE_DIR;
                Directory.CreateDirectory(move_here);

                File.Move(fi.FullName, move_here + "\\" + fi.Name);

                Console.WriteLine("mov > " + fi.FullName + " -> " + move_here + "\\" + fi.Name);
            }
            catch (ArgumentNullException)             { Console.WriteLine("ERR[ArgumentNullException]"); }
            catch (System.Security.SecurityException) { Console.WriteLine("ERR[SecurityException]"); }
            catch (ArgumentException)                 { Console.WriteLine("ERR[ArgumentException]"); }
            catch (UnauthorizedAccessException)       { Console.WriteLine("ERR[UnauthorizedAccessException]"); }
            catch (PathTooLongException)              { Console.WriteLine("ERR[PathTooLongException]"); }
            catch (NotSupportedException)             { Console.WriteLine("ERR[NotSupportedException]"); }
            catch (DirectoryNotFoundException)        { Console.WriteLine("ERR[DirectoryNotFoundException]"); }
            catch (IOException e)                     { Console.WriteLine("ERR[IOException] " + e.Message); }
            catch (Exception)                         { Console.WriteLine("ERR[Exception]"); }
        }
        private static void file_del(string file)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);

                Console.WriteLine("del > " + file);
            }
            catch (ArgumentNullException) { Console.WriteLine("ERR[ArgumentNullException]"); }
            catch (ArgumentException) { Console.WriteLine("ERR[ArgumentException]"); }
            catch (PathTooLongException) { Console.WriteLine("ERR[PathTooLongException]"); }
            catch (NotSupportedException) { Console.WriteLine("ERR[NotSupportedException]"); }
            catch (FileNotFoundException) { Console.WriteLine("ERR[FileNotFoundException]: " + file); }
            catch (IOException) { Console.WriteLine("ERR[IOException]"); }
            catch (System.Security.SecurityException) { Console.WriteLine("ERR[SecurityException]"); }
            catch (UnauthorizedAccessException) { Console.WriteLine("ERR[UnauthorizedAccessException]"); }
            catch (Exception) { Console.WriteLine("ERR[Exception]"); }
        }
        public static string ROund(double bytes)
        {
            // 1 Byte = 8 Bit
            // 1 Kilobyte = 1024 Bytes
            // 1 Megabyte = 1048576 Bytes
            // 1 Gigabyte = 1073741824 Bytes
            // 1 Terabyte = 1099511627776 Bytes

            string num_double_string = bytes + " B";

            if (bytes > 1099511627776) //TB
            {
                bytes = bytes / 1099511627776;
                num_double_string = Math.Round(bytes, 2) + " TB";
            }
            else if (bytes > 1073741824) //GB
            {
                bytes = bytes / 1073741824;
                num_double_string = Math.Round(bytes, 2) + " GB";
            }
            else if (bytes > 1048576) //MB
            {
                bytes = bytes / 1048576;
                num_double_string = Math.Round(bytes, 2) + " MB";
            }
            else if (bytes > 1024) //KB
            {
                bytes = bytes / 1024;
                num_double_string = Math.Round(bytes, 2) + " KB";
            }
            else
            {
                num_double_string = bytes + " B";
            }

            return num_double_string;
        }
        public static string ROund_time(double time)
        {
            string num_double_string = time + " s";
            
            
            num_double_string = TimeSpan.FromSeconds(time).ToString(@"hh\:mm\:ss");


            return num_double_string;
        }
        private static string GetTime()
        {
            TimeSpan diff = (new DateTime(2011, 02, 10) - new DateTime(2011, 02, 01));
            return diff.TotalMilliseconds.ToString();
        }
        private static string CalculateMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        private static string GetFileName(string path)
        {
            string[] index = path.Split('\\');
            return index[index.Length - 1];
        }
        
    }
}
