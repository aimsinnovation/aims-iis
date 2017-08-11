using System;
using System.Diagnostics;
using System.IO;

namespace Aims.Sdk.Installer.Actions
{
	public static class IisHelper
	{
		private static readonly string AppcmdPath =
			Path.Combine(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.SystemX86), "inetsrv", "Appcmd.exe");

		public static readonly string RegIIS32Path =
			Path.Combine(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Windows),
				"Microsoft.NET", "Framework", "v4.0.30319", "aspnet_regiis.exe");

		public static readonly string RegIIS64Path =
			Path.Combine(global::System.Environment.GetFolderPath(global::System.Environment.SpecialFolder.Windows),
				"Microsoft.NET", "Framework64", "v4.0.30319", "aspnet_regiis.exe");

		/// <summary>
		/// Runs Appcmd with the specified command line arguments.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <returns>
		/// Appcmd exit code.
		/// </returns>
		public static int RunAppcmd(string args)
		{
			string output;
			int exitCode = 0;
			if (!TryRunAppcmd(args, out output, out exitCode))
				throw new Exception(output);
			return exitCode;
		}

		/// <summary>
		/// Runs Appcmd with the specified command line arguments.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <param name="output">Appcmd output.</param>
		/// <returns>
		/// <c>true</c> on success; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryRunAppcmd(string args, out string output, out int exitCode)
		{
			if (!File.Exists(AppcmdPath))
			{
				output = "IIS not installed or its version is too old.";
				exitCode = -1;
				return false;
			}

			var info = new ProcessStartInfo
			{
				Arguments = args,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				FileName = AppcmdPath,
				Verb = "runas",
			};
			var process = Process.Start(info);
			process.WaitForExit();
			output = process.StandardOutput.ReadToEnd();
			if (process.ExitCode != 0)
			{
				output = String.Join(global::System.Environment.NewLine, output, process.StandardError.ReadToEnd());
			}
			exitCode = process.ExitCode;
			return process.ExitCode == 0;
		}

		/// <summary>
		/// Runs RegIIS from the specified path with the specified command line arguments.
		/// </summary>
		/// <param name="path">RegIIS path.</param>
		/// <param name="args">Command line arguments.</param>
		/// <param name="output">Appcmd output.</param>
		/// <returns>
		/// <c>true</c> on success; otherwise, <c>false</c>.
		/// </returns>
		public static bool TryRunRegIis(string path, string args, out string output)
		{
			var info = new ProcessStartInfo
			{
				Arguments = args,
				CreateNoWindow = true,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				FileName = path,
			};
			var process = Process.Start(info);
			process.WaitForExit();
			output = process.StandardOutput.ReadToEnd();
			if (process.ExitCode != 0)
			{
				output = String.Join(global::System.Environment.NewLine, output, process.StandardError.ReadToEnd());
			}
			return process.ExitCode == 0;
		}
	}
}