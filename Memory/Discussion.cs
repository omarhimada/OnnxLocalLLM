using Microsoft.Extensions.VectorData;

namespace UI.Memory {
	internal class Discussion {
		[VectorStoreKey] internal long Id { get; set; }
		[VectorStoreData] internal required string Text { get; set; }
		[VectorStoreVector(384)] internal ReadOnlyMemory<float> Vector { get; set; }
		[VectorStoreData] public long UnixTimeMilliseconds { get; set; }
	}
}
