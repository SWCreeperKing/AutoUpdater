This is a downloader, its purpose is to be ran with arguments to download

this is made for <a href="https://github.com/SWCreeperKing/RayWork/tree/master/RayWork.SelfUpdater">
RayWork.SelfUpdater</a>

### methodology

---

1. main program downloads the downloader in a sub folder
2. main program runs downloader with arguments
3. main program exits after running downloader
4. downloader will clear original directory [optional]
5. downloader will download from the internet
6. downloader will extract from zip [optional]
    - more extractions to support in future (help needed)
7. downloader will move from extracted folders to original [optional/input needed]
8. downloader will run main program
9. main program will delete downloader

## Running Arguments

---

1. download site
   - site that the download comes from, must be a direct download
2. download folder
   - can not be empty directory
   - has to be the directory to download into
3. clean path [false]
   - if to clean/delete everything in download folder
4. zip type [none]
   - what compression type is used, currently only `zip` is supported
5. close on complete [false]
   - close the downloader console when the download is complete
6. copy source [null]
   - if set then it will copy from a directory inside the unzipped folder into the download folder
7. run program path
   - the path of the program to run