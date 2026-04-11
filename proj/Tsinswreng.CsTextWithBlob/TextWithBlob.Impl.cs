namespace Tsinswreng.CsTextWithBlob;

using System;
using System.Buffers;
using System.Text;


public partial class TextWithMemory: ITextWithMemory{
	public u64 HeaderBytesLen{get;set;}
	public string Text { get;set;}
	public ReadOnlyMemory<byte> Blob { get;set;}
	public TextWithMemory(str Text, ReadOnlyMemory<byte> Blob){
		this.HeaderBytesLen = (u64)Encoding.UTF8.GetByteCount(Text);
		this.Text = Text;
		this.Blob = Blob;
	}
	public static partial TextWithMemory Pack(string Text, ReadOnlyMemory<byte> Blob){
		return new TextWithMemory(Text, Blob);
	}
	#region --- 解包 ---
	/// 从完整的数据块解析，成功返回实例，否则抛 ArgumentException。
	public static partial TextWithMemory Parse(ReadOnlyMemory<byte> Data){
		if (Data.Length < HeaderLen){
			throw new ArgumentException("数据长度不足 8 字节头部");
		}

		ulong textByteCountU = System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(Data.Span.Slice(0, HeaderLen));

		int textByteCount = checked((int)textByteCountU);
		int totalNeed = HeaderLen + textByteCount;
		if (Data.Length < totalNeed){
			throw new ArgumentException("数据长度不足，文本域尚未完整");
		}
		string text = Encoding.UTF8.GetString(Data.Span.Slice(HeaderLen, textByteCount));
		ReadOnlyMemory<byte> binary = Data.Slice(totalNeed);

		return new TextWithMemory(text, binary);
	}

	#endregion
}

public static partial class ExtnTextWithMemory {
	public const i32 HeaderLen = TextWithMemory.HeaderLen;

	#region --- 打包 ---
	
	public static partial byte[] ToByteArr<TSelf>(
		this TSelf z
	)where TSelf:ITextWithMemory{
		i32 textByteCount = (i32)z.HeaderBytesLen;//.ToInt();
		u64 total = HeaderLen + (u64)textByteCount + (u64)z.Blob.Length;
		byte[] arr = new byte[total];

		// 写头（固定小端）
		System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(arr.AsSpan(0, HeaderLen), (ulong)textByteCount);

		// 写 text
		Encoding.UTF8.GetBytes(z.Text, arr.AsSpan(HeaderLen, textByteCount));
		// 写 binary
		z.Blob.CopyTo(arr.AsMemory(HeaderLen + textByteCount));
		return arr;
	}

	/// 序列化到 IBufferWriter，适合与 System.IO.Pipelines 搭配。
	public static partial TSelf WriteTo<TSelf>(
		this TSelf z
		,IBufferWriter<byte> Writer
	)where TSelf:ITextWithMemory
	{
		//int textByteCount = Encoding.UTF8.GetByteCount(z.Text);
		i32 textByteCount = (i32)z.HeaderBytesLen;//.ToInt();
		Span<byte> span = Writer.GetSpan(HeaderLen + textByteCount + z.Blob.Length);

		// header
		System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(span, (ulong)textByteCount);
		span = span.Slice(HeaderLen);

		// text
		Encoding.UTF8.GetBytes(z.Text, span);
		span = span.Slice(textByteCount);

		// binary
		z.Blob.Span.CopyTo(span);

		Writer.Advance(HeaderLen + textByteCount + z.Blob.Length);
		return z;
	}
	#endregion


}


#if false

#endif
