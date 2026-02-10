using System.IO;
using System.Text;
using System.Windows;
using static OLLM.Constants;

namespace OLLM.Initialization {
	public static class EnsureModelsArePresent {

		/// <summary>
		/// Checks whether all required models (Mistral3 and nomic-embed-text) are present and accessible.
		/// </summary>
		internal static (string? modelPath, string? embedModelPath) EnsureRequiredModelsArePresent() {
			string? modelPathToReturn = null;

			// Construct a detailed message to show the user if neither of the required models could be found.
			StringBuilder potentialFriendlyUserErrorMessage = new();

			// Attempt to retrieve the Mistral model ONNX
			if (!TryRequiredModelIsPresent(_preBuildModelPath, out string? modelPathToUse) && modelPathToUse == null) {
				potentialFriendlyUserErrorMessage.AppendLine(
					$"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{modelPathToUse}");
			}

			// Attempt to retrieve the embedding model ONNX
			if (!TryRequiredModelIsPresent(_preBuildEmbedModelDirectory, out string? embedModelPathToUse) ||
				embedModelPathToUse == null) {
				potentialFriendlyUserErrorMessage.AppendLine(
					$"{_userFriendlyModelDirectoryErrorResponse}{Environment.NewLine}{embedModelPathToUse}");
			}

			if (modelPathToReturn == null && embedModelPathToUse == null) {
				MessageBox.Show(potentialFriendlyUserErrorMessage.ToString(),
					_userFriendlyErrorOccurredTryingToLoadModels);
			}

			return (modelPathToUse!, embedModelPathToUse!);
		}

		/// <summary>
		/// Checks whether the required model directories are present at either the specified debug path or its published location.
		/// </summary>
		internal static bool TryRequiredModelIsPresent(string debugPath, out string? pathToUse) {
			pathToUse = null;
			if (debugPath == _preBuildEmbedModelDirectory) {
				#region Verify embed model is present (check for debug mode first, then assume release/published)
				if (!Directory.Exists(debugPath)) {
					// Try the embed model from the published directory instead of the debugging directory:
					pathToUse = debugPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();
					if (!Directory.Exists(pathToUse)) {
						// If we still cannot find the required model(s) then show the user a friendly message and return false.
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{pathToUse}");
						return false;
					}
				} else {
					pathToUse = debugPath;
				}
				#endregion
			} else {
				#region Verify LLM is present (check for debug mode first, then assume release/published)
				if (!Directory.Exists(debugPath)) {
					// Assume the program is published and the directory is close to the executable.
					string publishedLocation = debugPath.TrimStart($"{AppContext.BaseDirectory}..\\..\\..").ToString();

					if (!Directory.Exists(publishedLocation)) {
						// If we still cannot find the required model(s) then show the user a friendly message and return false.
						MessageBox.Show($"{_userFriendlyModelDirectoryErrorResponse}{publishedLocation}");
						return false;
					}
				} else {
					pathToUse = debugPath;
				}
				#endregion
			}

			return pathToUse != null;
		}
	}
}
