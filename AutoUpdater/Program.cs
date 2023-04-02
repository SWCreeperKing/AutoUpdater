using System.Diagnostics;
using System.IO.Compression;

public class Program
{
    public static readonly string CurrentDirectory =
        Environment.ProcessPath.Remove(Environment.ProcessPath.Replace("\\", "/").LastIndexOf('/'));

    public static void Main(string[] args)
    {
        Console.WriteLine($"STARTING UPDATER WITH ARGS: [{string.Join(",", args)}]\n\n");
        Console.WriteLine($"current path: [{CurrentDirectory}]");
        try
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Arguments are incorrect");
                Continue();
                return;
            }

            var downloadSite = args[0];
            var downloadFolder = args[1];
            var closeOnComplete = args.Length > 2 && bool.Parse(args[2]);
            var copySource = args.Length > 3 ? args[3] : null;
            var runProgramPath = args.Length > 4 ? args[4] : null;

            var fileName = downloadSite.Split('/')[^1];
            Console.WriteLine($"Downloading: [{fileName}]");
            if (File.Exists(fileName))
            {
                CleanFile(fileName);
            }

            Download(downloadSite, fileName).GetAwaiter().GetResult();
            Console.WriteLine($"Downloaded: [{fileName}]");

            UnZip(downloadSite.Split('.')[^1].ToLower(), fileName, downloadFolder);

            if (copySource is not (null or "null"))
            {
                MovePath(copySource, downloadFolder);
                Directory.Delete(copySource);
            }

            if (runProgramPath is not null)
            {
                Process.Start(runProgramPath, "update-complete");
            }

            if (closeOnComplete) return;
            Continue();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception Found!");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e);
            Console.ResetColor();
            Continue();
        }
    }

    private static async Task Download(string url, string fileName)
    {
        using var client = new HttpClient();
        await using var clientStream = await client.GetStreamAsync(url);
        await using var fileStream = new FileStream(fileName, FileMode.CreateNew);
        await clientStream.CopyToAsync(fileStream);
    }

    private static void MovePath(string path, string dest)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            var newFilePath = $"{dest}{file[file.Replace("\\", "/").LastIndexOf('/')..]}";

            if (File.Exists(newFilePath))
            {
                CleanFile(newFilePath);
            }

            File.Move(file, newFilePath);
        }

        foreach (var dir in Directory.GetDirectories(path))
        {
            var newDir = $"{dest}{dir[dir.Replace("\\", "/").LastIndexOf('/')..]}";

            if (Directory.Exists(newDir))
            {
                CleanDir(newDir);
            }

            Directory.Move(dir, newDir);
        }
    }

    private static void CleanDir(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            CleanFile(file);
        }

        foreach (var subDir in Directory.GetDirectories(dir))
        {
            CleanDir(subDir);
        }

        Directory.Delete(dir);
    }

    private static void CleanFile(string file)
    {
        while (true)
        {
            try
            {
                File.Delete(file);
                break;
            }
            catch (AccessViolationException)
            {
                Console.WriteLine($"file: [{file}] is in use, waiting 2s to try again . . .");
                Task.Delay(2000).GetAwaiter().GetResult();
            }
        }
    }

    private static void UnZip(string zipType, string file, string dest)
    {
        switch (zipType)
        {
            case "zip":
                Console.WriteLine("Unzipping");
                ZipFile.ExtractToDirectory(file, dest, true);
                File.Delete(file);
                Console.WriteLine("Unzipped");
                break;
        }
    }

    private static void Continue()
    {
        Console.WriteLine("Downloader Finished");
        Console.WriteLine("Press Enter to continue . . .");
        Console.ReadLine();
    }
}