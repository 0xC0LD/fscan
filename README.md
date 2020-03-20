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
 | VERSION...: 20
 | USAGE.....: fscan.exe <file/command> <command2> <cmd3> <cmd4> ...

 +===[ STANDARD OPTIONS ]
 | name    = find duplicate files by name.ext
 | noext   = find duplicate files by name
 | hash    = find duplicate files by md5 checksum hash
 | hashbuf = find duplicate files by md5 checksum hash, but use a larger byte buffer
 | hashexe = find duplicate files by md5 checksum hash, but use the 'md5sum.exe' to get the file hash
 | byte    = find files that have the same byte size
 | pic     = find duplicate images (*.jpg, *.jpeg, *.png, *.bmp)
 | vid     = find corrupt and playable videos (uses ffmpeg)
 | vidt    = find playable videos (uses ffmpeg)
 | vidf    = find corrupt videos (uses ffmpeg)
 | sound   = print video files that have, and don't have sound/audio (uses ffmpeg)
 | soundt  = find video files that have sound/audio (uses ffprobe)
 | soundf  = find video files that don't have sound/audio (uses ffprobe)

 +===[ RUNTIME OPTIONS / OPTIONS WHILE PROCESSING ]
 | all = also scan subdirectories
 | 1   = use first file (del/mov/...)
 | 2   = use second file (del/mov/...) (default)
 | del = send the found file to recycle bin
 | mov = move the found file to a folder (fscan_dir)
 | v   = print status every second
 | vv  = print status every 500 ms
 | vvv = print status every 250 ms

 +===[ PRINT (only) OPTIONS ]
 | sizea    = print file sizes in ascending order
 | sized    = print file sizes in descending order
 | dsizea   = print directory size in ascending order
 | dsized   = print directory size in descending order
 | dcounta  = print directory files count in ascending order
 | dcountd  = print directory files count in descending order
 | rdcounta = print directory (+subdirs) files count in ascending order
 | rdcountd = print directory (+subdirs) files count in descending order
 | long     = print files with over 260 characters in file path (too long)
 | lena     = print video length in ascending order
 | lend     = print video length in descending order
```
