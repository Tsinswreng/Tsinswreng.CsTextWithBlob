using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithStreamCases;

/// <summary>
/// Tests for <see cref="Tsinswreng.CsTextWithBlob.TextWithStream.Mk(ulong, string, Stream)"/>.
/// </summary>
public partial class TestTextWithStream {
	/// <summary>
	/// Register all cases for Pack.
	/// </summary>
	public void RegisterPack(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithStream),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithStream)],
			[nameof(Tsinswreng.CsTextWithBlob.TextWithStream.Mk)],
			"TextWithStream.Pack:"
		);
		var r = register.Register;

		r("returns object with given fields", async _ => {
			const string text = "A中";
			var payload = new MemoryStream(new byte[] { 0x01, 0x02 });
			var headerLen = (ulong)Encoding.UTF8.GetByteCount(text);
			var packed = Tsinswreng.CsTextWithBlob.TextWithStream.Mk(headerLen, text, payload);
			AssertEqual(headerLen, packed.HeaderBytesLen, "Pack/header");
			AssertEqual(text, packed.Text, "Pack/text");
			if(!ReferenceEquals(payload, packed.Payload)) {
				throw new Exception("Case 'Pack/payload-ref' failed. Payload stream reference should be preserved.");
			}
			return null;
		});

		r("throws when header length mismatches text bytes", async _ => {
			const string text = "中";
			try {
				_ = Tsinswreng.CsTextWithBlob.TextWithStream.Mk(1UL, text, new MemoryStream());
				throw new Exception("Expected ArgumentException but Pack succeeded.");
			} catch(ArgumentException) {
				// expected
			}
			return null;
		});
	}
}
