using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
namespace OLLM.Memory {
	public class Discussion {
		[VectorStoreKey] public long Id { get; set; }
		[VectorStoreData] public required string Text { get; set; }
		[VectorStoreVector(384)] public required GeneratedEmbeddings<Embedding<float>> Vector { get; set; }
		[VectorStoreData] public long UnixTimeMilliseconds { get; set; }
	}
}