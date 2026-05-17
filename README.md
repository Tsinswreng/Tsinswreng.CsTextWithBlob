## Tsinswreng.CsTextWithBlob

Tsinswreng.CsTextWithBlob 提供一種簡單的二進制打包格式：

- 8 字節大端序頭部
- UTF-8 文本部分
- 剩餘二進制負載

它同時提供基於 `ReadOnlyMemory<byte>` 和 `Stream` 的兩套 API。

### 安裝

```bash
dotnet add package Tsinswreng.CsTextWithBlob --version 0.0.1-alpha
```
