
namespace EventStore.Plugins.Transforms;

public interface IChunkWriteTransform {
	ChunkDataWriteStream TransformData(ChunkDataWriteStream stream);
	ValueTask CompleteData(int footerSize, int alignmentSize, CancellationToken token = default);
	ValueTask<int> WriteFooter(ReadOnlyMemory<byte> footer, CancellationToken token = default);
}
