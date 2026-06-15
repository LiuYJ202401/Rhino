# GetInteger - 获取整数输入

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户输入的整数。

## 常用方法

| 方法 | 说明 |
|------|------|
| `SetCommandPrompt(string)` | 设置提示 |
| `SetDefaultInteger(int)` | 设置默认值 |
| `SetLowerLimit(int, bool)` | 设置下限 |
| `SetUpperLimit(int, bool)` | 设置上限 |
| `AcceptNothing(bool)` | 允许 Enter 接受默认值 |

## 基本用法

```csharp
var getInteger = new GetInteger();
getInteger.SetCommandPrompt("输入数量 <5>");
getInteger.SetDefaultInteger(5);
getInteger.SetLowerLimit(1, true);  // >= 1

if (getInteger.Get() == GetResult.Number)
{
    int count = getInteger.Number();
}
else if (getInteger.Get() == GetResult.Nothing)
{
    int count = 5; // 使用默认值
}
```
