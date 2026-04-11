using System.Buffers.Binary;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Tests for <see cref="ExtnTextWithBlob.ToByteArr{TSelf}(TSelf)"/>.
/// </summary>
public partial class TestTextWithBlob {
	/// <summary>
	/// Register all cases for ToByteArr.
	/// </summary>
	/// <param name="Node">Target test node.</param>
	public void RegisterToByteArr(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithBlob),
			[typeof(ExtnTextWithBlob)],
			[nameof(ExtnTextWithBlob.ToByteArr)],
			"ToByteArr:"
		);
		var r = register.Register;

		r("writes big-endian header then utf8 text then blob", async _ => {
			const string text = "A中";
			var blob = new byte[] { 0x10, 0x20 };
			var packed = Tsinswreng.CsTextWithBlob.TextWithBlob.Pack(text, blob);
			var bytes = packed.ToByteArr();

			var textBytes = Utf8(text);
			var header = BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(0, Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen));
			AssertEqual((ulong)textBytes.Length, header, "ToByteArr/header");
			AssertBytesEqual(textBytes, bytes.AsMemory(Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen, textBytes.Length), "ToByteArr/text");
			AssertBytesEqual(
				blob,
				bytes.AsMemory(Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen + textBytes.Length, blob.Length),
				"ToByteArr/blob"
			);
			return null;
		});

		r("supports empty text and empty blob", async _ => {
			var packed = Tsinswreng.CsTextWithBlob.TextWithBlob.Pack(string.Empty, ReadOnlyMemory<byte>.Empty);
			var bytes = packed.ToByteArr();
			AssertEqual(Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen, bytes.Length, "ToByteArr/empty/length");
			var header = BinaryPrimitives.ReadUInt64BigEndian(bytes.AsSpan(0, Tsinswreng.CsTextWithBlob.TextWithBlob.HeaderLen));
			AssertEqual(0UL, header, "ToByteArr/empty/header");
			return null;
		});
	}
}
