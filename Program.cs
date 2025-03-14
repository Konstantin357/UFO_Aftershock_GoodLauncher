using System.Reflection;
using System.Runtime.InteropServices;

[DllImport("kernel32", SetLastError = true)]
static extern IntPtr LoadLibrary(string lpFileName);
static void ExtractEmbeddedResource(string resourceName, string outputPath)
{
    Assembly assembly = Assembly.GetExecutingAssembly();
    string[] names = assembly.GetManifestResourceNames();
    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
    if (stream == null) return;
    using var outFile = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
    stream.CopyTo(outFile);
}

string nativeDllname = Environment.Is64BitProcess ? "steam_api64.dll" : "steam_api.dll";
string txtName = "steam_appid.txt";

// Extract to current directory
ExtractEmbeddedResource("UFO_Aftershock_GoodLauncher." + nativeDllname, nativeDllname);
ExtractEmbeddedResource("UFO_Aftershock_GoodLauncher." + txtName, txtName);
LoadLibrary(nativeDllname);

if (!Steamworks.Packsize.Test())
{
    Console.WriteLine("Packsize Test failed.");
}

if (!Steamworks.DllCheck.Test())
{
    Console.WriteLine("DllCheck Test failed.");
}

try
{
    if (Steamworks.SteamAPI.Init())
    {
        Console.WriteLine("Steamworks initialized successfully!");
        const uint appID = 289500;

        Steamworks.AppId_t appID_t = new Steamworks.AppId_t(appID);
        bool isInstalled = Steamworks.SteamApps.BIsAppInstalled(appID_t);
        if (isInstalled)
        {
            string installDir;
            uint result = Steamworks.SteamApps.GetAppInstallDir(appID_t, out installDir, 1024);
            string gameEXE = "UFO.exe";

            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WorkingDirectory = installDir;
            startInfo.FileName = installDir + "\\" + gameEXE;
            for (int i = 0; i < args.Length; i++)
            {
                startInfo.Arguments += " " + args[i];
            }
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.CreateNoWindow = false;
            System.Diagnostics.Process? UFOProcess = System.Diagnostics.Process.Start(startInfo);

            Console.WriteLine($"Game is installed in: {installDir}");
        }
        else
        {
            Console.WriteLine("Game is not installed.");
        }

        // Shutdown Steamworks
        Steamworks.SteamAPI.Shutdown();

        return;
    }
    else
    {
        Console.WriteLine("Failed to initialize Steamworks.");
    }
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Console.WriteLine("Failed to initialize Steamworks.");
}

Thread.Sleep(5000);