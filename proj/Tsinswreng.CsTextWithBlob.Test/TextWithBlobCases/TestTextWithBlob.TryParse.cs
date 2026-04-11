using System.Buffers;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Tests for <see cref="Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse(ref ReadOnlySequence{byte})"/>.
/// </summary>
public partial class TestTextWithBlob {
	/// <summary>
	/// Register all cases for TryParse.
	/// </summary>
	/// <param name="Node">Target test node.</param>
	public void RegisterTryParse(ITestNode Node) {
		var register = Node.MkTestFnRegister(
			typeof(TestTextWithBlob),
			[typeof(Tsinswreng.CsTextWithBlob.TextWithBlob)],
			[nameof(Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse)],
			"TryParse:"
		);
		var r = register.Register;

		r("parses one packet and consumes exact bytes", async _ => {
			var packet1 = BuildPacketBytes("abc", new byte[] { 0x41, 0x42 });
			var packet2 = BuildPacketBytes("z", ReadOnlyMemory<byte>.Empty);
			var merged = new byte[packet1.Length + packet2.Length];
			Array.Copy(packet1, 0, merged, 0, packet1.Length);
			Array.Copy(packet2, 0, merged, packet1.Length, packet2.Length);
			var buffer = new ReadOnlySequence<byte>(merged);

			var parsed = Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse(ref buffer);
			if(parsed is null) {
				throw new Exception("Expected a parsed packet but got null.");
			}

			AssertEqual("abc", parsed.Text, "TryParse/consume/text");
			AssertBytesEqual(new byte[] { 0x41, 0x42 }, parsed.Blob, "TryParse/consume/blob");
			AssertEqual((long)packet2.Length, buffer.Length, "TryParse/consume/remaining-len");

			var parsed2 = Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse(ref buffer);
			if(parsed2 is null) {
				throw new Exception("Expected second packet but got null.");
			}
			AssertEqual("z", parsed2.Text, "TryParse/consume/second-text");
			AssertEqual(0L, buffer.Length, "TryParse/consume/remaining-after-second");
			return null;
		});

		r("returns null and does not consume when header is incomplete", async _ => {
			var bytes = new byte[] { 0x01, 0x02, 0x03 };
			var buffer = new ReadOnlySequence<byte>(bytes);
			var before = buffer.Length;
			var parsed = Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse(ref buffer);
			if(parsed is not null) {
				throw new Exception("Expected null for incomplete header.");
			}
			AssertEqual(before, buffer.Length, "TryParse/incomplete-header/no-consume");
			return null;
		});

		r("returns null and does not consume when packet body is incomplete", async _ => {
			var full = BuildPacketBytes("abcd", ReadOnlyMemory<byte>.Empty);
			var truncated = new byte[full.Length - 1];
			Array.Copy(full, truncated, truncated.Length);
			var buffer = new ReadOnlySequence<byte>(truncated);
			var before = buffer.Length;
			var parsed = Tsinswreng.CsTextWithBlob.TextWithBlob.TryParse(ref buffer);
			if(parsed is not null) {
				throw new Exception("Expected null for incomplete packet body.");
			}
			AssertEqual(before, buffer.Length, "TryParse/incomplete-body/no-consume");
			return null;
		});
	}
}
