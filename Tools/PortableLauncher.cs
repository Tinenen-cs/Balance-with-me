using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// Single-file, installer-free wrapper around the complete official Unity build.
internal static class PortableLauncher
{
    static string Quote(string value)
    {
        var result=new StringBuilder("\"");int slashes=0;
        foreach(char c in value)
        {
            if(c=='\\') { slashes++;continue; }
            if(c=='\"') { result.Append('\\',slashes*2+1);result.Append(c);slashes=0;continue; }
            result.Append('\\',slashes);slashes=0;result.Append(c);
        }
        result.Append('\\',slashes*2);return result.Append('"').ToString();
    }
    [STAThread] static int Main(string[] args)
    {
        bool created;
        using(var mutex=new Mutex(true,"Local\\BalanceWithMePortable",out created))
        {
            if(!created) { MessageBox.Show("Balance with me is already running.","Balance with me");return 0; }
            string tempRoot=Path.GetFullPath(Path.GetTempPath());
            string folder=Path.Combine(tempRoot,"BalanceWithMe-play-"+Guid.NewGuid().ToString("N"));
            try
            {
                Application.EnableVisualStyles();
                using(var loading=new Form())
                {
                    loading.Text="Balance with me";loading.Width=390;loading.Height=140;
                    loading.StartPosition=FormStartPosition.CenterScreen;loading.FormBorderStyle=FormBorderStyle.FixedDialog;
                    loading.ControlBox=false;
                    loading.Controls.Add(new Label { Text="Preparing your portable game...\nNo installation or administrator access required.",Dock=DockStyle.Fill,TextAlign=System.Drawing.ContentAlignment.MiddleCenter });
                    loading.Show();Application.DoEvents();
                    Directory.CreateDirectory(folder);
                    string prefix=Path.GetFullPath(folder)+Path.DirectorySeparatorChar;
                    using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("game.zip"))
                    using(var archive=new ZipArchive(stream,ZipArchiveMode.Read))
                    {
                        foreach(var entry in archive.Entries)
                        {
                            string target=Path.GetFullPath(Path.Combine(folder,entry.FullName));
                            if(!target.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new IOException("Invalid archive entry.");
                            if(entry.Name.Length==0) { Directory.CreateDirectory(target);continue; }
                            Directory.CreateDirectory(Path.GetDirectoryName(target));
                            using(var source=entry.Open())using(var file=File.Create(target))source.CopyTo(file);
                            Application.DoEvents();
                        }
                    }
                    string exe=Path.Combine(folder,"BalanceWithMe.exe");
                    if(!File.Exists(exe) || !File.Exists(Path.Combine(folder,"UnityPlayer.dll")))throw new IOException("The game package is incomplete.");
                    var arguments=new StringBuilder();
                    foreach(string arg in args) { if(arguments.Length>0)arguments.Append(' ');arguments.Append(Quote(arg)); }
                    loading.Hide();
                    using(var game=Process.Start(new ProcessStartInfo(exe,arguments.ToString()) { WorkingDirectory=folder,UseShellExecute=false }))
                    { game.WaitForExit();return game.ExitCode; }
                }
            }
            catch(Exception error)
            {
                MessageBox.Show("The portable game could not start.\n\n"+error.Message,"Balance with me",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;
            }
            finally
            {
                // Only remove this launch's generated directory, directly beneath the OS temp directory.
                if(Path.GetDirectoryName(Path.GetFullPath(folder)).TrimEnd('\\')==tempRoot.TrimEnd('\\') && Path.GetFileName(folder).StartsWith("BalanceWithMe-play-",StringComparison.Ordinal))
                    try { Directory.Delete(folder,true); } catch(IOException) { } catch(UnauthorizedAccessException) { }
                mutex.ReleaseMutex();
            }
        }
    }
}
