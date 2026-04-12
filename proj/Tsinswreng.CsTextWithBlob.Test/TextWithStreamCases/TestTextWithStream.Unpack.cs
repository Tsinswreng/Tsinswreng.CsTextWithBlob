using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithStreamCases;

/// <summary>
/// Tests for <see cref="Tsinswreng.CsTextWithBlob.TextWithStream.Unpack(Stream)"/>.
/// </summary>
public partial class TestTextWithStream {
	/// <summary>
	/// Register all cases for Unpack.
	/// </summary>
	public void RegisterUnpack(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithStream),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithStream)],
			[nameof(Tsinswreng.CsTextWithBlob.TextWithStream.Unpack)],
			"TextWithStream.Unpack:"
		);
		var r = register.Register;

		r("parses text and payload from valid stream", async _ => {
			var packet = BuildPacket("abc中", new byte[] { 0xAA, 0xBB, 0xCC });
			using var input = new MemoryStream(packet);
			var unpacked = Tsinswreng.CsTextWithBlob.TextWithStream.Unpack(input);
			AssertEqual("abc中", unpacked.Text, "Unpack/text");
			AssertEqual((ulong)System.Text.Encoding.UTF8.GetByteCount("abc中"), unpacked.HeaderBytesLen, "Unpack/header");
			var payloadBytes = ReadAllBytes(unpacked.Payload);
			if(!payloadBytes.AsSpan().SequenceEqual(new byte[] { 0xAA, 0xBB, 0xCC })) {
				throw new Exception("Case 'Unpack/payload' failed. Payload bytes mismatch.");
			}
			return null;
		});

		r("throws when stream shorter than header", async _ => {
			using var input = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });
			try {
				_ = Tsinswreng.CsTextWithBlob.TextWithStream.Unpack(input);
				throw new Exception("Expected ArgumentException but Unpack succeeded.");
			} catch(ArgumentException) {
				// expected
			}
			return null;
		});
	}
}
