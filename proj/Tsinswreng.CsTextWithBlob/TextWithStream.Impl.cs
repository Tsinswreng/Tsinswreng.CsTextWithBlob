using System.Buffers.Binary;
using System.Text;

namespace Tsinswreng.CsTextWithBlob;

public partial class TextWithStream : ITextWithStream {
	public partial TextWithStream() {
		HeaderBytesLen = 0;
		Text = string.Empty;
		Payload = Stream.Null;
	}

	/// <summary>
	/// Build a packet object from already-prepared fields.
	/// </summary>
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

	/// <summary>
	/// Parse one packet from stream: 8-byte big-endian text length, UTF-8 text, and the remaining bytes as payload.
	/// </summary>
	/// <param name="stream">Input packet stream.</param>
	/// <returns>Parsed packet with payload copied into a readable memory stream.</returns>
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

		var payloadMs = new MemoryStream();
		stream.CopyTo(payloadMs);
		payloadMs.Position = 0;

		return new TextWithStream {
			HeaderBytesLen = textByteCountU,
			Text = text,
			Payload = payloadMs
		};
	}
}
