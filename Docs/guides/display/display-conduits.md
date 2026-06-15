# Display Conduits - 显示导管

## 概述

Display Conduit 允许你自定义 Rhino 的显示管道，在视图中绘制自定义图形。常用于：
- 命令预览
- 临时几何显示
- 自定义覆盖层

## 基本结构

```csharp
class MyConduit : Rhino.Display.DisplayConduit
{
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
    {
        base.CalculateBoundingBox(e);
        // 包含你的几何体的包围盒
        e.IncludeBoundingBox(m_bbox);
    }

    protected override void PostDrawObjects(DrawEventArgs e)
    {
        base.PostDrawObjects(e);
        // 在对象之上绘制
        e.Display.DrawLine(m_lineStart, m_lineEnd, Color.Red);
    }
}
```

## 绘制通道 (按执行顺序)

| 通道 | 说明 | 深度测试 |
|------|------|----------|
| CalculateBoundingBox | 计算包围盒 | - |
| PreDrawObjects | 对象绘制前 | 启用 |
| PostDrawObjects | 对象绘制后 | 启用 |
| DrawForeground | 前景绘制 | 禁用 |
| DrawOverlay | 覆盖层 (交互时) | 禁用 |

## 启用/禁用

```csharp
var conduit = new MyConduit();
conduit.Enabled = true;  // 启用
conduit.Enabled = false; // 禁用
```

## 绘制方法

```csharp
protected override void PostDrawObjects(DrawEventArgs e)
{
    // 线
    e.Display.DrawLine(from, to, color, thickness);

    // 点
    e.Display.DrawPoint(point, color);

    // 向量/箭头
    e.Display.DrawArrow(line, color);

    // 曲线
    e.Display.DrawCurve(curve, color, thickness);

    // 圆
    e.Display.DrawCircle(circle, color, thickness);

    // 文字
    e.Display.DrawText(text, point, color);

    // Brep
    e.Display.DrawBrep(brep, color);
}
```

## 命令预览示例

```csharp
class LinePreviewConduit : DisplayConduit
{
    private Point3d m_start;
    private Point3d m_end;
    private bool m_active;

    public void Start(Point3d start)
    {
        m_start = start;
        m_end = start;
        m_active = true;
        Enabled = true;
    }

    public void Update(Point3d current)
    {
        m_end = current;
    }

    public void Stop()
    {
        m_active = false;
        Enabled = false;
    }

    protected override void DrawOverlay(DrawEventArgs e)
    {
        if (!m_active) return;
        e.Display.DrawLine(m_start, m_end, Color.Red, 2);
    }
}
```

## 注意事项

1. **包围盒**: 必须正确计算包围盒，否则图形可能被裁剪
2. **性能**: Conduit 每帧都会调用，避免重复计算
3. **清理**: 使用完毕后务必禁用
4. **冲突**: 注意与其他 Conduit 的交互
