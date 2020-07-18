using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Retroloader
{
	/* 
	 * Class for storing data from a parsed Retroarch core's .info file.
	 */
	public class RetroarchCoreInfo
	{
		public string Path;
		public string Name;
		public string PrettyName;
		public string[] SupportedExtensions;

		public RetroarchCoreInfo(string path, string name, string prettyName, string[] supportedExtensions)
		{
			Path = path;
			Name = name;
			PrettyName = prettyName;
			SupportedExtensions = supportedExtensions;
		}
	}

	static class Retroloader
	{
		/* Parses a Retroarch core's .info file for the given keys and returns a Dictionary. */
		static Dictionary<string, string> ParseRetroarchCoreInfoFile(string path, string[] keys)
		{
			var dict = new Dictionary<string, string>();
			string[] lines = System.IO.File.ReadAllLines(path);
			foreach (string line in lines)
			{
				foreach (string key in keys)
				{
					if (line.Length > key.Length && String.Equals(key, line.Substring(0, key.Length)))
					{
						string value = line.Split('=')[1].Trim(new[] { ' ', '\\', '"' });
						dict[key] = value;
					}
				}
			}
			return dict;
		}

		/*
		 * Attempts to locate a dir containing retroarch.exe in adjacent dirs or on the PATH.
		 * Returns the dir path as a string, or null if nothing was found.
		 */
		static string FindRetroarch()
		{
			string exePath = Application.StartupPath;
			string workingDir = Directory.GetCurrentDirectory();

			string retroarchDir = null;
			if (File.Exists(Path.Combine(exePath, "retroarch.exe")))
			{
				retroarchDir = exePath;
			}
			else if (File.Exists(Path.Combine(workingDir, "retroarch.exe")))
			{
				retroarchDir = workingDir;
			}
			else if (File.Exists(Path.Combine(exePath, "..", "retroarch.exe")))
			{
				retroarchDir = Path.Combine(exePath, "..");
			}
			else if (File.Exists(Path.Combine(exePath, "..", "Retroarch", "retroarch.exe")))
			{
				retroarchDir = Path.Combine(exePath, "..", "Retroarch");
			}
			else
			{
				// Check on the path using 'where'.
				var wherePSI = new ProcessStartInfo("cmd.exe", "/c where retroarch.exe")
				{
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				var where = Process.Start(wherePSI);

				string result;
				using (var output = where.StandardOutput)
				{
					result = output.ReadToEnd().Trim();
				}

				if (!result.StartsWith("INFO"))
				{
					if (File.Exists(result))
					{
						var whereDir = Path.GetDirectoryName(result);
						// Find the real path if we have encountered a Scoop shim rather than the "real" executable.
						if (File.Exists(Path.Combine(whereDir, "retroarch.shim")))
						{
							// Shim file is one line, looks like:
							//      path = C:\Users\Ryan\AppData\Local\Scoop\apps\retroarch\current\retroarch.exe

							var shim = System.IO.File.ReadAllLines(Path.Combine(whereDir, "retroarch.shim"));
							var shimmedPath = shim[0].Remove(0, 7).Trim(); // Remove "path = "
							if (File.Exists(shimmedPath))
							{
								retroarchDir = Path.GetDirectoryName(shimmedPath);
							}
						} else
						{
							retroarchDir = whereDir;
						}

					}
				}
			}
			return retroarchDir;
		}

		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				string retroarchDir = FindRetroarch();
				if (retroarchDir is null || !Directory.Exists(retroarchDir))
				{
					MessageBox.Show("Could not find retroarch.exe.\n\nMove Retroloader into Retroarch's folder, or ensure that Retroarch is on your PATH.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}

				var retroarchPath = Path.Combine(retroarchDir, "retroarch.exe");

				// Verify valid target.
				if (args == null || args.Length < 1)
				{
					MessageBox.Show("You must provide a valid path to a file.\n\nEither use Retroloader as the target of an 'Open with...' request or drag & drop a file onto Retroloader's executable.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}

				// Get target extension.
				string target = args.Last();
				FileInfo targetFI = new FileInfo(target);
				string targetExtension = targetFI.Extension.Substring(1, targetFI.Extension.Length - 1);
				if (!File.Exists(target))
				{
					MessageBox.Show($"File \n{target}\n doesn't exist.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}

				// Parse core info files to get supported extensions per core.
				var cores = new List<RetroarchCoreInfo>();
				var coresPath = Path.Combine(retroarchDir, "cores");
				var infosPath = Path.Combine(retroarchDir, "info");
				if (!Directory.Exists(coresPath))
				{
					MessageBox.Show("Could not find the 'cores' folder in Retroarch's folder.\n\nIs Retroarch installed correctly (or are you using an unusual configuation)?.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}
				if (!Directory.Exists(infosPath))
				{
					MessageBox.Show("Could not find the 'info' folder in Retroarch's folder.\n\nLaunch Retroarch, then select 'Update Core Info Files' from the Online Updater menu.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}
				foreach (string corePath in Directory.EnumerateFiles(coresPath))
				{
					FileInfo coreFI = new FileInfo(corePath);
					string coreName = coreFI.Name.Substring(0, coreFI.Name.Length - 4);
					string infoPath = Path.Combine(infosPath, $"{coreName}.info");
					if (!File.Exists(infoPath))
					{
						MessageBox.Show($"Could not find an info file for core:\n'{infoPath}'\n\nLaunch Retroarch, then select 'Update Core Info Files' from the Online Updater menu.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
						Environment.Exit(1);
					}
					var info = ParseRetroarchCoreInfoFile(infoPath, new[] { "corename", "supported_extensions" });
					cores.Add(new RetroarchCoreInfo(corePath, coreName, info["corename"], info["supported_extensions"].Split('|')));
				}

				// Create a list of cores that support files with the target extension.
				var validCores = new List<RetroarchCoreInfo>();
				foreach (RetroarchCoreInfo core in cores)
				{
					if (core.SupportedExtensions.Contains(targetExtension))
					{
						validCores.Add(core);
					}
				}

				// If only one core supports the extension, run target with core; else, ask user which core to use.
				if (validCores.Count == 0)
				{
					string message = $"No core reported being able to load '.{targetExtension}' files.\n\nLaunch Retroarch, then install an appropriate core using the Online Updater menu.";
					string caption = "Retroloader";
					var buttons = MessageBoxButtons.OK;
					MessageBox.Show(message, caption, buttons, MessageBoxIcon.Error);
				}
				else if (validCores.Count > 1)
				{
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					CoreSelector cs = new CoreSelector(retroarchDir, target, validCores.ToArray());
					Application.Run(cs);
				}
				else
				{
					var core = validCores[0];
					Process.Start(retroarchPath, $" -L \"{core.Path}\" \"{target}\"");
				}
			}
			catch (Exception e)
			{
				MessageBox.Show($"Retroloader encountered an error.\n\nException: {e}", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
