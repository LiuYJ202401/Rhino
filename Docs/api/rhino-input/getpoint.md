# GetPoint - 获取点输入

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户在视图中点击的点位置，支持动态绘制和约束。

## 常用方法

| 方法 | 说明 |
|------|------|
| `SetCommandPrompt(string)` | 设置命令提示 |
| `Get()` | 获取输入 (阻塞等待) |
| `SetBasePoint(Point3d, bool)` | 设置基点并启用橡皮线 |
| `SetDefaultPoint(Point3d)` | 设置默认点 |
| `AcceptNothing(bool)` | 是否允许按 Enter 接受默认值 |

## 动态绘制

| 事件 | 说明 |
|------|------|
| DynamicDraw | 动态绘制预览 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Point | Point3d | 获取到的点 (Get 返回后) |
| PointCount | int | 点击的点数 |

## 基本用法

```csharp
var getPoint = new GetPoint();
getPoint.SetCommandPrompt("选择点");

if (getPoint.Get() == GetResult.Point)
{
    Point3d pt = getPoint.Point();
    RhinoApp.WriteLine($"选中点: {pt}");
}
```

## 带基点的两点输入

```csharp
// 第一点
var getPt1 = new GetPoint();
getPt1.SetCommandPrompt("起点");
if (getPt1.Get() != GetResult.Point) return Result.Cancel;
Point3d pt0 = getPt1.Point();

// 第二点 (带动态线)
var getPt2 = new GetPoint();
getPt2.SetCommandPrompt("终点");
getPt2.SetBasePoint(pt0, true);  // 设置基点，启用橡皮线
getPt2.DynamicDraw += (sender, e) => {
    // 动态绘制从 pt0 到当前鼠标位置的线
    e.Display.DrawLine(pt0, e.CurrentPoint, Color.Black);
};
if (getPt2.Get() != GetResult.Point) return Result.Cancel;
Point3d pt1 = getPt2.Point();
```

## 带默认值

```csharp
var getPoint = new GetPoint();
getPoint.SetCommandPrompt("点位置 <原点>");
getPoint.SetDefaultPoint(Point3d.Origin);
getPoint.AcceptNothing(true);  // 允许按 Enter

GetResult result = getPoint.Get();
if (result == GetResult.Point)
{
    Point3d pt = getPoint.Point();
}
else if (result == GetResult.Nothing)
{
    Point3d pt = Point3d.Origin;  // 使用默认值
}
```

## GetResult 枚举

| 值 | 说明 |
|-----|------|
| Point | 成功获取点 |
| Nothing | 用户按 Enter (接受默认值) |
| Cancel | 用户按 ESC |
| NoPoint | 无点输入 |
