using Microsoft.Extensions.VectorData;

namespace UI.Memory {
	public class Discussion {
		[VectorStoreKey] public long Id { get; set; }
		[VectorStoreData] public required string Text { get; set; }
		[VectorStoreVector(384)] public ReadOnlyMemory<float> Vector { get; set; }
		[VectorStoreData] public long UnixTimeMilliseconds { get; set; }

		//VectorStoreCollectionDefinition vectorStoreCollectionDefinition = new() {
		//	Fields = new List<VectorStoreProperty>
		//	{
		//		new VectorStoreProperty("HotelId", typeof(string)),
		//		new VectorStoreDataFieldDefinition("HotelName", typeof(string)) { IsFilterable = true },
		//		new VectorStoreVectorFieldDefinition("DescriptionEmbedding", typeof(ReadOnlyMemory<float>))
		//		{
		//			Dimensions = 1536,
		//			DistanceFunction = DistanceFunction.CosineSimilarity
		//		}
		//	}
		//};
	}

}
