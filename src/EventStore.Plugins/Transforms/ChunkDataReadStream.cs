using System.Runtime.CompilerServices;

namespace EventStore.Plugins.Transforms;

public class ChunkDataReadStream(Stream chunkFileStream) : Stream {
	public Stream ChunkFileStream => chunkFileStream;

	public sealed override bool CanRead => true;
	public sealed override bool CanSeek => true;
	public sealed override bool CanWrite => false;
	public sealed override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();
	public override void Write(ReadOnlySpan<byte> buffer) => throw new InvalidOperationException();
	public sealed override void WriteByte(byte value) => throw new InvalidOperationException();

	public sealed override void Flush() => throw new InvalidOperationException();
	public sealed override void SetLength(long value) => throw new InvalidOperationException();
	public override long Length => throw new NotSupportedException();

	public sealed override int Read(byte[] buffer, int offset, int count) {
		ValidateBufferArguments(buffer, offset, count);

		return Read(buffer.AsSpan(offset, count));
	}

	// reads must always return exactly `Span<byte>.Length` bytes as we never read past the (flushed) writer checkpoint
	public override int Read(Span<byte> buffer) => ChunkFileStream.Read(buffer);

	public sealed override int ReadByte() {
		Unsafe.SkipInit(out byte value);
		return Read(new(ref value)) is 1 ? value : -1;
	}

	// seeks need to support only `SeekOrigin.Begin`
	public override long Seek(long offset, SeekOrigin origin) {
		if (origin != SeekOrigin.Begin)
			throw new NotSupportedException();

		return ChunkFileStream.Seek(offset, origin);
	}

	public override long Position {
		get => ChunkFileStream.Position;
		set => ChunkFileStream.Position = value;
	}

	protected override void Dispose(bool disposing) {
		try {
			if (!disposing)
				return;

			chunkFileStream.Dispose();
		} finally {
			base.Dispose(disposing);
		}
	}
}
