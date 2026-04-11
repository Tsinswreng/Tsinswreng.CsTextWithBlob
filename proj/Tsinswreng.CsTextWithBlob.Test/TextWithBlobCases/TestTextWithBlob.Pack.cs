using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Tests for <see cref="Tsinswreng.CsTextWithBlob.TextWithMemory.Pack(string, ReadOnlyMemory{byte})"/>.
/// </summary>
public partial class TestTextWithBlob {
	/// <summary>
	/// Register all cases for Pack.
	/// </summary>
	/// <param name="Node">Target test node.</param>
	public void RegisterPack(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithBlob),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithMemory)],
			[nameof(Tsinswreng.CsTextWithBlob.TextWithMemory.Pack)],
			"Pack:"
		);
		var r = register.Register;

		r("sets utf8 byte length and preserves fields", async _ => {
			const string text = "A中";
			var blob = new byte[] { 0x01, 0x02, 0x03 };
			var actual = Tsinswreng.CsTextWithBlob.TextWithMemory.Pack(text, blob);
			ulong expectedHeaderBytesLen = (ulong)Encoding.UTF8.GetByteCount(text);
			AssertEqual(expectedHeaderBytesLen, actual.HeaderBytesLen, "Pack/header-byte-len");
			AssertEqual(text, actual.Text, "Pack/text");
			AssertBytesEqual(blob, actual.Blob, "Pack/blob");
			return null;
		});

		r("supports empty binary payload", async _ => {
			const string text = "hello";
			var actual = Tsinswreng.CsTextWithBlob.TextWithMemory.Pack(text, ReadOnlyMemory<byte>.Empty);
			AssertEqual((ulong)Encoding.UTF8.GetByteCount(text), actual.HeaderBytesLen, "Pack/empty/header-byte-len");
			AssertEqual(0, actual.Blob.Length, "Pack/empty/blob-len");
			return null;
		});
	}
}
