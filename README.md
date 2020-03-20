# fscan - file (duplicates) scanner/searcher (C#, console)
- Easily: find duplicate files / compare files.

### EXAMPLES
```
> fscan hash              = just print files that are the same
> fscan hash all del      = find files that are the same and delete them
> fscan soundf del        = find videos that have no sound and delete them
> fscan pic all mov       = find duplicate images and move them
```
...

> fscan
```
 +===[ ABOUT ]
 | ABOUT.....: file scanner/searcher
 | AUTHOR....: 0xC0LD
 | BUILT IN..: VS C# .NET
 | VERSION...: 19
 | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...

 +===[ STANDARD OPTIONS ]
 | name   = find duplicate files by name.ext
 | noext  = find duplicate files by name
 | hash   = find duplicate files by md5 checksum hash (large files will slow down the process)
 | hashf  = find duplicate files by md5 checksum hash, but use a larger byte buffer (faster)
 | byte   = find files that have the same byte size
 | pic    = find duplicate images (*.jpg, *.jpeg, *.png, *.bmp)
 | vid    = find corrupt and playable videos (uses ffmpeg)
 | vidt   = find playable videos (uses ffmpeg)
 | vidf   = find corrupt videos (uses ffmpeg)
 | sound  = print video files that have, and don't have sound/audio (uses ffmpeg)
 | soundt = find video files that have sound/audio (uses ffmpeg)
 | soundf = find video files that don't have sound/audio (uses ffmpeg)

 +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]
 | all = also scan subdirectories
 | 1   = use first file (del/mov/...)
 | 2   = use second file (del/mov/...) (default)
 | del = send the found file to recycle bin
 | mov = move the found file to a folder (fscan_dir)
 | v   = print status every second

 +===[ PRINT (only) OPTIONS ]
 | sizea   = print file sizes in ascending order
 | sized   = print file sizes in descending order
 | dsizea  = print dir size in ascending order
 | dsized  = print dir size in descending order
 | dcounta = print dir file count in ascending order
 | dcountd = print dir file count in descending order
 | long    = print files with over 260 characters in file path (too long)
 | lena    = print video length in ascending order
 | lend    = print video length in descending order
```
