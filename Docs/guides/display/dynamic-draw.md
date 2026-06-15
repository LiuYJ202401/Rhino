# 动态绘制

## 概述

动态绘制 (Dynamic Draw) 允许在用户交互时实时显示预览。

## GetPoint 动态绘制

```csharp
var getPoint = new GetPoint();
getPoint.DynamicDraw += (sender, e) => {
    // e.CurrentPoint 是当前鼠标位置
    Point3d current = e.CurrentPoint;
    
    // 绘制预览内容
    e.Display.DrawLine(m_startPoint, current, Color.Red, 2);
    e.Display.DrawCircle(new Circle(current, 10.0), Color.Blue);
};
getPoint.Get();
```

## 常见模式

### 橡皮线

```csharp
var getPoint = new GetPoint();
getPoint.SetBasePoint(m_fixedPoint, true); // 启用橡皮线
getPoint.DynamicDraw += (s, e) => {
    // 橡皮线自动绘制，可添加其他预览
    e.Display.DrawPoint(e.CurrentPoint, Color.Red);
};
```

### 预览几何体

```csharp
getPoint.DynamicDraw += (s, e) => {
    Point3d pt = e.CurrentPoint;
    
    // 预览圆
    double radius = m_center.DistanceTo(pt);
    Circle preview = new Circle(m_center, radius);
    e.Display.DrawCircle(preview, Color.Black, 1);
    
    // 显示半径文字
    string text = $"R = {radius:F2}";
    e.Display.Draw2dText(text, Color.Black, pt, false, 12);
};
```

### 预览变换

```csharp
getPoint.DynamicDraw += (s, e) => {
    Vector3d vec = e.CurrentPoint - m_origin;
    Transform xform = Transform.Translation(vec);
    
    // 预览变换后的物体
    foreach (var geom in m_geometry)
    {
        Geometry duplicated = geom.DuplicateGeometry();
        duplicated.Transform(xform);
        
        if (duplicated is Curve curve)
            e.Display.DrawCurve(curve, Color.Blue);
        else if (duplicated is Brep brep)
            e.Display.DrawBrep(brep, Color.Blue);
    }
};
```

## Display 绘制方法

| 方法 | 说明 |
|------|------|
| DrawLine | 直线 |
| DrawPoint | 点 |
| DrawCircle | 圆 |
| DrawCurve | 曲线 |
| DrawBrep | Brep |
| DrawArrow | 箭头 |
| DrawText / Draw2dText | 文字 |
| DrawMesh | 网格 |
| DrawDot | 点标记 |
