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
            var cleanPath = args.Length > 2 && bool.Parse(args[2]);
            var zipType = args.Length > 3 ? args[3] : "none";
            var closeOnComplete = args.Length > 4 && bool.Parse(args[4]);
            var copySource = args.Length > 5 ? args[5] : null;

            if (cleanPath)
            {
                Console.WriteLine($"Cleaning path: [{downloadFolder}]");
                CleanPath(downloadFolder);
            }

            var fileName = downloadSite.Split('/')[^1];
            Console.WriteLine($"Downloading: [{fileName}]");
            Download(downloadSite, fileName).GetAwaiter().GetResult();
            Console.WriteLine($"Downloaded: [{fileName}]");

            Console.WriteLine("Unzipping");
            UnZip(zipType, fileName, downloadFolder);
            Console.WriteLine("Unzipped");

            if (copySource is not null)
            {
                MovePath(copySource, downloadFolder);
                Directory.Delete(copySource);
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

    private static bool CleanPath(string path)
    {
        if (path == CurrentDirectory) return false;

        foreach (var file in Directory.GetFiles(path))
        {
            Console.WriteLine($"deleting [{file}]");

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

        foreach (var folder in Directory.GetDirectories(path))
        {
            if (!CleanPath(folder)) continue;

            Console.WriteLine($"deleting [{folder}]");

            while (true)
            {
                try
                {
                    Directory.Delete(folder);
                    break;
                }
                catch (AccessViolationException)
                {
                    Console.WriteLine($"folder: [{folder}] is in use, waiting 2s to try again . . .");
                    Task.Delay(2000).GetAwaiter().GetResult();
                }
            }
        }

        return true;
    }

    private static void MovePath(string path, string dest)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            File.Move(file,
                $"{dest}{file[file.Replace("\\", "/").LastIndexOf('/')..]}");
        }

        foreach (var dir in Directory.GetDirectories(path))
        {
            Directory.Move(dir,
                $"{dest}{dir[dir.Replace("\\", "/").LastIndexOf('/')..]}");
        }
    }

    private static void UnZip(string zipType, string file, string dest)
    {
        switch (zipType)
        {
            case "zip":
                ZipFile.ExtractToDirectory(file, dest);
                File.Delete(file);
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