using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Security.Cryptography;
using System.Drawing;
using Microsoft.VisualBasic.FileIO;
using System.Security;
using WMPLib;

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
            Console.WriteLine(" | BUILT IN..: VS C# .NET");
            Console.WriteLine(" | VERSION...: 22");
            Console.WriteLine(" | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...");
            Console.WriteLine("");
            Console.WriteLine(" +===[ STANDARD OPTIONS ]");
            Console.WriteLine(" | name    = find duplicate files by name.ext");
            Console.WriteLine(" | noext   = find duplicate files by name");
            Console.WriteLine(" | hash    = find duplicate files by md5 checksum hash");
            Console.WriteLine(" | hashbuf = find duplicate files by md5 checksum hash, but use a larger byte buffer (1 mil bytes)");
            Console.WriteLine(" | hashexe = find duplicate files by md5 checksum hash, but use the 'md5sum.exe' to get the file hash");
            Console.WriteLine(" | byte    = find files that have the same byte size");
            Console.WriteLine(" | pic     = find duplicate images (*.jpg, *.jpeg, *.png, *.bmp)");
            Console.WriteLine(" | vid     = find corrupt and playable videos (uses ffmpeg)");
            Console.WriteLine(" | vidt    = find playable video files (uses ffmpeg)");
            Console.WriteLine(" | vidf    = find corrupt video files (uses ffmpeg)");
            Console.WriteLine(" | sound   = print video files that have, and don't have sound/audio (uses ffprobe)");
            Console.WriteLine(" | soundt  = find video files that have sound/audio (uses ffprobe)");
            Console.WriteLine(" | soundf  = find video files that don't have sound/audio (uses ffprobe)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]");
            Console.WriteLine(" | all = also scan subdirectories");
            Console.WriteLine(" | 1   = use first file (del/mov/...)");
            Console.WriteLine(" | 2   = use second file (del/mov/...) (default)");
            Console.WriteLine(" | del = send the found file to recycle bin");
            Console.WriteLine(" | mov = move the found file to a folder (fscan_dir)");
            Console.WriteLine(" | v   = print status every second");
            Console.WriteLine(" | vv  = print status every 500 ms");
            Console.WriteLine(" | vvv = print status every 250 ms");
            Console.WriteLine("");
            Console.WriteLine(" +===[ PRINT (only) OPTIONS ]");
            Console.WriteLine(" | sizea    = print file sizes in ascending order");
            Console.WriteLine(" | sized    = print file sizes in descending order");
            Console.WriteLine(" | dsizea   = print directory size in ascending order");
            Console.WriteLine(" | dsized   = print directory size in descending order");
            Console.WriteLine(" | dcounta  = print directory files count in ascending order");
            Console.WriteLine(" | dcountd  = print directory files count in descending order");
            Console.WriteLine(" | rdcounta = print directory (+subdirs) files count in ascending order");
            Console.WriteLine(" | rdcountd = print directory (+subdirs) files count in descending order");
            Console.WriteLine(" | long     = print files with over 260 characters in file path (too long)");
            Console.WriteLine(" | lena     = print video length in ascending order");
            Console.WriteLine(" | lend     = print video length in descending order");
            Console.WriteLine("");
            
        }

        private static bool DELETE = false;
        private static bool MOVE = false;
        private static bool VERBOSE = false; private static int VERBOSE_DELAY = 1000;
        private readonly static string MOVE_DIR = "fscan_dir";
        public static readonly List<string> VideoTypes = new List<string> { ".mp4", ".webm", ".avi", ".mov", ".mkv", ".flv", ".mpeg", ".mpg", ".wmv", ".mp3", ".ogg" };
        private static System.IO.SearchOption mode = System.IO.SearchOption.TopDirectoryOnly;
        
        private static bool ONLY_TRUE = false;
        private static bool ONLY_FALSE = false;
        private static bool useSecondItem = true;
        private static int tested = 0;

        private static DateTime currentTime = new DateTime();

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
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "all": { mode = System.IO.SearchOption.AllDirectories; break; }
                    case "del": { DELETE = true; break; }
                    case "mov": { MOVE   = true; break; }
                    case "1":   { useSecondItem = false; break; }
                    case "2":   { useSecondItem = true;  break; }
                    case "v":   { VERBOSE = true; VERBOSE_DELAY = 1000; break; }
                    case "vv":  { VERBOSE = true; VERBOSE_DELAY = 500;  break; }
                    case "vvv": { VERBOSE = true; VERBOSE_DELAY = 250;  break; }
                }
            }

            currentTime = DateTime.Now;

            int ret = new Func<int>(() => {

                //scan options
                foreach (string arg in args)
                {
                    switch (arg.ToLower())
                    {
                        case "name":     {                    return option_find_dupes();                }
                        case "noext":    {                    return option_find_dupes_noext();          }
                        case "hash":     {                    return option_find_dupes_md5hash(0);       }
                        case "hashbuf":  {                    return option_find_dupes_md5hash(1);       }
                        case "hashexe":  {                    return option_find_dupes_md5hash(2);       }
                        case "byte":     {                    return option_find_dupes_byte();           }
                        case "pic":      {                    return option_find_dupes_img();            }
                        case "vid":      {                    return option_find_unplayablevideos();     }
                        case "vidt":     { ONLY_TRUE = true;  return option_find_unplayablevideos();     }
                        case "vidf":     { ONLY_FALSE = true; return option_find_unplayablevideos();     }
                        case "sound":    {                    return option_find_mutes();                }
                        case "soundt":   { ONLY_TRUE = true;  return option_find_mutes();                }
                        case "soundf":   { ONLY_FALSE = true; return option_find_mutes();                }
                        case "sizea":    {                    return option_print_size(false);           }
                        case "sized":    {                    return option_print_size(true);            }
                        case "dsizea":   {                    return option_print_dirSize(false);        }
                        case "dsized":   {                    return option_print_dirSize(true);         }
                        case "dcounta":  {                    return option_print_dirCount(false);       }
                        case "dcountd":  {                    return option_print_dirCount(true);        }
                        case "rdcounta": {                    return option_print_dirCount(false, true); }
                        case "rdcountd": {                    return option_print_dirCount(true, true);  } 
                        case "long":     {                    return option_print_longnames();           }
                        case "lena":     {                    return option_print_duration(false);       }
                        case "lend":     {                    return option_print_duration(true);        }
                    }
                }

                Console.WriteLine("Invalid option.");
                return 1;
            })();

            

            return ret;
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
                Console.WriteLine("# file name.........: " + fi.Name);
                Console.WriteLine("# file path.........: " + fi.FullName);
                Console.WriteLine("# directory.........: " + fi.Directory);
                Console.WriteLine("# file size.........: " + ROund(fi.Length) + " (" + fi.Length + " bytes)");
                Console.WriteLine("# creation time.....: " + fi.CreationTime);
                Console.WriteLine("# last access time..: " + fi.LastAccessTime);
                Console.WriteLine("# last write time...: " + fi.LastWriteTime);
                
                foreach(string t in VideoTypes)
                {
                    if (fi.Extension == t)
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
                        if (string.IsNullOrEmpty(ffmpeg_output)) { Console.WriteLine("# is playable.......: true"); }
                        else { Console.WriteLine("# is playable.......: false"); }

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
                        if (string.IsNullOrEmpty(ffprobe_output)) { Console.WriteLine("# has audio.........: false"); }
                        else                                      { Console.WriteLine("# has audio.........: true"); }

                        break;
                    }
                }
                
            }
            return 0;
        }
        
        private static int option_find_dupes()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            Console.WriteLine("# found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();
            
            int count = 0;
            Hashtable table = new Hashtable();
            foreach (string file1 in files)
            {
                string filename1 = GetFileName(file1);
                if (table.ContainsKey(filename1))
                {
                    // get file2 from hash table (file that has been added earlier)
                    string file2 = table[filename1].ToString();

                    count++;
                    Console.WriteLine(count + ": " + file1);
                    Console.WriteLine(count + ": " + file2);
                    processFile2(file1, file2);
                    Console.WriteLine("");

                } else { table.Add(filename1, file1); }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_noext()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            Console.WriteLine("# found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable table = new Hashtable();
            foreach (string file1 in files)
            {
                FileInfo fi1 = new FileInfo(file1);
                string filename1 = fi1.Name.Replace(fi1.Extension, ""); //get name without extension
                if (table.ContainsKey(filename1))
                {
                    // get file2 from hash table (file that has been added earlier)
                    string file2 = table[filename1].ToString();

                    count++;
                    Console.WriteLine(count + ": " + file1);
                    Console.WriteLine(count + ": " + file2);
                    processFile2(file1, file2);
                    Console.WriteLine("");

                }
                else { table.Add(filename1, file1); }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_md5hash(int hashMode)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            Console.WriteLine("# found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable table = new Hashtable();
            foreach (string file1 in files)
            {
                string hash1 = "ERROR";
                switch (hashMode)
                {
                    case 0: hash1 = CalculateMD5(file1);          break;
                    case 1: hash1 = CalculateMD5fast(file1);      break;
                    case 2: hash1 = CalculateMD5withProc(file1);  break;
                }

                if (hash1 == "ERROR") { Console.WriteLine("# ERROR: something failed..."); return 1; }

                if (table.ContainsKey(hash1))
                {
                    // get file2 from hash table (file that has been added earlier)
                    string file2 = table[hash1].ToString();

                    count++;
                    Console.WriteLine(count + ": " + file1 + " (" + hash1 + ")");
                    Console.WriteLine(count + ": " + file2 + " (" + hash1 + ")");
                    processFile2(file1, file2);
                    Console.WriteLine("");

                }
                else { table.Add(hash1, file1); }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_byte()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            Console.WriteLine("# found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable table = new Hashtable();
            foreach (string file1 in files)
            {
                string size1 = new FileInfo(file1).Length.ToString();
                if (table.ContainsKey(size1))
                {
                    // get file2 from hash table (file that has been added earlier)
                    string file2 = table[size1].ToString();

                    count++;
                    Console.WriteLine(count + ": " + file1 + " (" + size1 + ")");
                    Console.WriteLine(count + ": " + file2 + " (" + size1 + ")");
                    processFile2(file1, file2);
                    Console.WriteLine("");

                }
                else { table.Add(size1, file1); }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_dupes_img()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(Environment.CurrentDirectory, "*.png", mode));
            files.AddRange(Directory.GetFiles(Environment.CurrentDirectory, "*.jpg", mode));
            files.AddRange(Directory.GetFiles(Environment.CurrentDirectory, "*.jpeg", mode));

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            Hashtable table = new Hashtable();
            foreach (string file1 in files)
            {
                Image img = Image.FromFile(file1);
                Bitmap bmp = new Bitmap(img, new Size(16, 16));
                string hash1 = GetImgHash(bmp);
                bmp.Dispose();
                img.Dispose();
                if (table.ContainsKey(hash1))
                {
                    // get file2 from hash table (file that has been added earlier)
                    string file2 = table[hash1].ToString();

                    count++;
                    Console.WriteLine(count + ": " + file1);
                    Console.WriteLine(count + ": " + file2);
                    processFile2(file1, file2);
                    Console.WriteLine("");

                }
                else { table.Add(hash1, file1); }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }

        private static int option_find_mutes()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            //get files
            List<string> files = new List<string>();
            foreach (string type in VideoTypes)
            { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*" + type, mode)) { files.Add(file); } }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("# scanning for audio...");
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
                catch (System.ComponentModel.Win32Exception e) { Console.WriteLine("# ERROR: ffprobe.exe : " + e.Message); return 1; }
                catch (Exception e)                            { Console.WriteLine("ERR[Exception]: "        + e.Message); return 1; }

                string output = ffprobe.StandardOutput.ReadToEnd();
                ffprobe.WaitForExit();

                //empty = no sound
                if (!string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file);
                        processFile(file);
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file);
                        processFile(file);
                    }
                }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        private static int option_find_unplayablevideos()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            //get files
            List<string> files = new List<string>();
            foreach (string type in VideoTypes)
            { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*" + type, mode)) { files.Add(file); } }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("# scanning for playable/corrupt videos...");
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
                catch (System.ComponentModel.Win32Exception e) { Console.WriteLine("# ERROR: ffmpeg.exe : " + e.Message); return 1; }
                catch (Exception e)                            { Console.WriteLine("ERR[Exception]: "        + e.Message); return 1; }
                
                string output = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
                
                //empty = no errors
                if (string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file);
                        processFile(file);
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file);
                        processFile(file);
                    }
                }

                tested++;
            }

            th.Abort();
            print_info_end();

            return 0;
        }
        
        private static void print_info()
        {
            while (true)
            {
                if (VERBOSE) { Thread.Sleep(VERBOSE_DELAY); }
                else         { Console.ReadKey(true);       }
                
                Console.WriteLine(": " + tested + " --- " + (DateTime.Now - currentTime).TotalMilliseconds.ToString() + " ms");
            }
        }
        private static void print_info_end()
        {
            Console.WriteLine();
            Console.WriteLine("# tested files..: " + tested);
            Console.WriteLine("# time taken....: " + (DateTime.Now - currentTime).TotalMilliseconds.ToString() + " ms");
        }
        
        private static int option_print_longnames()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            List<string> files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode).ToList();
            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            Console.WriteLine("# searching for \"path too long\" file names...");
            Console.WriteLine("");

            int count = 0;
            foreach (string file in files)
            {
                if (file.Length >= 260)
                {
                    count++;
                    Console.WriteLine(count + ": " + file);
                }
}

            if (count == 0)
            {
                Console.WriteLine("# 0 errors");
            }

            return 0;
        }
        private static int option_print_size(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            FileInfo[] procItems = new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.*", mode);

            Console.WriteLine("# found " + procItems.Length + " file(s)");
            Console.WriteLine("# sorting by size....");
            Console.WriteLine("");

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string, string>[] items = new Tuple<long, string, string, string, string>[procItems.Length];
            for (int i = 0; i < procItems.Length; i++)
            {
                long size = procItems[i].Length;
                var t = Tuple.Create<long, string, string, string, string>(size, (i + 1).ToString(), ROund(size), size.ToString(), procItems[i].FullName);

                items[i] = t;

                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
            }

            try
            {
                if (descend) { items = items.OrderByDescending(f => f.Item1).ToArray(); }
                else         { items = items.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (PathTooLongException e)
            {
                Console.WriteLine("ERR[PathTooLongException]: " + e.Message);
                Console.WriteLine("if there are files with names over 260 characters...");
                Console.WriteLine("... use the 'long' option to find them");
                return 1;
            }
            catch (ArgumentNullException e)       { Console.WriteLine("ERR[ArgumentNullException]: "       + e.Message); return 1; }
            catch (SecurityException e)           { Console.WriteLine("ERR[SecurityException]: "           + e.Message); return 1; }
            catch (ArgumentException e)           { Console.WriteLine("ERR[ArgumentException]: "           + e.Message); return 1; }
            catch (UnauthorizedAccessException e) { Console.WriteLine("ERR[UnauthorizedAccessException]: " + e.Message); return 1; }
            catch (NotSupportedException e)       { Console.WriteLine("ERR[NotSupportedException]: "       + e.Message); return 1; }
            catch (Exception e)                   { Console.WriteLine("ERR[Exception]: "                   + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Size", "Bytes", "Path");
            for (int i = 0; i < items.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), items[i].Item3, items[i].Item4, items[i].Item5); }

            return 0;
        }
        private static int option_print_dirSize(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            DirectoryInfo[] procItems = new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("*.*", mode);

            Console.WriteLine("# found " + procItems.Length + " dir(s)");
            Console.WriteLine("# sorting by size....");
            Console.WriteLine("");
            
            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string, string>[] items = new Tuple<long, string, string, string, string>[procItems.Length];
            for (int i = 0; i < procItems.Length; i++)
            {
                long size = DirSize(procItems[i]);
                var t = Tuple.Create<long, string, string, string, string>(size, (i + 1).ToString(), ROund(size), size.ToString(), procItems[i].FullName);

                items[i] = t;

                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
            }

            try
            {
                if (descend) { items = items.OrderByDescending(f => f.Item1).ToArray(); }
                else         { items = items.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (PathTooLongException e)
            {
                Console.WriteLine("ERR[PathTooLongException]: " + e.Message);
                Console.WriteLine("if there are files with names over 260 characters...");
                Console.WriteLine("... use the 'long' option to find them");
                return 1;
            }
            catch (ArgumentNullException e)       { Console.WriteLine("ERR[ArgumentNullException]: "       + e.Message); return 1; }
            catch (SecurityException e)           { Console.WriteLine("ERR[SecurityException]: "           + e.Message); return 1; }
            catch (ArgumentException e)           { Console.WriteLine("ERR[ArgumentException]: "           + e.Message); return 1; }
            catch (UnauthorizedAccessException e) { Console.WriteLine("ERR[UnauthorizedAccessException]: " + e.Message); return 1; }
            catch (NotSupportedException e)       { Console.WriteLine("ERR[NotSupportedException]: "       + e.Message); return 1; }
            catch (Exception e)                   { Console.WriteLine("ERR[Exception]: "                   + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Size", "Bytes", "Path");
            for (int i = 0; i < items.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), items[i].Item3, items[i].Item4, items[i].Item5); }

            return 0;
        }
        private static int option_print_dirCount(bool descend = false, bool enableSubDirFileCount = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            DirectoryInfo[] procItems = new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("*.*", mode);

            Console.WriteLine("# found " + procItems.Length + " dir(s)");
            Console.WriteLine("# sorting by size....");
            Console.WriteLine("");
            
            int longest_numb  = 1;
            int longest_info  = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string>[] items = new Tuple<long, string, string, string>[procItems.Length];
            for (int i = 0; i < procItems.Length; i++)
            {
                int fileCount = procItems[i].GetFiles("*.*", enableSubDirFileCount ?  System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly).Length;
                var t = Tuple.Create<long, string, string, string>(fileCount, (i + 1).ToString(), fileCount.ToString(), procItems[i].FullName);

                items[i] = t;

                if (t.Item2.Length > longest_numb) { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info) { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_path) { longest_path = t.Item4.Length; }
            }

            try
            {
                if (descend) { items = items.OrderByDescending(f => f.Item1).ToArray(); }
                else         { items = items.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (PathTooLongException e)
            {
                Console.WriteLine("ERR[PathTooLongException]: " + e.Message);
                Console.WriteLine("if there are files with names over 260 characters...");
                Console.WriteLine("... use the 'long' option to find them");
                return 1;
            }
            catch (ArgumentNullException e)       { Console.WriteLine("ERR[ArgumentNullException]: "       + e.Message); return 1; }
            catch (SecurityException e)           { Console.WriteLine("ERR[SecurityException]: "           + e.Message); return 1; }
            catch (ArgumentException e)           { Console.WriteLine("ERR[ArgumentException]: "           + e.Message); return 1; }
            catch (UnauthorizedAccessException e) { Console.WriteLine("ERR[UnauthorizedAccessException]: " + e.Message); return 1; }
            catch (NotSupportedException e)       { Console.WriteLine("ERR[NotSupportedException]: "       + e.Message); return 1; }
            catch (Exception e)                   { Console.WriteLine("ERR[Exception]: "                   + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,0}";
            Console.WriteLine(headerFormat, "#", "Files", "Path");
            for (int i = 0; i < items.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), items[i].Item3, items[i].Item4); }

            return 0;
        }
        private static int option_print_duration(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            
            List<FileInfo> procItemsList = new List<FileInfo>();
            foreach (string type in VideoTypes) { procItemsList.AddRange(new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*" + type, mode)); }
            FileInfo[] procItems = procItemsList.ToArray();
            procItemsList.Clear();

            Console.WriteLine("# found " + procItems.Length + " file(s)");
            if (procItems.Length == 0) { return 1; }
            Console.WriteLine("# sorting by video length...");
            Console.WriteLine("");

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 11;
            int longest_path  = 4;
            Tuple<double, string, string, string, string>[] items = new Tuple<double, string, string, string, string>[procItems.Length];
            for (int i = 0; i < procItems.Length; i++)
            {
                var len = new WindowsMediaPlayer().newMedia(procItems[i].FullName);
                var t = Tuple.Create<double, string, string, string, string>(len.duration, (i + 1).ToString(), len.durationString, len.duration.ToString(), procItems[i].FullName);

                items[i] = t;

                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
            }

            try
            {
                if (descend) { items = items.OrderByDescending(f => f.Item1).ToArray(); }
                else         { items = items.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (PathTooLongException e)
            {
                Console.WriteLine("ERR[PathTooLongException]: " + e.Message);
                Console.WriteLine("if there are files with names over 260 characters...");
                Console.WriteLine("... use the 'long' option to find them");
                return 1;
            }
            catch (ArgumentNullException e)       { Console.WriteLine("ERR[ArgumentNullException]: "       + e.Message); return 1; }
            catch (SecurityException e)           { Console.WriteLine("ERR[SecurityException]: "           + e.Message); return 1; }
            catch (ArgumentException e)           { Console.WriteLine("ERR[ArgumentException]: "           + e.Message); return 1; }
            catch (UnauthorizedAccessException e) { Console.WriteLine("ERR[UnauthorizedAccessException]: " + e.Message); return 1; }
            catch (NotSupportedException e)       { Console.WriteLine("ERR[NotSupportedException]: "       + e.Message); return 1; }
            catch (Exception e)                   { Console.WriteLine("ERR[Exception]: "                   + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Time", "Time Double", "Path");
            for (int i = 0; i < items.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), items[i].Item3, items[i].Item4, items[i].Item5); }

            return 0;
        }
        
        private static void processFile(string file)
        {
            if      (MOVE)   { file_mov(file); }
            else if (DELETE) { file_del(file); }
        }
        private static void processFile2(string f1, string f2)
        {
            processFile(useSecondItem ? f2 : f1);
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
        private static string ROund(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
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
        // only use limited bytes buffer
        private static string CalculateMD5fast(string path)
        {
            using (var md5 = MD5.Create())
            using (var stream = new BufferedStream(File.OpenRead(path), 1200000))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        public static string CalculateMD5withProc(string file)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = "md5sum.exe";
                proc.StartInfo.Arguments = "\"" + file + "\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.Start();
                proc.WaitForExit();
                string output = proc.StandardOutput.ReadToEnd();
                return output.Split(' ')[0].Substring(1).ToUpper();
            }
            catch (System.ComponentModel.Win32Exception e) { Console.WriteLine("# ERROR: md5sum.exe : " + e.Message); return "ERROR"; }
            catch (Exception e)                            { Console.WriteLine("ERR[Exception]: "       + e.Message); return "ERROR"; }
        }
        private static string GetFileName(string path)
        {
            string[] index = path.Split('\\');
            return index[index.Length - 1];
        }
        public static string GetImgHash(Bitmap bmpMin)
        {
            string lResult = string.Empty;
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false        
                    Color c = bmpMin.GetPixel(i, j);
                    lResult += c.R.ToString() + c.G.ToString() + c.B.ToString();
                }
            }
            return lResult;
        }
        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis) { size += fi.Length; }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis) { size += DirSize(di); }
            return size;
        }
    }
}
