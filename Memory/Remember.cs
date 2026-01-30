namespace UI.Memory {
	using Microsoft.Extensions.AI;
	using Microsoft.Extensions.VectorData;
	using Microsoft.SemanticKernel.Connectors.SqliteVec;

	internal class Remember : IDisposable {
		internal const string _db = "Data Source=chat_history.db";
		internal const string _discussions = "discussions";

		internal static SqliteVectorStore? _vectorStore;

		internal static SqliteCollection<long, Discussion>? _memories;
		internal static IEmbeddingGenerator<string, Embedding<float>>? _embedder;

		public static async Task InitAsync(
			IEmbeddingGenerator<string, Embedding<float>> embedder,
			CancellationToken ct = default) {
			_embedder = embedder;

			_vectorStore?.Dispose();
			_vectorStore = new(_db);

			_memories = _vectorStore.GetCollection<long, Discussion>(_discussions);
			await _memories.EnsureCollectionExistsAsync(ct);
		}

		public static async Task Remembering() {
			// 2. Initialize the persistent SQLite store (Survives app restarts)
			SqliteCollection<ulong, Discussion> memories = _vectorStore!.GetCollection<ulong, Discussion>(_discussions);
			await memories.EnsureCollectionExistsAsync();

			if (await memories.CollectionExistsAsync()) {

			}
		}

		/// <summary>
		/// Store a discussion that had occurred.
		/// </summary>
		public static async Task MemorizeDiscussionAsync(string text, CancellationToken ct = default) {
			if (_memories is null || _embedder is null) {
				throw new InvalidOperationException("Order of operations is incorrect. Initialize memory first.");
			}

			ReadOnlyMemory<float> vec = await _embedder.GenerateVectorAsync(text, cancellationToken: ct);

			// Use Unix ms as a simple unique-ish key. If you expect collisions, add a counter or random suffix by switching to string keys.
			long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			Discussion turn = new() {
				Id = id,
				Text = text,
				Vector = vec,
				UnixTimeMilliseconds = id
			};

			await _memories.UpsertAsync(turn, ct);
		}

		/// <summary>
		/// Think backwards for a second, try to remember before responding.
		/// </summary>
		public static async Task<IReadOnlyList<Discussion>> RememberDiscussionAsync(
			string query,
			int topK = 8,
			int candidatePool = 33,
			double halfLifeDays = 365,
			CancellationToken ct = default) {

			if (_memories is null || _embedder is null) {
				throw new InvalidOperationException("Memory is not initialized.");
			}

			ReadOnlyMemory<float> embeddingVectorQuery =
				await _embedder.GenerateVectorAsync(query, cancellationToken: ct);

			// Retrieve candidate set from vector search (bigger than topK)
			List<(Discussion Turn, double AdjustedDistance)> scored = new(candidatePool);
			long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			// When VectorSearchResult score is null fallback to ordering sequentially with an index 'rank'
			int rank = 0;
			await foreach (VectorSearchResult<Discussion> hit in _memories.SearchAsync(embeddingVectorQuery, top: candidatePool, cancellationToken: ct)) {
				Discussion turn = hit.Record;

				// With VectorSearchResult the hit.Score is distance, so lower is better
				double baseDistance = hit.Score ?? rank;
				double adjustedDistance = baseDistance;

				if (!double.IsPositiveInfinity(halfLifeDays) && halfLifeDays > 0) {
					double ageDays = Math.Max(0, (nowMs - turn.UnixTimeMilliseconds) / 86_400_000.0);
					double decay = Math.Exp(-Math.Log(2) * ageDays / halfLifeDays);

					// Older memories become effectively 'more distant'
					adjustedDistance = baseDistance / Math.Max(decay, 1e-9);
				}

				scored.Add((turn, adjustedDistance));
				rank++;
			}

			// 'scored' is filtered such that 'OrderBy' time complexity O(NlogN) doesn't get out of control
			return scored
				.OrderBy(x => x.AdjustedDistance)
				.Take(topK)
				.Select(x => x.Turn)
				.ToList();
		}

		/// <summary>
		/// Close the connections, keep the memories.
		/// </summary>
		public void Dispose() {
			_memories?.Dispose();
			_vectorStore?.Dispose();
			_memories = null;
			_vectorStore = null;
		}
	}
}
