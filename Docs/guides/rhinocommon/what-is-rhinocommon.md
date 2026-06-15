# 什么是 RhinoCommon?

## 概述

RhinoCommon 是跨平台 .NET 插件 SDK，可用于：
- Rhino for Windows
- Rhino for Mac
- Rhino.Python 脚本
- Grasshopper

## 组件

| 程序集 | 说明 |
|--------|------|
| **RhinoCommon.dll** | 核心 .NET 程序集，插件引用此程序集与 Rhino 交互 |
| **Eto.dll** | 跨平台 UI 框架 |
| **Rhino.UI.dll** | Rhino 特定 UI 工具类 |

## 插件类型

RhinoCommon 支持五种插件类型：

| 类型 | 说明 |
|------|------|
| **General Utility** | 通用工具插件，包含一个或多个命令 |
| **File Import** | 文件导入插件 |
| **File Export** | 文件导出插件 |
| **Custom Rendering** | 自定义渲染 |
| **3D Digitizing** | 三维数字化设备接口 |

## 使用示例

```csharp
using Rhino;
using Rhino.Commands;

public class MyCommand : Command
{
    public override string EnglishName => "MyCommand";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        Point3d pt = new Point3d(10, 10, 10);
        doc.Objects.AddPoint(pt);
        return Result.Success;
    }
}
```
