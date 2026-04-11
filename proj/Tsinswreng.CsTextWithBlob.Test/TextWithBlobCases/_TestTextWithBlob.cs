using System.Text;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;

/// <summary>
/// Main tester of <see cref="Tsinswreng.CsTextWithBlob.TextWithBlob"/> and <see cref="ExtnTextWithBlob"/>.
/// Each partial file registers tests for one target function.
/// </summary>
public partial class TestTextWithBlob : ITester {
	/// <summary>
	/// Register all function-level test groups.
	/// </summary>
	/// <param name="Node">Optional parent node.</param>
	/// <returns>Node that contains all test cases in this tester.</returns>
	public ITestNode RegisterTestsInto(ITestNode? Node) {
		Node ??= new TestNode();
		Node.Ordered = false;
		Node.IsParallelRecursive = false;
		RegisterPack(Node);
		RegisterParse(Node);
		RegisterTryParse(Node);
		RegisterToByteArr(Node);
		RegisterWriteTo(Node);
		return Node;
	}

	/// <summary>
	/// Assert value equality with readable failure message.
	/// </summary>
	/// <typeparam name="T">Compared value type.</typeparam>
	/// <param name="Expected">Expected value.</param>
	/// <param name="Actual">Actual value.</param>
	/// <param name="CaseName">Case label for diagnostics.</param>
	protected static void AssertEqual<T>(T Expected, T Actual, string CaseName) {
		if(!EqualityComparer<T>.Default.Equals(Expected, Actual)) {
			throw new Exception($"Case '{CaseName}' failed. Expected: {Expected}; Actual: {Actual}.");
		}
	}

	/// <summary>
	/// Assert two byte sequences are exactly identical.
	/// </summary>
	/// <param name="Expected">Expected bytes.</param>
	/// <param name="Actual">Actual bytes.</param>
	/// <param name="CaseName">Case label for diagnostics.</param>
	protected static void AssertBytesEqual(ReadOnlyMemory<byte> Expected, ReadOnlyMemory<byte> Actual, string CaseName) {
		if(!Expected.Span.SequenceEqual(Actual.Span)) {
			throw new Exception(
				$"Case '{CaseName}' failed. Expected bytes: [{BitConverter.ToString(Expected.ToArray())}], " +
				$"Actual bytes: [{BitConverter.ToString(Actual.ToArray())}]."
			);
		}
	}

	/// <summary>
	/// Build a packet's serialized bytes via pack + serialize pipeline.
	/// </summary>
	/// <param name="Text">Text part.</param>
	/// <param name="Blob">Binary payload.</param>
	/// <returns>Serialized full packet bytes.</returns>
	protected static byte[] BuildPacketBytes(string Text, ReadOnlyMemory<byte> Blob) {
		return Tsinswreng.CsTextWithBlob.TextWithBlob.Pack(Text, Blob).ToByteArr();
	}

	/// <summary>
	/// UTF-8 bytes helper used across test cases.
	/// </summary>
	/// <param name="Text">Input text.</param>
	/// <returns>UTF-8 bytes.</returns>
	protected static byte[] Utf8(string Text) {
		return Encoding.UTF8.GetBytes(Text);
	}
}
