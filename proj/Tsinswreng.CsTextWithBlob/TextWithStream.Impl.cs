using System.Buffers.Binary;
using System.Text;

namespace Tsinswreng.CsTextWithBlob;

public partial class TextWithStream : ITextWithStream {
	public partial TextWithStream() {
		HeaderBytesLen = 0;
		Text = string.Empty;
		Payload = Stream.Null;
	}

	/// Build a packet object from already-prepared fields.
	/// <param name="HeaderBytesLen">UTF-8 byte length of <paramref name="Text"/>.</param>
	/// <param name="Text">Text part.</param>
	/// <param name="Payload">Remaining payload stream.</param>
	/// <returns>Packet object for later serialization/processing.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="Payload"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when header byte length does not match UTF-8 byte count of text.</exception>
	public static partial TextWithStream Mk(
		u64 HeaderBytesLen,
		string Text,
		Stream Payload
	) {
		if(Payload is null) {
			throw new ArgumentNullException(nameof(Payload));
		}
		var expected = (u64)Encoding.UTF8.GetByteCount(Text);
		if(HeaderBytesLen != expected) {
			throw new ArgumentException(
				$"{nameof(HeaderBytesLen)} ({HeaderBytesLen}) does not match UTF-8 byte count ({expected}).",
				nameof(HeaderBytesLen)
			);
		}
		return new TextWithStream {
			HeaderBytesLen = HeaderBytesLen,
			Text = Text,
			Payload = Payload
		};
	}
	
	public static partial TextWithStream MkUtf8(
		string Text,
		Stream Payload
	){
		return Mk(
			(u64)Encoding.UTF8.GetByteCount(Text),
			Text,
			Payload
		);
	}

	/// Parse one packet from stream: 8-byte big-endian text length, UTF-8 text, and the remaining bytes as payload.
	/// <param name="stream">Input packet stream.</param>
	/// <returns>Parsed packet with payload as the remaining part of <paramref name="stream"/> (no copy).</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when stream content is shorter than required header/text bytes.</exception>
	/// <exception cref="OverflowException">Thrown when text length cannot fit into <see cref="int"/>.</exception>
	public static partial TextWithStream Unpack(Stream stream) {
		if(stream is null) {
			throw new ArgumentNullException(nameof(stream));
		}

		Span<byte> header = stackalloc byte[8];
		int readHead = stream.ReadAtLeast(header, 8, throwOnEndOfStream: false);
		if(readHead < 8) {
			throw new ArgumentException("Stream does not contain full 8-byte header.", nameof(stream));
		}

		ulong textByteCountU = BinaryPrimitives.ReadUInt64BigEndian(header);
		int textByteCount = checked((int)textByteCountU);

		byte[] textBytes = new byte[textByteCount];
		int readText = stream.ReadAtLeast(textBytes.AsSpan(), textByteCount, throwOnEndOfStream: false);
		if(readText < textByteCount) {
			throw new ArgumentException("Stream does not contain full text bytes.", nameof(stream));
		}

		string text = Encoding.UTF8.GetString(textBytes);

		return new TextWithStream {
			HeaderBytesLen = textByteCountU,
			Text = text,
			Payload = stream
		};
	}

	public static async partial Task<TextWithStream> UnpackAsy(Stream stream, CT Ct) {
		if(stream is null) {
			throw new ArgumentNullException(nameof(stream));
		}

		var header = new byte[8];
		int readHead = await ReadAtLeastAsy(stream, header.AsMemory(), 8, Ct);
		if(readHead < 8) {
			throw new ArgumentException("Stream does not contain full 8-byte header.", nameof(stream));
		}

		ulong textByteCountU = BinaryPrimitives.ReadUInt64BigEndian(header);
		int textByteCount = checked((int)textByteCountU);

		var textBytes = new byte[textByteCount];
		int readText = await ReadAtLeastAsy(stream, textBytes.AsMemory(), textByteCount, Ct);
		if(readText < textByteCount) {
			throw new ArgumentException("Stream does not contain full text bytes.", nameof(stream));
		}

		string text = Encoding.UTF8.GetString(textBytes);

		return new TextWithStream {
			HeaderBytesLen = textByteCountU,
			Text = text,
			Payload = stream
		};
	}

	private static async Task<int> ReadAtLeastAsy(Stream stream, Memory<byte> buffer, int minimumBytes, CT Ct) {
		if(minimumBytes <= 0) {
			return 0;
		}
		int totalRead = 0;
		while(totalRead < minimumBytes) {
			int n = await stream.ReadAsync(buffer.Slice(totalRead), Ct);
			if(n == 0) {
				break;
			}
			totalRead += n;
		}
		return totalRead;
	}
}

public static partial class ExtnTextWithStream {
	/// Serialize packet to stream as: 8-byte big-endian text length + UTF-8 text bytes + payload bytes.
	/// <returns>A readable memory stream positioned at start.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <see cref="TextWithStream.Payload"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <see cref="TextWithStream.HeaderBytesLen"/> does not match UTF-8 byte count of <see cref="TextWithStream.Text"/>.</exception>
	public static partial Stream ToStream(this ITextWithStream z) {
		if(z.Payload is null) {
			throw new ArgumentNullException(nameof(z.Payload));
		}

		var text = z.Text ?? string.Empty;
		byte[] textBytes = Encoding.UTF8.GetBytes(text);
		ulong expected = (ulong)textBytes.Length;
		if(z.HeaderBytesLen != expected) {
			throw new ArgumentException(
				$"{nameof(z.HeaderBytesLen)} ({z.HeaderBytesLen}) does not match UTF-8 byte count ({expected}).",
				nameof(z.HeaderBytesLen)
			);
		}

		byte[] headAndText = new byte[8 + textBytes.Length];
		BinaryPrimitives.WriteUInt64BigEndian(headAndText.AsSpan(0, 8), z.HeaderBytesLen);
		textBytes.CopyTo(headAndText.AsSpan(8));
		var prefix = new MemoryStream(headAndText, writable: false);

		if(z.Payload.CanSeek) {
			z.Payload.Position = 0;
		}
		return new ChainedReadStream(prefix, z.Payload);
	}
}

file sealed class ChainedReadStream : Stream {
	private readonly Stream _first;
	private readonly Stream _second;
	private bool _firstDone;
	private long _position;

	public ChainedReadStream(Stream first, Stream second) {
		_first = first ?? throw new ArgumentNullException(nameof(first));
		_second = second ?? throw new ArgumentNullException(nameof(second));
	}

	public override bool CanRead => true;
	public override bool CanSeek => false;
	public override bool CanWrite => false;
	public override long Length => throw new NotSupportedException();
	public override long Position {
		get => _position;
		set => throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count) {
		return Read(buffer.AsSpan(offset, count));
	}

	public override int Read(Span<byte> buffer) {
		if(buffer.Length == 0) {
			return 0;
		}

		int totalRead = 0;
		if(!_firstDone) {
			int r1 = _first.Read(buffer);
			totalRead += r1;
			if(r1 == buffer.Length) {
				_position += totalRead;
				return totalRead;
			}
			_firstDone = r1 == 0 || _first.Position >= _first.Length;
			buffer = buffer.Slice(r1);
		}
		if(buffer.Length > 0) {
			int r2 = _second.Read(buffer);
			totalRead += r2;
		}
		_position += totalRead;
		return totalRead;
	}

	public override void Flush() => throw new NotSupportedException();
	public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
	public override void SetLength(long value) => throw new NotSupportedException();
	public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

	protected override void Dispose(bool disposing) {
		if(disposing) {
			_first.Dispose();
		}
		base.Dispose(disposing);
	}
}
