namespace Tsinswreng.CsTextWithBlob;
using System;
using System.Buffers;
using System.Text;

/// 格式：| UInt64頭(大端, 定義文本部分ʹ長度) | UTF-8 文本 | 二进制负载 |
public interface ITextWithBlob{
	public u64 HeaderBytesLen{get;set;}
	public string Text { get;set;}
	public ReadOnlyMemory<byte> Blob { get;set;}
}


public partial class TextWithBlob{
	public const i32 HeaderLen = 8;
	
	public static partial TextWithBlob Pack(string Text, ReadOnlyMemory<byte> Blob);
	public static partial TextWithBlob Parse(ReadOnlyMemory<byte> Data);
}


public static partial class ExtnTextWithBlob{
	public static partial byte[] ToByteArr<TSelf>(
		this TSelf z
	)where TSelf:ITextWithBlob;
	
	/// 序列化到 IBufferWriter，适合与 System.IO.Pipelines 搭配。
	public static partial TSelf WriteTo<TSelf>(
		this TSelf z
		,IBufferWriter<byte> Writer
	)where TSelf:ITextWithBlob;
}
