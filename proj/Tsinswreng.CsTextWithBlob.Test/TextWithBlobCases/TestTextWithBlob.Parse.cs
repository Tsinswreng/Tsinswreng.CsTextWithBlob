using System.Buffers.Binary;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Tests for <see cref="Tsinswreng.CsTextWithBlob.TextWithBlob.Parse(ReadOnlyMemory{byte})"/>.
/// </summary>
public partial class TestTextWithBlob {
	/// <summary>
	/// Register all cases for Parse.
	/// </summary>
	/// <param name="Node">Target test node.</param>
	public void RegisterParse(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithBlob),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithBlob)],
			[nameof(Tsinswreng.CsTextWithBlob.TextWithBlob.Parse)],
			"Parse:"
		);
		var r = register.Register;

		r("parses a valid packet with text and binary payload", async _ => {
			const string text = "hello中";
			var blob = new byte[] { 0x10, 0x20, 0x30 };
			var data = BuildPacketBytes(text, blob);
			var actual = Tsinswreng.CsTextWithBlob.TextWithBlob.Parse(data);
			AssertEqual(text, actual.Text, "Parse/valid/text");
			AssertBytesEqual(blob, actual.Blob, "Parse/valid/blob");
			return null;
		});

		r("throws when data is shorter than header", async _ => {
			var invalid = new byte[Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen - 1];
			try {
				_ = Tsinswreng.CsTextWithBlob.TextWithBlob.Parse(invalid);
				throw new Exception("Expected ArgumentException but parsing succeeded.");
			} catch(ArgumentException) {
				// Expected path.
			}
			return null;
		});

		r("throws when text bytes are incomplete", async _ => {
			var invalid = new byte[Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen + 2];
			BinaryPrimitives.WriteUInt64BigEndian(invalid.AsSpan(0, Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen), 5UL);
			invalid[Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen] = (byte)'a';
			invalid[Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen + 1] = (byte)'b';
			try {
				_ = Tsinswreng.CsTextWithBlob.TextWithBlob.Parse(invalid);
				throw new Exception("Expected ArgumentException but parsing succeeded.");
			} catch(ArgumentException) {
				// Expected path.
			}
			return null;
		});
	}
}
