using Microsoft.Extensions.VectorData;

namespace UI.Memory {
	public class Discussion {
		[VectorStoreKey] public long Id { get; set; }
		[VectorStoreData] public required string Text { get; set; }
		[VectorStoreVector(384)] public ReadOnlyMemory<float> Vector { get; set; }
		[VectorStoreData] public long UnixTimeMilliseconds { get; set; }
	}
}
