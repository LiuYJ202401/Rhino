# GetNumber - 获取数字输入

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户输入的实数。

## 常用方法

| 方法 | 说明 |
|------|------|
| `SetCommandPrompt(string)` | 设置提示 |
| `SetDefaultNumber(double)` | 设置默认值 |
| `SetLowerLimit(double, bool)` | 设置下限 |
| `SetUpperLimit(double, bool)` | 设置上限 |
| `AcceptNothing(bool)` | 允许 Enter 接受默认值 |

## 基本用法

```csharp
var getNumber = new GetNumber();
getNumber.SetCommandPrompt("输入半径 <10.0>");
getNumber.SetDefaultNumber(10.0);
getNumber.SetLowerLimit(0.0, false); // 大于 0

if (getNumber.Get() == GetResult.Number)
{
    double radius = getNumber.Number();
}
else if (getNumber.Get() == GetResult.Nothing)
{
    double radius = 10.0; // 使用默认值
}
```

## 上下限

```csharp
var getNumber = new GetNumber();
getNumber.SetCommandPrompt("输入角度 (0-360)");
getNumber.SetLowerLimit(0.0, true);  // >= 0
getNumber.SetUpperLimit(360.0, true); // <= 360

if (getNumber.Get() == GetResult.Number)
{
    double angle = getNumber.Number();
}
```
