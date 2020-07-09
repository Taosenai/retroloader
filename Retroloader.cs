using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Diagnostics;

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

		[STAThread]
		static void Main(string[] args)
		{
			// Find Retroarch.
			string exePath = Application.StartupPath;
			string workingPath = Directory.GetCurrentDirectory();
			string retroarchPath = null;
			if (File.Exists(Path.Combine(exePath, "retroarch.exe")))
			{
				retroarchPath = exePath;
			}
			else if (File.Exists(Path.Combine(workingPath, "retroarch.exe")))
			{
				retroarchPath = workingPath;
			}
			else if (File.Exists(Path.Combine(exePath, "..", "retroarch.exe")))
			{
				retroarchPath = Path.Combine(exePath, "..");
			}
			else if (File.Exists(Path.Combine(exePath, "..", "Retroarch", "retroarch.exe")))
			{
				retroarchPath = Path.Combine(exePath, "..", "Retroarch");
			}

			if (retroarchPath is null)
			{
				MessageBox.Show("Could not find retroarch.exe.\n\nMove Retroloader into Retroarch's folder.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			}


			// Verify valid path for target.
			if (args == null || args.Length < 1)
			{
				MessageBox.Show("You must provide a valid path to a file.\n\nEither use Retroloader as the target of an 'Open with...' request or drag & drop a file onto Retroloader's executable.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(1);
			}

			// Get target extension
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
			foreach (string corePath in Directory.EnumerateFiles(Path.Combine(retroarchPath, "cores")))
			{
				FileInfo coreFI = new FileInfo(corePath);
				string coreName = coreFI.Name.Substring(0, coreFI.Name.Length - 4);
				string infoPath = Path.Combine(retroarchPath, "info", $"{coreName}.info");
				if (!File.Exists(infoPath))
				{
					MessageBox.Show($"Could not find info file for core:\n'{infoPath}'\n\nLaunch Retroarch, then select 'Update Core Info Files' from the Online Updater menu.", "Retroloader", MessageBoxButtons.OK, MessageBoxIcon.Error);
					Environment.Exit(1);
				}
				var info = ParseRetroarchCoreInfoFile(infoPath, new[] { "corename", "supported_extensions" });
				cores.Add(new RetroarchCoreInfo(corePath, coreName, info["corename"], info["supported_extensions"].Split('|')));
			}

			var validCores = new List<RetroarchCoreInfo>();
			foreach (RetroarchCoreInfo core in cores)
			{
				if (core.SupportedExtensions.Contains(targetExtension))
				{
					validCores.Add(core);
				}
			}

			// Run target with core, or ask user which core to use if multiple cores report support for target's extension.
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
				CoreSelector cs = new CoreSelector(retroarchPath, target, validCores.ToArray());
				Application.Run(cs);
			}
			else
			{
				var core = validCores[0];
				System.Diagnostics.Debug.WriteLine(Path.Combine(retroarchPath, "retroarch.exe") + $" -L \"{core.Path}\" \"{target}\"");
				Process.Start(Path.Combine(retroarchPath, "retroarch.exe"), $" -L \"{core.Path}\" \"{target}\"");
			}
		}
	}
}
