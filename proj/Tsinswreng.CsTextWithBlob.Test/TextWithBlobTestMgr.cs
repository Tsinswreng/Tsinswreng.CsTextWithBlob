using Tsinswreng.CsTreeTest;
using Tsinswreng.CsTextWithBlob.Test.TextWithBlobCases;
using Tsinswreng.CsTextWithBlob.Test.TextWithStreamCases;

namespace Tsinswreng.CsTextWithBlob.Test;

/// <summary>
/// Collect and register all testers for this test project.
/// </summary>
public class TextWithBlobTestMgr : DiEtTestMgr {
	public static TextWithBlobTestMgr Inst = new();

	/// <summary>
	/// Register all tester classes into root test node.
	/// </summary>
	/// <param name="Test">Optional node parameter (root node is always used).</param>
	/// <returns>Root node after registration.</returns>
	public override ITestNode RegisterTestsInto(ITestNode? Test) {
		Test = this.TestNode;
		this.RegisterTester<TestTextWithBlob>();
		this.RegisterTester<TestTextWithStream>();
		return Test;
	}
}
