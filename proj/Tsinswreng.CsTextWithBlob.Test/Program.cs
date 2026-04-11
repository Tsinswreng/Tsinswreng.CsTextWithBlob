using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsTreeTest;

namespace Tsinswreng.CsTextWithBlob.Test;

/// Test entrypoint for this csproj.
public static class Program {
	/// Build DI container and execute the test tree.
	/// <param name="Args">Command-line arguments (unused).</param>
	public static async Task Main(string[] Args) {
		_ = Args;
		IServiceCollection svcColct = new ServiceCollection();
		var mgr = TextWithBlobTestMgr.Inst;
		_ = mgr.InitSvc(svcColct, sc => sc.BuildServiceProvider());
		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}
