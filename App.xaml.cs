using OLLM.Interact;
using OLLM.Memory;
using OLLM.State;
using System.Windows;
using static OLLM.Constants;
using static OLLM.Initialization.EnsureModelsArePresent;

namespace OLLM {
	internal partial class App : Application {
		internal ModelState? ModelState;
		internal EmbedderState? EmbedderState;
		internal MiniEmbedder? MiniEmbedder;

		internal static readonly LoadingWindow SplashWindow = new();

		protected override async void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);

			// Show loading screen while model attempts to load into memory
			SplashWindow.Show();
			SplashWindow.Activate();

			try {
				// (Mistral-7B or Mistral-14B) and nomic-embed-text-1-5
				(string? modelPath, string? embedModelPath) = EnsureRequiredModelsArePresent();

				if (modelPath == null || embedModelPath == null) {
					// Previous method already displayed the friendly user message and provided some guidance.
					Current.Shutdown();
					return;
				}

				MainWindow mainWindow = new();

				ModelState = new(modelPath);
				EmbedderState = new(embedModelPath);
				MiniEmbedder = new(ModelState, EmbedderState);

				mainWindow.Initialize(ModelState, EmbedderState, MiniEmbedder);

				MainWindow = mainWindow;
				mainWindow.Show();

				FinishedInitializing();

			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			}
		}

		/// <summary>
		/// Allow the main window's constructor to
		/// close the loading window after it has initialized its components and memory store.
		/// </summary>
		internal static void FinishedInitializing() => SplashWindow.Hide();
	}
}
