# 用户输入指南

## 输入类型概览

| 输入类型 | 类 | 说明 |
|---------|-----|------|
| 点 | GetPoint | 获取点位置 |
| 对象 | GetObject | 选择对象 |
| 数字 | GetNumber | 获取实数 |
| 整数 | GetInteger | 获取整数 |
| 字符串 | GetString | 获取文本 |
| 选项 | GetOption | 从列表选择 |

## 点输入

```csharp
var getPoint = new GetPoint();
getPoint.SetCommandPrompt("选择点");
getPoint.SetDefaultPoint(Point3d.Origin);

if (getPoint.Get() == GetResult.Point)
{
    Point3d pt = getPoint.Point();
}
```

## 两点输入 (带预览)

```csharp
// 第一点
var gp1 = new GetPoint();
gp1.SetCommandPrompt("起点");
if (gp1.Get() != GetResult.Point) return Result.Cancel;
Point3d pt0 = gp1.Point();

// 第二点 (带橡皮线)
var gp2 = new GetPoint();
gp2.SetCommandPrompt("终点");
gp2.SetBasePoint(pt0, true);
gp2.DynamicDraw += (s, e) => {
    e.Display.DrawLine(pt0, e.CurrentPoint, Color.Black);
};
if (gp2.Get() != GetResult.Point) return Result.Cancel;
Point3d pt1 = gp2.Point();
```

## 对象选择

```csharp
var getObject = new GetObject();
getObject.SetCommandPrompt("选择曲线");
getObject.GeometryFilter = ObjectType.Curve;

if (getObject.Get() == GetResult.Object)
{
    ObjRef objRef = getObject.Object(0);
    Curve curve = objRef.Curve();
}
```

## 数字输入

```csharp
var getNumber = new GetNumber();
getNumber.SetCommandPrompt("输入半径");
getNumber.SetDefaultNumber(10.0);
getNumber.SetLowerLimit(0.0, false);

if (getNumber.Get() == GetResult.Number)
{
    double value = getNumber.Number();
}
```

## 组合输入

```csharp
// 先选对象，再输入参数
var go = new GetObject();
go.SetCommandPrompt("选择曲线");
if (go.Get() != GetResult.Object) return Result.Cancel;
Curve curve = go.Object(0).Curve();

var gn = new GetNumber();
gn.SetCommandPrompt("输入复制数量");
gn.SetDefaultNumber(5);
gn.SetLowerLimit(1, true);
if (gn.Get() != GetResult.Number) return Result.Cancel;
int count = (int)gn.Number();
```
