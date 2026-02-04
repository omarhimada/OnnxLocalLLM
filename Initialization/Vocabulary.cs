using System.IO;
using System.Windows;
using static UI.Constants;

namespace UI.Initialization {
	internal static class Vocabulary {
		internal static string? GetRequiredTextDocument(string debugPath) {
			string? pathToUse = null;
			if (debugPath == _debugModeVocabTextPath) {
				#region Verify embed vocab.txt is present (check for debug mode first, then assume release/published)
				if (!File.Exists(debugPath)) {
					// Try the embed vocab.txt from the published directory instead of the debugging directory:
					pathToUse = debugPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();
					if (!File.Exists(pathToUse)) {
						// If we still cannot find the required vocab.txt then show the user a friendly message and return false.
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{pathToUse}");
						return null;
					}
				} else {
					pathToUse = debugPath;
				}
				#endregion
			}

			return pathToUse;
		}
	}
}
