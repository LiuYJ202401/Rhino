# DisplayConduit - 显示导管

## 命名空间
`Rhino.Display`

## 摘要
允许在 Rhino 显示管道中绘制自定义图形。

## 常用重写方法

| 方法 | 说明 | 时机 |
|------|------|------|
| CalculateBoundingBox | 计算包围盒 | 每帧开始 |
| PreDrawObjects | 对象绘制前 | 深度测试启用 |
| PostDrawObjects | 对象绘制后 | 深度测试启用 |
| DrawForeground | 前景绘制 | 深度测试禁用 |
| DrawOverlay | 覆盖层 | 交互时绘制 |

## 基本结构

```csharp
class MyConduit : DisplayConduit
{
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
    {
        base.CalculateBoundingBox(e);
        // 包含自定义几何的包围盒
        e.IncludeBoundingBox(m_bbox);
    }

    protected override void PostDrawObjects(DrawEventArgs e)
    {
        base.PostDrawObjects(e);
        // 绘制内容
        e.Display.DrawLine(m_from, m_to, Color.Red);
    }
}
```

## 启用/禁用

```csharp
var conduit = new MyConduit();
conduit.Enabled = true;  // 启用
conduit.Enabled = false; // 禁用
```

## DrawEventArgs 方法

```csharp
e.Display.DrawLine(from, to, color, thickness);
e.Display.DrawPoint(point, color);
e.Display.DrawCircle(circle, color, thickness);
e.Display.DrawCurve(curve, color, thickness);
e.Display.DrawBrep(brep, color);
e.Display.DrawArrow(line, color);
e.Display.DrawText(text, point, color);
```
