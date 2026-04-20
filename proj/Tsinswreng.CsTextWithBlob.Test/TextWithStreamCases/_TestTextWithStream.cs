using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithStreamCases;

/// <summary>
/// Main tester for <see cref="Tsinswreng.CsTextWithBlob.TextWithStream"/>.
/// Each partial file targets one function.
/// </summary>
public partial class TestTextWithStream : ITester {
	/// <summary>
	/// Register all function-level test groups.
	/// </summary>
	/// <param name="Node">Optional parent node.</param>
	/// <returns>Node containing all test cases.</returns>
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterPack(Node);
		RegisterToStream(Node);
		RegisterUnpack(Node);
		return Node;
	}

	/// <summary>
	/// Assert value equality with readable diagnostics.
	/// </summary>
	protected static void AssertEqual<T>(T Expected, T Actual, string CaseName) {
		if(!EqualityComparer<T>.Default.Equals(Expected, Actual)) {
			throw new Exception($"Case '{CaseName}' failed. Expected: {Expected}; Actual: {Actual}.");
		}
	}

	/// <summary>
	/// Read entire stream from current position.
	/// </summary>
	protected static byte[] ReadAllBytes(Stream Stream) {
		using var ms = new MemoryStream();
		Stream.CopyTo(ms);
		return ms.ToArray();
	}

	/// <summary>
	/// Build packet bytes: header + text + payload.
	/// </summary>
	protected static byte[] BuildPacket(string Text, byte[] Payload) {
		var textBytes = Encoding.UTF8.GetBytes(Text);
		var all = new byte[8 + textBytes.Length + Payload.Length];
		System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(all.AsSpan(0, 8), (ulong)textBytes.Length);
		textBytes.CopyTo(all.AsSpan(8));
		Payload.CopyTo(all.AsSpan(8 + textBytes.Length));
		return all;
	}
}
