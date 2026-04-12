using System.Text;
using Tsinswreng.CsCore;

namespace Tsinswreng.CsTextWithBlob;


[Doc(@$"
- {nameof(HeaderBytesLen)}: 8 bytes head (big endian, store the bytes count of {nameof(Text)})
- {nameof(Text)}
- blob
")]
public interface ITextWithStream{
	
	public u64 HeaderBytesLen{get;set;}
	public string Text { get;set;}
	public Stream Payload { get;set;}
}


public partial class TextWithStream{
	public u64 HeaderBytesLen{get;set;}
	public string Text {get;set;}
	public Stream Payload {get;set;}
	public partial TextWithStream();
	public static partial TextWithStream Pack(
		u64 HeaderBytesLen,
		string Text,
		Stream Payload
	);
	
	[Doc(@$"{nameof(ITextWithStream.HeaderBytesLen)} auto set to  bytes count of {nameof(Text)} in UTF8 encoding.")]
	public static partial TextWithStream PackUtf8(
		string Text,
		Stream Payload
	);
	public static partial TextWithStream Unpack(Stream stream);
	
}
