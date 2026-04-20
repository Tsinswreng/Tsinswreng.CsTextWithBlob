using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithStreamCases;

/// Tests for <see cref="Tsinswreng.CsTextWithBlob.ExtnTextWithStream.ToStream(Tsinswreng.CsTextWithBlob.TextWithStream)"/>.
public partial class TestTextWithStream {
	/// Register all cases for ToStream.
	public void RegisterToStream(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithStream),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithStream)],
			[nameof(Tsinswreng.CsTextWithBlob.ExtnTextWithStream.ToStream)],
			"TextWithStream.ToStream:"
		);
		var r = register.Register;

		r("writes header text and payload to packet stream", async _ => {
			const string text = "ab中";
			var payloadBytes = new byte[] { 0x10, 0x20, 0x30 };
			using var payload = new MemoryStream(payloadBytes);
			var packet = Tsinswreng.CsTextWithBlob.TextWithStream.MkUtf8(text, payload);

			using var stream = packet.ToStream();
			var actual = ReadAllBytes(stream);
			var expected = BuildPacket(text, payloadBytes);
			if(!actual.AsSpan().SequenceEqual(expected)) {
				throw new Exception("Case 'ToStream/packet-bytes' failed. Serialized packet bytes mismatch.");
			}
			return null;
		});

		r("throws when header length mismatches utf8 text bytes", async _ => {
			var packet = new Tsinswreng.CsTextWithBlob.TextWithStream {
				HeaderBytesLen = 1UL,
				Text = "中",
				Payload = new MemoryStream()
			};
			try {
				_ = packet.ToStream();
				throw new Exception("Expected ArgumentException but ToStream succeeded.");
			} catch(ArgumentException) {
				// expected
			}
			return null;
		});
	}
}
