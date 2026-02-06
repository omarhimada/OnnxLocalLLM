using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.IO;
using System.Windows;
using UI.Initialization;
using UI.Memory;
using static UI.Constants;
using static UI.Initialization.EnsureModelsArePresent;

namespace UI {
	internal partial class App : Application {
		private readonly ServiceProvider _serviceProvider;

		internal Model? _model;
		internal Tokenizer? _tokenizer;
		internal GeneratorParams? _generatorParams;

		internal static readonly LoadingWindow _splashWindow = new();
		internal LocalMiniLmEmbeddingGenerator? _localMiniLmEmbeddingGenerator;

		#region Initialize these once
		internal int _padId { get; set; }
		internal int _clsId { get; set; }
		internal int _unkId { get; set; }
		internal int _sepId { get; set; }

		internal string _vocabularyPath = string.Empty;

		internal Dictionary<string, int> _vocabulary = [];

		internal string? _modelPath { get; set; }
		internal string? _embedModelPath { get; set; }
		#endregion

		private InferenceSession? _session { get; set; }

		internal App() {
			(_modelPath, _embedModelPath) = EnsureRequiredModelsArePresent();
			if (_modelPath == null || _embedModelPath == null) {
				// Previous method already displayed the friendly user message and provided some guidance.
				Current.Shutdown();
				return;
			}

			ServiceCollection services = new();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();
		}

		private void ConfigureServices(IServiceCollection services) {
			#region DI
			services.AddSingleton<LocalMiniLmEmbeddingGenerator>(sp => {
				SessionOptions options = new();

				#region Try DML, then CUDA, and finally fallback to CPU
				try {
					options.AppendExecutionProvider_DML();
				} catch (NotSupportedException) {
					MessageBox.Show(_userFriendlyErrorOccurredLoadingDMLProvider);
					try {
						options.AppendExecutionProvider_CUDA();
					} catch (Exception) {
						MessageBox.Show(_userFriendlyErrorOccurredLoadingCUDAProvider);
						options.AppendExecutionProvider_CPU();
					}
				} catch (Exception) {
					options.AppendExecutionProvider_CPU();
				}
				#endregion

				_session = new InferenceSession(_debugModeEmbedModelPath, options);
				_vocabularyPath = Vocabulary.GetRequiredTextDocument(_debugModeVocabTextPath) ?? string.Empty;

				if (!string.IsNullOrEmpty(_vocabularyPath)) {
					_vocabulary = new(StringComparer.Ordinal);
					IEnumerable<(string?, int i)> allLines =
						File.ReadLines(_vocabularyPath).Select((l, i) => (l?.Trim(), i));

					foreach ((string? line, int index) in allLines) {
						if (string.IsNullOrEmpty(line)) {
							continue;
						}

						_vocabulary.TryAdd(line, index);
					}

					int GetIdOrDefault(string token, int fallback) => _vocabulary.GetValueOrDefault(token, fallback);

					_padId = GetIdOrDefault(_pad, 0);
					_unkId = GetIdOrDefault(_unk, 100);
					_clsId = GetIdOrDefault(_cls, GetIdOrDefault(_mistral3TokenStartTurn, 101));
					_sepId = GetIdOrDefault(_sep, GetIdOrDefault(_mistral3TokenStop, 102));
				}
				_localMiniLmEmbeddingGenerator = new LocalMiniLmEmbeddingGenerator(_embedModelPath!, _vocabularyPath, maxTokenLength: 4096, options);
				return _localMiniLmEmbeddingGenerator;
			});

			services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
				sp.GetRequiredService<LocalMiniLmEmbeddingGenerator>());

			// Aso register MainWindow as a service in order to utilize DI
			services.AddTransient<MainWindow>();
			#endregion
		}

		protected override async void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			AppContext.SetSwitch(_appContextSwitchForSelectionBrush, false);

			// Show loading screen while model attempts to load into memory
			_splashWindow.Show();
			_splashWindow.Activate();

			try {
				#region Loading: 2-3 seconds of loading the model into RAM before the window appears...
				Config config = new(_modelPath);
				//config.AppendProvider("dml");

				// ~ 5.01 seconds
				_model = new(config);

				// ~ 0.0508 seconds
				_tokenizer = new(_model);

				// ~ 0.0002 seconds
				_generatorParams = new(_model);
				#endregion

				MainWindow mainWindow = new();
				mainWindow.Initialize(_model, _tokenizer, _generatorParams);
				try {
					mainWindow.PostInitialize(_serviceProvider.GetRequiredService<LocalMiniLmEmbeddingGenerator>());
				} catch (Exception) {
					// Continue;
				}

				mainWindow.Show();

			} catch (Exception exception) {
				MessageBox.Show($"{_userFriendlyErrorOccurredDuringInitialization}\r\n{exception.Message}");
				Shutdown();
			}
		}

		/// <summary>
		/// Allow the main window's constructor to
		/// close the loading window after it has initialized its components and memory store.
		/// </summary>
		internal static void FinishedInitializing() {
			_splashWindow.Hide();
		}
	}
}
