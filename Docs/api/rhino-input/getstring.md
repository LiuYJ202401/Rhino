# GetString - 获取字符串输入

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户输入的字符串。

## 常用方法

| 方法 | 说明 |
|------|------|
| `SetCommandPrompt(string)` | 设置提示 |
| `AcceptNothing(bool)` | 允许 Enter 接受空字符串 |
| `AcceptSpace(bool)` | 允许空格键结束输入 |

## 基本用法

```csharp
var getString = new GetString();
getString.SetCommandPrompt("输入名称");
getString.AcceptNothing(true);

GetResult result = getString.Get();
if (result == GetResult.String)
{
    string name = getString.StringResult();
}
```

## 选项输入

```csharp
var getString = new GetString();
getString.SetCommandPrompt("选择选项 [是(Y)/否(N)]");
getString.AcceptNothing(true);

// 也可以使用 GetOption 处理选项
```
