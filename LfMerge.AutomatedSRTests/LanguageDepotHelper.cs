// Copyright (c) 2017 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Diagnostics;
using System.IO;
using Chorus.VcsDrivers.Mercurial;
using Palaso.IO;
using Palaso.Progress;

namespace LfMerge.AutomatedSRTests
{
	public class LanguageDepotHelper: IDisposable
	{
		public LanguageDepotHelper()
		{
			LdDirectory = Path.Combine(Directories.TempDir, "LanguageDepot");
			Directory.CreateDirectory(LdDirectory);
			HgRepository.CreateRepositoryInExistingDir(LdDirectory, new NullProgress());

			ApplyPatch("r0.patch");
		}

		#region Dispose functionality
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!string.IsNullOrEmpty(LdDirectory))
					DirectoryUtilities.DeleteDirectoryRobust(LdDirectory);
			}
			LdDirectory = null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~LanguageDepotHelper()
		{
			Dispose(false);
		}
		#endregion

		public static string LdDirectory { get; private set; }

		/// <summary>
		/// Applies a patch to the mock LanguageDepot hg repo
		/// </summary>
		/// <param name="patchFile">Patchfile and path, relative to <see cref="Directories.DataDir"/>.</param>
		/// <exception cref="FileNotFoundException">If <paramref name="patchFile"/> doesn't
		/// exist in <see cref="Directories.DataDir"/>.</exception>
		/// <exception cref="ApplicationException">If "hg import" returns a non-0 exit
		/// code</exception>
		public void ApplyPatch(string patchFile)
		{
			var patchPath = Path.Combine(Directories.DataDir, patchFile);
			if (!File.Exists(patchPath))
				throw new FileNotFoundException("Can't find patchfile", patchPath);

			var command = $"hg import --import-branch {patchPath}";
			var result = HgRunner.Run(command, LdDirectory, 10, new NullProgress());
			if (result.ExitCode != 0)
				throw new ApplicationException($"'{command}' returned {result.ExitCode}");
		}

	}
}
