using System;
using System.Threading;
using System.Security.Cryptography;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;

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
            Console.WriteLine(" | BUILT IN..: VS C# .NET 4.5");
            Console.WriteLine(" | VERSION...: 33");
            Console.WriteLine(" | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...");
            Console.WriteLine("");
            Console.WriteLine(" +===[ STANDARD OPTIONS ]");
            Console.WriteLine(" | +==[ FIND DUPLICATE FILES (FDF) ]");
            Console.WriteLine(" | | name    = find duplicate files by name.ext");
            Console.WriteLine(" | | noext   = find duplicate files by name");
            Console.WriteLine(" | | hash    = find duplicate files by md5 checksum hash");
            Console.WriteLine(" | | hashbuf = find duplicate files by md5 checksum hash, but use a larger byte buffer (1 mil bytes)");
            Console.WriteLine(" | | hashexe = find duplicate files by md5 checksum hash, but use the 'md5sum.exe' to get the file hash");
            Console.WriteLine(" | | byte    = find files that have the same byte size");
            Console.WriteLine(" | | pic     = find duplicate images (resize image to 16x16 -> compare pixels)");
            Console.WriteLine(" | | pic2    = find duplicate images (resize image to 16x16 -> average out the RGB -> compare pixels)");
            Console.WriteLine(" |");
            Console.WriteLine(" | +==[ FIND FILES THAT ___ (FFT) ]");
            Console.WriteLine(" | | vid     = find corrupt and playable videos (uses ffmpeg)");
            Console.WriteLine(" | | vidt    = find playable video files (uses ffmpeg)");
            Console.WriteLine(" | | vidf    = find corrupt video files (uses ffmpeg)");
            Console.WriteLine(" | | sound   = print video files that have, and don't have sound/audio (uses ffprobe)");
            Console.WriteLine(" | | soundt  = find video files that have sound/audio (uses ffprobe)");
            Console.WriteLine(" | | soundf  = find video files that don't have sound/audio (uses ffprobe)");
            Console.WriteLine(" | | long    = find files with over 260 characters in file path (too long)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]");
            Console.WriteLine(" | all     = also scan subdirectories");
            Console.WriteLine(" | del     = send the found file to recycle bin");
            Console.WriteLine(" | mov     = move the found file to a folder (fscan_dir)");
            Console.WriteLine(" | vXXXX   = print status every XXXX ms");
            Console.WriteLine(" |");
            Console.WriteLine(" | +==[ FDF ]");
            Console.WriteLine(" | | 1     = use the first file (del/mov/...)");
            Console.WriteLine(" | | 2     = use the second file (del/mov/...) (default)");
            Console.WriteLine(" | | ask   = when a dupe is found prompt on what to do with the files");
            Console.WriteLine(" | | end   = print/process files when the file scanning/comparing is finished");
            Console.WriteLine(" | | nomd5 = don't calculate/print the md5 checksum of files");
            Console.WriteLine("");
            Console.WriteLine(" +===[ PRINT ONLY OPTIONS / SORT OPTIONS ]");
            Console.WriteLine(" | sizea    = print file sizes in ascending order");
            Console.WriteLine(" | sized    = print file sizes in descending order");
            Console.WriteLine(" | dsizea   = print directory size in ascending order");
            Console.WriteLine(" | dsized   = print directory size in descending order");
            Console.WriteLine(" | dcounta  = print directory files count in ascending order");
            Console.WriteLine(" | dcountd  = print directory files count in descending order");
            Console.WriteLine(" | rdcounta = print directory (+subdirs) files count in ascending order");
            Console.WriteLine(" | rdcountd = print directory (+subdirs) files count in descending order");
            Console.WriteLine(" | datea    = print file creation dates in ascending order");
            Console.WriteLine(" | dated    = print file creation dates in descending order");
            Console.WriteLine(" | lena     = print video length in ascending order (uses ffprobe)");
            Console.WriteLine(" | lend     = print video length in descending order (uses ffprobe)");
            Console.WriteLine("");
            Console.WriteLine(" +===[ OPTION_ = use multithreading (extremely fast, experimental, not finished) ]");
            Console.WriteLine(" | name_, noext_, hash_, hashbuf_, hashexe_, byte_, pic_, pic2_");
            Console.WriteLine(" | vid_, vidt_, vidf_, sound_, soundt_, soundf_,");
            Console.WriteLine(" | lena_, lend_");
            Console.WriteLine("");


        }

        private static bool DELETE  = false;
        private static bool MOVE    = false;
        private static bool PROMPT  = false;
        private static bool END     = false;
        private static bool VERBOSE = false; private static int VERBOSE_DELAY = 1000;
        private static bool NOMD5   = false;
        private readonly static string MOVE_DIR = "fscan_dir";
        public static readonly string[] VideoTypes = { ".mp4", ".webm", ".avi", ".mov", ".mkv", ".flv", ".mpeg", ".mpg", ".wmv", ".mp3", ".ogg" };
        public static readonly string[] ImageTypes = { ".png", ".jpg", ".jpeg", ".bmp" };
        private static System.IO.SearchOption mode = System.IO.SearchOption.TopDirectoryOnly;
        
        private static bool ONLY_TRUE = false;
        private static bool ONLY_FALSE = false;
        private static bool useSecondItem = true;

        private static float gl_tested_max = 0;
        private static float gl_tested = 0;
        private static float gl_errors = 0;

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
                if (arg.StartsWith("v"))
                {
                    string val = arg.Remove(0, 1);
                    VERBOSE = true; VERBOSE_DELAY = int.TryParse(val, out int res) ? res : 1000;
                }
                else
                {
                    switch (arg.ToLower())
                    {
                        case "all":   { mode = System.IO.SearchOption.AllDirectories; break; }
                        case "del":   { DELETE = true; break; }
                        case "mov":   { MOVE   = true; break; }
                        case "ask":   { PROMPT = true; break; }
                        case "nomd5": { NOMD5  = true; break; }
                        case "end":   { END    = true; break; }
                        case "1":     { useSecondItem = false; break; }
                        case "2":     { useSecondItem = true;  break; }
                    }
                }
            }

            currentTime = DateTime.Now;

            int ret = 1;
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "name":     {                    ret = option_find_dupes(0);                    break; }
                    case "noext":    {                    ret = option_find_dupes(1);                    break; }
                    case "byte":     {                    ret = option_find_dupes(2);                    break; }
                    case "hash":     {                    ret = option_find_dupes(10);                   break; }
                    case "hashbuf":  {                    ret = option_find_dupes(11);                   break; }
                    case "hashexe":  {                    ret = option_find_dupes(12);                   break; }
                    case "pic":      {                    ret = option_find_dupes(20);                   break; }
                    case "pic2":     {                    ret = option_find_dupes(21);                   break; }
                    case "name_":    {                    ret = option_find_dupes_threaded(0);           break; }
                    case "noext_":   {                    ret = option_find_dupes_threaded(1);           break; }
                    case "byte_":    {                    ret = option_find_dupes_threaded(2);           break; }
                    case "hash_":    {                    ret = option_find_dupes_threaded(10);          break; }
                    case "hashbuf_": {                    ret = option_find_dupes_threaded(11);          break; }
                    case "hashexe_": {                    ret = option_find_dupes_threaded(12);          break; }
                    case "pic_":     {                    ret = option_find_dupes_threaded(20);          break; }
                    case "pic2_":    {                    ret = option_find_dupes_threaded(21);          break; }
                    case "vid":      {                    ret = option_find_unplayablevideos();          break; }
                    case "vidt":     { ONLY_TRUE  = true; ret = option_find_unplayablevideos();          break; }
                    case "vidf":     { ONLY_FALSE = true; ret = option_find_unplayablevideos();          break; }
                    case "vid_":     {                    ret = option_find_unplayablevideos_threaded(); break; }
                    case "vidt_":    { ONLY_TRUE  = true; ret = option_find_unplayablevideos_threaded(); break; }
                    case "vidf_":    { ONLY_FALSE = true; ret = option_find_unplayablevideos_threaded(); break; }
                    case "sound":    {                    ret = option_find_mutes();                     break; }
                    case "soundt":   { ONLY_TRUE  = true; ret = option_find_mutes();                     break; }
                    case "soundf":   { ONLY_FALSE = true; ret = option_find_mutes();                     break; }
                    case "sound_":   {                    ret = option_find_mutes_threaded();            break; }
                    case "soundt_":  { ONLY_TRUE  = true; ret = option_find_mutes_threaded();            break; }
                    case "soundf_":  { ONLY_FALSE = true; ret = option_find_mutes_threaded();            break; }
                    case "sizea":    {                    ret = option_print_size(false);                break; }
                    case "sized":    {                    ret = option_print_size(true);                 break; }
                    case "dsizea":   {                    ret = option_print_dirSize(false);             break; }
                    case "dsized":   {                    ret = option_print_dirSize(true);              break; }
                    case "dcounta":  {                    ret = option_print_dirCount(false);            break; }
                    case "dcountd":  {                    ret = option_print_dirCount(true);             break; }
                    case "rdcounta": {                    ret = option_print_dirCount(false, true);      break; }
                    case "rdcountd": {                    ret = option_print_dirCount(true, true);       break; }
                    case "datea":    {                    ret = option_print_date(false);                break; }
                    case "dated":    {                    ret = option_print_date(true);                 break; }
                    case "long":     {                    ret = option_print_longnames();                break; }
                    case "lena":     {                    ret = option_print_duration(false);            break; }
                    case "lend":     {                    ret = option_print_duration(true);             break; }
                    case "lena_":    {                    ret = option_print_duration_threaded(false);   break; }
                    case "lend_":    {                    ret = option_print_duration_threaded(true);    break; }
                }
            }

            print_info_end();

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
                Console.WriteLine("# file's directory..: " + fi.Directory);
                Console.WriteLine("# file size.........: " + ROund(fi.Length) + " (" + fi.Length + " bytes)");
                Console.WriteLine("# creation time.....: " + fi.CreationTime);
                Console.WriteLine("# last access time..: " + fi.LastAccessTime);
                Console.WriteLine("# last write time...: " + fi.LastWriteTime);
                Console.WriteLine("# md5 checksum hash.: " + CalculateMD5(fi.FullName));

                foreach (string t in VideoTypes)
                {
                    if (fi.Extension == t)
                    {
                        Process ffmpeg = new Process();
                        ffmpeg.StartInfo.UseShellExecute = false;
                        ffmpeg.StartInfo.RedirectStandardOutput = true;
                        ffmpeg.StartInfo.RedirectStandardError = true;
                        ffmpeg.StartInfo.FileName = "ffmpeg.exe";
                        ffmpeg.StartInfo.Arguments = "-v error -i " + "\"" + fi.FullName + "\"" + " -f null -";
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
                        ffprobe.StartInfo.Arguments = "-i " + "\"" + fi.FullName + "\"" + " -show_streams -select_streams a -loglevel error";
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

        private static int option_find_dupes(int findMode)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            Console.WriteLine("# path: " + di.FullName);
            
            List<FileInfo> files = new List<FileInfo>();
            switch (findMode)
            {
                case 0:
                case 1:
                case 2:
                case 10:
                case 11:
                case 12:
                default: files.AddRange(new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.*", mode)); break;

                case 20:
                case 21: for (int i = 0; i < ImageTypes.Length; i++) { files.AddRange(di.GetFiles("*" + ImageTypes[i], mode)); } break;
            }
            if (files == null) { return 1; }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            Hashtable table = new Hashtable();
            foreach (FileInfo file1 in files)
            {
                string hash1 = string.Empty;
                switch (findMode)
                {
                    case 0:  hash1 = Path.GetFileName(file1.FullName);                 break;
                    case 1:  hash1 = Path.GetFileNameWithoutExtension(file1.FullName); break;
                    case 2:  hash1 = file1.Length.ToString();                          break;
                    case 10: hash1 = CalculateMD5(file1.FullName);                     break;
                    case 11: hash1 = CalculateMD5fast(file1.FullName);                 break;
                    case 12: hash1 = CalculateMD5withProc(file1.FullName);             break;
                    case 20: hash1 = GetImgHash(file1.FullName);                       break;
                    case 21: hash1 = GetImgHash2(file1.FullName);                      break;
                }

                if (string.IsNullOrEmpty(hash1)) { gl_errors++; Console.Error.WriteLine("# ERROR: failed to id file: " + file1.FullName); continue; }

                if (table.ContainsKey(hash1))
                {
                    FileInfo file2 = new FileInfo(table[hash1].ToString());
                    gl_dupes.Add(new Dupe(gl_dupes.Count+1, file1, file2));
                    if (!END) { printAndProcessDupes(gl_dupes.Count, file1, file2); }

                } else { table.Add(hash1, file1.FullName); }

                gl_tested++;
            }

            th.Abort();
            
            if (END) { processDupes(); }

            return 0;
        }
        private static int option_find_dupes_threaded(int findMode)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            Console.WriteLine("# path: " + di.FullName);

            List<FileInfo> files = new List<FileInfo>();
            switch (findMode)
            {
                case 0:
                case 1:
                case 2:
                case 10:
                case 11:
                case 12:
                default: files.AddRange(new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.*", mode)); break;

                case 20:
                case 21: foreach (string type in ImageTypes) { files.AddRange(di.GetFiles("*" + type, mode)); } break;
            }
            if (files == null) { return 1; }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# starting the comparison...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            List<Tuple<string, FileInfo>> hashNfile = new List<Tuple<string, FileInfo>>();
            
            CountdownEvent countdown = new CountdownEvent(files.Count);
            foreach (FileInfo file in files)
            {
                ThreadPool.QueueUserWorkItem((i) =>
                {
                    string hash = string.Empty;
                    switch (findMode)
                    {
                        case 0:  hash = Path.GetFileName(file.FullName);                 break;
                        case 1:  hash = Path.GetFileNameWithoutExtension(file.FullName); break;
                        case 2:  hash = file.Length.ToString();                          break;
                        case 10: hash = CalculateMD5(file.FullName);                     break;
                        case 11: hash = CalculateMD5fast(file.FullName);                 break;
                        case 12: hash = CalculateMD5withProc(file.FullName);             break;
                        case 20: hash = GetImgHash(file.FullName);                       break;
                        case 21: hash = GetImgHash2(file.FullName);                      break;
                    }

                    if (string.IsNullOrEmpty(hash)) { gl_errors++; Console.Error.WriteLine("# ERROR: failed to id file: " + file.FullName); }
                    else                            { hashNfile.Add(new Tuple<string, FileInfo>(hash, file));                               }
                    gl_tested++;
                    countdown.Signal();
                });
            }
            countdown.Wait();
            
            Hashtable table = new Hashtable();
            foreach(Tuple<string, FileInfo> tuple in hashNfile)
            {
                string hash1 = tuple.Item1;
                FileInfo file1 = tuple.Item2;
                if (table.ContainsKey(hash1))
                {
                    FileInfo file2 = new FileInfo(table[hash1].ToString());
                    gl_dupes.Add(new Dupe(gl_dupes.Count + 1, file1, file2));
                    if (!END) { printAndProcessDupes(gl_dupes.Count, file1, file2); }
                }
                else { table.Add(hash1, file1.FullName); }
            }

            th.Abort();
            
            if (END) { processDupes(); }

            return 0;
        }

        /* FIND FILES THAT __ */
        public static List<string> gl_files = new List<string>(); // for END option
        private static int option_find_mutes()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            //get files
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            List<FileInfo> files = new List<FileInfo>();
            foreach (string type in VideoTypes)
            { files.AddRange(di.GetFiles("*" + type, mode)); }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# scanning for audio...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            foreach (FileInfo file in files)
            {
                Process ffprobe = new Process();

                try
                {
                    ffprobe.StartInfo.UseShellExecute = false;
                    ffprobe.StartInfo.RedirectStandardOutput = true;
                    ffprobe.StartInfo.RedirectStandardError = true;
                    ffprobe.StartInfo.FileName = "ffprobe.exe";
                    ffprobe.StartInfo.Arguments = "-i " + "\"" + file.FullName + "\"" + " -show_streams -select_streams a -loglevel error";
                    ffprobe.Start();
                }
                catch (Win32Exception e) { gl_errors++; Console.Error.WriteLine("# ERROR: ffprobe.exe : " + e.Message); return 1; }
                catch (Exception e)      { gl_errors++; Console.Error.WriteLine("ERR[Exception]: "        + e.Message); return 1; }

                string output = ffprobe.StandardOutput.ReadToEnd();
                ffprobe.WaitForExit();

                //empty = no sound
                if (!string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file.FullName);
                        processFile(file);
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file.FullName);
                        processFile(file);
                    }
                }

                gl_tested++;
            }

            th.Abort();
            
            return 0;
        }
        private static int option_find_mutes_threaded()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            //get files
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            List<FileInfo> files = new List<FileInfo>();
            foreach (string type in VideoTypes)
            { files.AddRange(di.GetFiles("*" + type, mode)); }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# scanning for audio...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            CountdownEvent countdown = new CountdownEvent(files.Count);
            foreach (FileInfo file in files)
            {
                ThreadPool.QueueUserWorkItem((i) =>
                {
                    Process ffprobe = new Process();
                    ffprobe.StartInfo.UseShellExecute = false;
                    ffprobe.StartInfo.RedirectStandardOutput = true;
                    ffprobe.StartInfo.RedirectStandardError = true;
                    ffprobe.StartInfo.FileName = "ffprobe.exe";
                    ffprobe.StartInfo.Arguments = "-i " + "\"" + file.FullName + "\"" + " -show_streams -select_streams a -loglevel error";

                    try
                    {

                        ffprobe.Start();
                    }
                    catch (Win32Exception e) { gl_errors++; Console.Error.WriteLine("# ERROR: ffprobe.exe : " + e.Message); return; }
                    catch (Exception e)      { gl_errors++; Console.Error.WriteLine("ERR[Exception]: "        + e.Message); return; }

                    string output = ffprobe.StandardOutput.ReadToEnd();
                    ffprobe.WaitForExit();

                    if (string.IsNullOrEmpty(output) && ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file.FullName);
                        processFile(file);
                    }
                    else if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file.FullName);
                        processFile(file);
                    }
                    gl_tested++;
                    countdown.Signal();
                });
            }
            countdown.Wait();

            th.Abort();
            
            return 0;
        }
        private static int option_find_unplayablevideos()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            //get files
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            List<FileInfo> files = new List<FileInfo>();
            foreach (string type in VideoTypes)
            { files.AddRange(di.GetFiles("*" + type, mode)); }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# scanning for playable/corrupt videos...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            foreach (FileInfo file in files)
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
                    ffmpeg.StartInfo.Arguments = "-v error -sseof -60 -i " + "\"" + file.FullName + "\"" + " -f null -";
                    ffmpeg.Start();
                }
                catch (Win32Exception e) { gl_errors++; Console.Error.WriteLine("# ERROR: ffmpeg.exe : " + e.Message); return 1; }
                catch (Exception e)      { gl_errors++; Console.Error.WriteLine("ERR[Exception]: "        + e.Message); return 1; }
                
                string output = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
                
                //empty = no errors
                if (string.IsNullOrEmpty(output))
                {
                    //TRUE
                    if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file.FullName);
                        processFile(file);
                    }
                }
                else
                {
                    //FALSE
                    if (ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file.FullName);
                        processFile(file);
                    }
                }

                gl_tested++;
            }

            th.Abort();
            
            return 0;
        }
        private static int option_find_unplayablevideos_threaded()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);

            // get files
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            List<FileInfo> files = new List<FileInfo>();
            foreach (string type in VideoTypes)
            { files.AddRange(di.GetFiles("*" + type, mode)); }

            Console.WriteLine("# found " + files.Count + " file(s)");
            if (files.Count == 0) { return 1; }
            gl_tested_max = files.Count;
            Console.WriteLine("# scanning for playable/corrupt videos...");
            Console.WriteLine("");

            if (ONLY_TRUE == false && ONLY_FALSE == false) { ONLY_TRUE = true; ONLY_FALSE = true; }

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            CountdownEvent countdown = new CountdownEvent(files.Count);
            foreach (FileInfo file in files)
            {
                ThreadPool.QueueUserWorkItem((i) =>
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
                        ffmpeg.StartInfo.Arguments = "-v error -sseof -60 -i " + "\"" + file.FullName + "\"" + " -f null -";
                        ffmpeg.Start();
                    }
                    catch (Win32Exception e) { gl_errors++; Console.Error.WriteLine("# ERROR: ffmpeg.exe : " + e.Message); return; }
                    catch (Exception e)      { gl_errors++; Console.Error.WriteLine("ERR[Exception]: "       + e.Message); return; }

                    string output = ffmpeg.StandardOutput.ReadToEnd();
                    ffmpeg.WaitForExit();

                    if (string.IsNullOrEmpty(output) && ONLY_FALSE)
                    {
                        Console.WriteLine("F: " + file.FullName);
                        processFile(file);
                    }
                    else if (ONLY_TRUE)
                    {
                        Console.WriteLine("T: " + file.FullName);
                        processFile(file);
                    }
                    gl_tested++;
                    countdown.Signal();
                });
            }

            countdown.Wait();

            th.Abort();
            
            return 0;
        }
        private static int option_print_longnames()
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            string[] files = Directory.GetFiles(Environment.CurrentDirectory, "*.*", mode);
            Console.WriteLine("# found " + files.Length + " file(s)");
            if (files.Length == 0) { return 1; }
            gl_tested_max = files.Length;
            Console.WriteLine("# searching for \"path too long\" file names...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int count = 0;
            foreach (string file in files)
            {
                if (file.Length >= 260)
                {
                    count++;
                    Console.WriteLine(count + ": " + file);
                }
                gl_tested++;
            }
            if (count == 0) { Console.WriteLine("# 0 errors"); }

            th.Abort();
            
            return 0;
        }

        private static void print_info()
        {
            if (!VERBOSE) { return; }
            while (true)
            {
                Thread.Sleep(VERBOSE_DELAY);
                if (userIsPrompted) { continue; }

                string perc = ((Math.Round((gl_tested == 0 || gl_tested_max == 0) ? 0 : (gl_tested / gl_tested_max) * 100, 2)).ToString() + "%").PadLeft(7, ' ');
                string outOf = (gl_tested + "/" + gl_tested_max).PadLeft(gl_tested_max.ToString().Length * 2 + 1, ' ');
                string time = (DateTime.Now - currentTime).ToString();
                Console.WriteLine(": " + perc + " " + outOf + (gl_dupes.Count != 0 ? " -> " + gl_dupes.Count : "") + " --- " + time);
            }
        }
        private static void print_info_end()
        {
              Console.WriteLine();
              Console.WriteLine("# tested files..: " + gl_tested);
              Console.WriteLine("# errors........: " + gl_errors);
              if (gl_dupes.Count != 0)
            { Console.WriteLine("# dupes found...: " + gl_dupes.Count); }
              Console.WriteLine("# time taken....: " + (DateTime.Now - currentTime).ToString());
              Console.WriteLine();
        }
        
        private static int option_print_size(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            FileInfo[] items = new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.*", mode);
            Console.WriteLine("# found " + items.Length + " file(s)");
            if (items.Length == 0) { return 1; }
            gl_tested_max = items.Length;
            Console.WriteLine("# sorting files by size....");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string, string>[] infoItems = new Tuple<long, string, string, string, string>[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                long size = items[i].Length;
                var t = Tuple.Create<long, string, string, string, string>(size, (i + 1).ToString(), ROund(size), size.ToString(), items[i].FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
                gl_tested++;
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Size", "Bytes", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4, infoItems[i].Item5); }

            th.Abort();
            
            return 0;
        }
        private static int option_print_dirSize(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            DirectoryInfo[] items = new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("*.*", mode);
            Console.WriteLine("# found " + items.Length + " dir(s)");
            if (items.Length == 0) { return 1; }
            gl_tested_max = items.Length;
            Console.WriteLine("# sorting dirs by size....");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string, string>[] infoItems = new Tuple<long, string, string, string, string>[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                long size = DirSize(items[i]);
                var t = Tuple.Create<long, string, string, string, string>(size, (i + 1).ToString(), ROund(size), size.ToString(), items[i].FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
                gl_tested++;
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { gl_errors++; Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Size", "Bytes", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4, infoItems[i].Item5); }

            th.Abort();
            
            return 0;
        }
        private static int option_print_dirCount(bool descend = false, bool enableSubDirFileCount = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            DirectoryInfo[] items = new DirectoryInfo(Environment.CurrentDirectory).GetDirectories("*.*", mode);
            Console.WriteLine("# found " + items.Length + " dir(s)");
            if (items.Length == 0) { return 1; }
            gl_tested_max = items.Length;
            Console.WriteLine("# sorting dirs by file count....");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int longest_numb  = 1;
            int longest_info  = 5;
            int longest_path  = 4;
            Tuple<long, string, string, string>[] infoItems = new Tuple<long, string, string, string>[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                int fileCount = items[i].GetFiles("*.*", enableSubDirFileCount ?  System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly).Length;
                var t = Tuple.Create<long, string, string, string>(fileCount, (i + 1).ToString(), fileCount.ToString(), items[i].FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb) { longest_numb = t.Item2.Length; }
                if (t.Item3.Length > longest_info) { longest_info = t.Item3.Length; }
                if (t.Item4.Length > longest_path) { longest_path = t.Item4.Length; }
                gl_tested++;
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { gl_errors++; Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,0}";
            Console.WriteLine(headerFormat, "#", "Files", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4); }

            th.Abort();
            
            return 0;
        }
        private static int option_print_date(bool descend = false)
        {
            Console.WriteLine("# path: " + Environment.CurrentDirectory);
            FileInfo[] items = new DirectoryInfo(Environment.CurrentDirectory).GetFiles("*.*", mode);
            Console.WriteLine("# found " + items.Length + " file(s)");
            if (items.Length == 0) { return 1; }
            gl_tested_max = items.Length;
            Console.WriteLine("# sorting files by date....");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_info2 = 3;
            int longest_path  = 4;
            Tuple<DateTime, string, string, string, string>[] infoItems = new Tuple<DateTime, string, string, string, string>[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var dateTime    = items[i].CreationTime;
                var dateTimeUTC = items[i].CreationTimeUtc;
                var t = Tuple.Create<DateTime, string, string, string, string>(dateTime, (i + 1).ToString(), dateTime.ToString(), dateTimeUTC.ToString(), items[i].FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb)  { longest_numb  = t.Item2.Length; }
                if (t.Item3.Length > longest_info)  { longest_info  = t.Item3.Length; }
                if (t.Item4.Length > longest_info2) { longest_info2 = t.Item4.Length; }
                if (t.Item5.Length > longest_path)  { longest_path  = t.Item5.Length; }
                gl_tested++;
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { gl_errors++; Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,-" + longest_info2 + "} {3,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,"  + longest_info2 + "} {3,0}";
            Console.WriteLine(headerFormat, "#", "Time", "UTC", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4, infoItems[i].Item5); }

            th.Abort();
            
            return 0;
        }
        private static int option_print_duration(bool descend = false)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            Console.WriteLine("# path: " + di.FullName);
            List<FileInfo> items = new List<FileInfo>();
            for (int i = 0; i < VideoTypes.Length; i++) { items.AddRange(di.GetFiles("*" + VideoTypes[i], mode)); }
            Console.WriteLine("# found " + items.Count + " file(s)");
            if (items.Count == 0) { return 1; }
            gl_tested_max = items.Count;
            Console.WriteLine("# sorting by video length...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            int longest_numb  = 1;
            int longest_info  = 4;
            int longest_path  = 4;
            Tuple<string, string, string, string>[] infoItems = new Tuple<string, string, string, string>[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                var len = getVideoLength(items[i].FullName);
                var t = Tuple.Create<string, string, string, string>(len, (i + 1).ToString(), len, items[i].FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb) { longest_numb = t.Item2.Length; }
                if (t.Item3.Length > longest_info) { longest_info = t.Item3.Length; }
                if (t.Item4.Length > longest_path) { longest_path = t.Item4.Length; }
                gl_tested++;
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { gl_errors++; Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat  = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,0}";
            string contentFormat = "{0,"  + longest_numb + "} {1,"  + longest_info + "} {2,0}";
            Console.WriteLine(headerFormat, "#", "Time", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4); }

            th.Abort();
            
            return 0;
        }
        private static int option_print_duration_threaded(bool descend = false)
        {
            DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
            Console.WriteLine("# path: " + di.FullName);
            List<FileInfo> items = new List<FileInfo>();
            for (int i = 0; i < VideoTypes.Length; i++) { items.AddRange(di.GetFiles("*" + VideoTypes[i], mode)); }
            Console.WriteLine("# found " + items.Count + " file(s)");
            if (items.Count == 0) { return 1; }
            gl_tested_max = items.Count;
            Console.WriteLine("# sorting by video length...");
            Console.WriteLine("");

            Thread th = new Thread(print_info) { IsBackground = true };
            th.Start();

            CountdownEvent countdown = new CountdownEvent(items.Count);
            List<Tuple<string, FileInfo>> filesWithLen = new List<Tuple<string, FileInfo>>();
            Mutex mtx = new Mutex();
            foreach (FileInfo file in items)
            {
                ThreadPool.QueueUserWorkItem((i) =>
                {
                    string len = getVideoLength(file.FullName);
                    mtx.WaitOne();
                    filesWithLen.Add(new Tuple<string, FileInfo>(len, file));
                    gl_tested++;
                    mtx.ReleaseMutex();
                    countdown.Signal();
                }); 
            }
            countdown.Wait();

            int longest_numb = 1;
            int longest_info = 4;
            int longest_path = 4;
            Tuple<string, string, string, string>[] infoItems = new Tuple<string, string, string, string>[items.Count];
            for (int i = 0; i < filesWithLen.Count; i++)
            {
                var t = Tuple.Create<string, string, string, string>(filesWithLen[i].Item1, (i + 1).ToString(), filesWithLen[i].Item1, filesWithLen[i].Item2.FullName);
                infoItems[i] = t;
                if (t.Item2.Length > longest_numb) { longest_numb = t.Item2.Length; }
                if (t.Item3.Length > longest_info) { longest_info = t.Item3.Length; }
                if (t.Item4.Length > longest_path) { longest_path = t.Item4.Length; }
            }

            try // SORT
            {
                if (descend) { infoItems = infoItems.OrderByDescending(f => f.Item1).ToArray(); }
                else         { infoItems = infoItems.OrderBy          (f => f.Item1).ToArray(); }
            }
            catch (Exception e) { gl_errors++; Console.Error.WriteLine("ERR[Exception]: " + e.Message); return 1; }

            string headerFormat = "{0,-" + longest_numb + "} {1,-" + longest_info + "} {2,0}";
            string contentFormat = "{0," + longest_numb + "} {1," + longest_info + "} {2,0}";
            Console.WriteLine(headerFormat, "#", "Time", "Path");
            for (int i = 0; i < infoItems.Length; i++) { Console.WriteLine(contentFormat, (i + 1).ToString(), infoItems[i].Item3, infoItems[i].Item4); }

            th.Abort();
            
            return 0;
        }

        // PROCESS DUPES
        private static void printAndProcessDupes(int num, FileInfo fi1, FileInfo fi2)
        {
            int maxPad = Math.Max(fi1.FullName.Length, fi2.FullName.Length);

            string size1 = ROund(fi1.Length) + " (" + fi1.Length + " bytes)";
            string size2 = ROund(fi2.Length) + " (" + fi2.Length + " bytes)";
            int maxPad2 = Math.Max(size1.Length, size2.Length);

            Console.WriteLine(num + ": " + fi1.FullName.PadRight(maxPad) + " " + size1.PadRight(maxPad2) + (NOMD5 ? "" : " " + CalculateMD5(fi1.FullName)));
            Console.WriteLine(num + ": " + fi2.FullName.PadRight(maxPad) + " " + size2.PadRight(maxPad2) + (NOMD5 ? "" : " " + CalculateMD5(fi2.FullName)));

            processFile2(fi1, fi2);
        }
        public struct Dupe
        {
            public Dupe(int n, FileInfo f1, FileInfo f2)
            { num = n; file1 = f1; file2 = f2; }
            public int num;
            public FileInfo file1;
            public FileInfo file2;
        }
        public static List<Dupe> gl_dupes = new List<Dupe>(); // for END option
        public static void processDupes()
        {
            foreach (Dupe dupe in gl_dupes)
            {
                printAndProcessDupes(dupe.num, dupe.file1, dupe.file2);
            }
        }
        private static bool userIsPrompted = false;
        private static string promptUser()
        {
            userIsPrompted = true;
            string a = Console.ReadLine();
            userIsPrompted = false;
            return a;
        }
        private static void processFile(FileInfo file)
        {
            if      (MOVE)   { file_mov(file.FullName); }
            else if (DELETE) { file_del(file.FullName); }
        }
        private static void processFile2(FileInfo f1, FileInfo f2)
        {
            if (PROMPT)
            {
                DoWhat:
                Console.Write("Do what? [mov/del]: ");
                string opt = promptUser();

                int option = 0;
                int file = 0;
                switch (opt)
                {
                    case "mov": case "m": option = 1; break;
                    case "del": case "d": option = 2; break;
                    default: Console.WriteLine("Choose a valid option."); goto DoWhat;
                }

                WhichFile:
                Console.Write("Which file? [1/2]: ");
                string opt2 = promptUser();

                switch (opt2)
                {
                    case "1": file = 1; break;
                    case "2": file = 2; break;
                    default: Console.WriteLine("Choose a valid option."); goto WhichFile;
                }

                switch (option)
                {
                    case 1:
                        {
                            file_mov(file == 1 ? f1.FullName : f2.FullName);
                            break;
                        }
                    case 2:
                        {
                            file_del(file == 1 ? f1.FullName : f2.FullName);
                            break;
                        }
                }

            }
            else
            {
                processFile(useSecondItem ? f2 : f1);
            }
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
            catch (ArgumentNullException e)             { gl_errors++; Console.Error.WriteLine("ERR[ArgumentNullException]"       + e.Message); }
            catch (System.Security.SecurityException e) { gl_errors++; Console.Error.WriteLine("ERR[SecurityException]"           + e.Message); }
            catch (ArgumentException e)                 { gl_errors++; Console.Error.WriteLine("ERR[ArgumentException]"           + e.Message); }
            catch (UnauthorizedAccessException e)       { gl_errors++; Console.Error.WriteLine("ERR[UnauthorizedAccessException]" + e.Message); }
            catch (PathTooLongException e)              { gl_errors++; Console.Error.WriteLine("ERR[PathTooLongException]"        + e.Message); }
            catch (NotSupportedException e)             { gl_errors++; Console.Error.WriteLine("ERR[NotSupportedException]"       + e.Message); }
            catch (DirectoryNotFoundException e)        { gl_errors++; Console.Error.WriteLine("ERR[DirectoryNotFoundException]"  + e.Message); }
            catch (IOException e)                       { gl_errors++; Console.Error.WriteLine("ERR[IOException] "                + e.Message); }
            catch (Exception e)                         { gl_errors++; Console.Error.WriteLine("ERR[Exception]"                   + e.Message); }
        }
        private static void file_del(string file)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                Console.WriteLine("del > " + file);
            }
            catch (ArgumentNullException e)             { gl_errors++; Console.Error.WriteLine("ERR[ArgumentNullException]"       + e.Message); }
            catch (System.Security.SecurityException e) { gl_errors++; Console.Error.WriteLine("ERR[SecurityException]"           + e.Message); }
            catch (ArgumentException e)                 { gl_errors++; Console.Error.WriteLine("ERR[ArgumentException]"           + e.Message); }
            catch (UnauthorizedAccessException e)       { gl_errors++; Console.Error.WriteLine("ERR[UnauthorizedAccessException]" + e.Message); }
            catch (PathTooLongException e)              { gl_errors++; Console.Error.WriteLine("ERR[PathTooLongException]"        + e.Message); }
            catch (NotSupportedException e)             { gl_errors++; Console.Error.WriteLine("ERR[NotSupportedException]"       + e.Message); }
            catch (DirectoryNotFoundException e)        { gl_errors++; Console.Error.WriteLine("ERR[DirectoryNotFoundException]"  + e.Message); }
            catch (IOException e)                       { gl_errors++; Console.Error.WriteLine("ERR[IOException] "                + e.Message); }
            catch (Exception e)                         { gl_errors++; Console.Error.WriteLine("ERR[Exception]"                   + e.Message); }
        }

        // ID-ing files
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
        private static string CalculateMD5fast(string path)
        {
            using (var md5 = MD5.Create())
            using (var stream = new BufferedStream(File.OpenRead(path), 1200000))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        private static string CalculateMD5withProc(string file)
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
            catch (Win32Exception e) { gl_errors++; Console.Error.WriteLine("# ERROR: md5sum.exe : " + e.Message); return ""; }
            catch (Exception e)      { gl_errors++; Console.Error.WriteLine("ERR[Exception]: "       + e.Message); return ""; }
        }
        private static string GetImgHash(string file1)
        {
            string lResult = string.Empty;

            Image img = Image.FromFile(file1);
            Bitmap bmp = new Bitmap(img, new Size(16, 16));
            img.Dispose();

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    lResult += pixelColor.R.ToString() + pixelColor.G.ToString() + pixelColor.B.ToString();
                }
            }
            bmp.Dispose();
            return lResult;
        }
        private static string GetImgHash2(string file1)
        {
            string lResult = string.Empty;

            Image img = Image.FromFile(file1);
            Bitmap bmp = new Bitmap(img, new Size(16, 16));
            img.Dispose();

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color pixelColor = bmp.GetPixel(x, y);
                    int avg = RoundOff((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
                    lResult += avg; /* 
                    + " "; /**/
                }
               //lResult += Environment.NewLine;
            }
            bmp.Dispose();

            //Console.WriteLine(lResult);
            return lResult;
        }

        private static long DirSize(DirectoryInfo d)
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
        private static string getVideoLength(string filePath)
        {
            Process ffprobe = new Process();
            try
            {
                ffprobe.StartInfo.UseShellExecute = false;
                ffprobe.StartInfo.RedirectStandardOutput = true;
                ffprobe.StartInfo.RedirectStandardError = true;
                ffprobe.StartInfo.FileName = "ffprobe.exe";
                ffprobe.StartInfo.Arguments = "\"" + filePath + "\"";
                ffprobe.Start();
            }
            catch (Win32Exception) { gl_errors++; return ""; }
            catch (Exception)      { gl_errors++; return ""; }

            string output = ffprobe.StandardError.ReadToEnd();
            ffprobe.WaitForExit();

            string[] lines = output.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            string DurationLine = string.Empty;
            foreach (string line in lines)
            {
                if (line.Contains("Duration")) { DurationLine = System.Text.RegularExpressions.Regex.Replace(line, @"\t|\n|\r", ""); }
            }

            string len = DurationLine.Replace("Duration:", "").Replace(" ", "").Split(',')[0];
            if (string.IsNullOrEmpty(len))
            {
                gl_errors++;
                return "";
            }

            return len;
        }
        private static int RoundOff(float i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
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
    }
}
