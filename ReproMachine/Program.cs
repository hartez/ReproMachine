using System;
using System.Diagnostics;
using System.IO;

namespace ReproMachine
{
	internal class Program
	{
		private const string reproWorkingDirectory = @"C:\repro";
		private const string sevenZipPath = @"C:\Program Files\7-Zip\7z.exe";

		private static readonly bool _cleanMacOS = true;

		private static void Main(string[] args)
		{
			using (var watcher = SetUpWatcher(reproWorkingDirectory))
			{
				var exit = false;
				while (!exit)
				{
					var cmd = Console.ReadLine().Trim();

					switch (cmd)
					{
						case ("exit"):
							return;
						default:
							continue;
					}
				}
			}
		}

		private static void FileRenamed(object sender, RenamedEventArgs args)
		{
			Console.WriteLine($"Renamed {args.FullPath}");
		}

		private static void FileCreated(object sender, FileSystemEventArgs args)
		{
			Handle(args.FullPath);
		}

		private static void Handle(string fullPath)
		{
			var extension = Path.GetExtension(fullPath);

			if (extension != ".zip")
			{
				Console.WriteLine($"{fullPath}, nothing to do.");
				return;
			}

			var dir = Path.GetDirectoryName(fullPath);

			var destination = Path.Combine(dir, Path.GetFileNameWithoutExtension(fullPath));

			Console.WriteLine($"Dest: {destination}");

			Extract(fullPath, destination);

			if (_cleanMacOS)
			{
				CleanUpMacOSFolder(destination);
			}

			CollapseToSolutionPath(destination);
			OpenSolution(destination);
		}

		private static FileSystemWatcher SetUpWatcher(string path)
		{
			var fsw = new FileSystemWatcher(path);

			fsw.Created += FileCreated;
			fsw.Renamed += FileRenamed;

			fsw.EnableRaisingEvents = true;

			return fsw;
		}

		private static void Extract(string path, string destination)
		{
			// 7z x -o'C:\repro\ShellRepro' 'C:\repro\ShellRepro.zip'
			var args = $"/C \"\"{sevenZipPath}\" x -o\"{destination}\" \"{path}\"\"";

			Console.WriteLine(args);

			var process = new Process();
			var startInfo = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				FileName = "cmd.exe",
				Arguments = args
			};

			process.StartInfo = startInfo;
			process.Start();
			process.WaitForExit();
		}

		private static void OpenSolution(string path)
		{
			var slnFilePath = FindSolution(path);

			if (string.IsNullOrEmpty(slnFilePath))
			{
				return;
			}

			var psi = new ProcessStartInfo(slnFilePath)
			{
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private static void EnsureAndroidSetToDeploy(string path)
		{

		}

		private static bool PathTooLongViolationLikely(string path)
		{
			return false;
		}

		private static void CollapseToSolutionPath(string path)
		{
			var slnFilePath = FindSolution(path);

			if (string.IsNullOrEmpty(slnFilePath))
			{
				return;
			}

			var slnDir = Path.GetDirectoryName(slnFilePath);

			if (slnDir != path)
			{
				// We've got extra directories between the extracted folder and the solution; we can close that gap
				var temp = Path.Combine(reproWorkingDirectory, Path.GetRandomFileName());

				Directory.Move(slnDir, temp);
				Directory.Delete(path, true);
				Directory.Move(temp, path);
			}
		}

		private static string FindSolution(string path)
		{
			var slns = Directory.GetFiles(path, "*.sln", SearchOption.AllDirectories);

			if (slns.Length == 0)
			{
				Console.Error.WriteLine($"Path {path} did not include any Solution files.");
				return string.Empty;
			}

			if (slns.Length > 1)
			{
				Console.Error.WriteLine($"Path {path} had more than one Solution file; you'll have to do this one manually.");
				return string.Empty;
			}

			return slns[0];
		}

		private static void CleanUpMacOSFolder(string path)
		{
			var macosFolder = Path.Combine(path, "__MACOSX");
			if (Directory.Exists(macosFolder))
			{
				Directory.Delete(macosFolder, true);
			}
		}
	}
}
