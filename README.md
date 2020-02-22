# fscan
file scanner/searcher (C#, console)

> fscan
```
 +===[ ABOUT ]
 | ABOUT.....: file scanner/searcher
 | AUTHOR....: 0xC0LD
 | BUILT IN..: VS C# .NET
 | VERSION...: 15
 | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...

 +===[ STANDARD OPTIONS ]
 | -c = find duplicate files by name.ext
 | -e = find duplicate files by name
 | -i = find duplicate files by md5 checksum hash
 | -p = find duplicate images (*.jpg, *.jpeg, *.png, *.bmp)
 | -v = find corrupt videos (uses ffmpeg)
 | -s = find video files that have (no) sound/audio (uses ffmpeg)
 |      (t = file with sound, f = file without sound)
 |      (use -t to only print files with sound)
 |      (use -f to only print files without sound)
 |      (printed files will be sent to runtime options if specified)

 +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]
 | -a = also scan subdirectories
 | -1 = use first file (del/mov/...)
 | -2 = use second file (del/mov/...) (default)
 | -d = send the found file to recycle bin
 | -m = move the found file to a folder (fscan_dir)
 | -v = print status

 +===[ PRINT (only) OPTIONS ]
 | -la = print file sizes (ascending order)
 | -ld = print file sizes (descending order)
 | -f  = print files with over 260 characters in file path (too long)
 | -ta = print video length (ascending order)
 | -td = print video length (descending order)

 +===[ EXAMPLES ]
 | > fscan -s -a -f -d
 | (scan all directories (dirs + subdirs) and delete files with no sound)
 | > fscan -c -a -d
 | (scan all directories (dirs + subdirs) and delete duplicate files)
```
