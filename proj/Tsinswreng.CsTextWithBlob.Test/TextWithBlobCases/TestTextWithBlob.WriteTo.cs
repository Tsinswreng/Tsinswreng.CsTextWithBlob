using System.Buffers;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Tests for <see cref="ExtnTextWithMemory.WriteTo{TSelf}(TSelf, IBufferWriter{byte})"/>.
/// </summary>
public partial class TestTextWithBlob {
	/// <summary>
	/// Register all cases for WriteTo.
	/// </summary>
	/// <param name="Node">Target test node.</param>
	public void RegisterWriteTo(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithBlob),
			[typeof(ExtnTextWithMemory)],
			[nameof(ExtnTextWithMemory.WriteTo)],
			"WriteTo:"
		);
		var r = register.Register;

		r("writes identical bytes as ToByteArr and returns same instance", async _ => {
			var packed = Tsinswreng.CsTextWithBlob.TextWithMemory.Pack("中", new byte[] { 0x07, 0x08 });
			var writer = new ArrayBufferWriter<byte>();
			var returned = packed.WriteTo(writer);
			if(!ReferenceEquals(packed, returned)) {
				throw new Exception("WriteTo should return the same packet instance for fluent usage.");
			}
			AssertBytesEqual(packed.ToByteArr(), writer.WrittenMemory, "WriteTo/same-bytes");
			return null;
		});

		r("appends to an existing writer position", async _ => {
			var packed = Tsinswreng.CsTextWithBlob.TextWithMemory.Pack("ok", new byte[] { 0x01 });
			var writer = new ArrayBufferWriter<byte>();
			var prefix = writer.GetSpan(1);
			prefix[0] = 0xFF;
			writer.Advance(1);
			packed.WriteTo(writer);

			var expectedTail = packed.ToByteArr();
			var written = writer.WrittenMemory;
			AssertEqual(1 + expectedTail.Length, written.Length, "WriteTo/append/length");
			AssertEqual((byte)0xFF, written.Span[0], "WriteTo/append/prefix");
			AssertBytesEqual(expectedTail, written.Slice(1), "WriteTo/append/tail");
			return null;
		});
	}
}
