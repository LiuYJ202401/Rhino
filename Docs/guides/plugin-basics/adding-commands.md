# 添加命令到项目

## 概述

RhinoCommon 插件可以包含多个命令。每个命令都是继承自 `Rhino.Commands.Command` 的独立类。

## 创建命令步骤

### 方法 1: 使用 Visual Studio 模板

1. 打开项目
2. 右键项目 > 添加 > 新建项
3. 选择 "Empty RhinoCommon Command" 模板
4. 输入文件名，点击添加

### 方法 2: 手动创建类

1. 创建新类文件
2. 继承 `Command`
3. 实现 `EnglishName` 和 `RunCommand`

## 最简单的命令示例

```csharp
using Rhino;
using Rhino.Commands;

namespace MyPlugin
{
    public class HelloCommand : Command
    {
        public override string EnglishName => "Hello";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            RhinoApp.WriteLine("Hello Rhino!");
            return Result.Success;
        }
    }
}
```

## 命令的结构

```csharp
public class MyCommand : Command
{
    // 1. 命令名称 (必需)
    public override string EnglishName => "MyCommandName";

    // 2. 可选: 本地化名称
    public override string LocalizedName => "我的命令";

    // 3. 命令执行 (必需)
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        // 命令逻辑
        return Result.Success;
    }
}
```

## 典型命令流程

```csharp
protected override Result RunCommand(RhinoDoc doc, RunMode mode)
{
    // 1. 获取输入
    var getPoint = new GetPoint();
    getPoint.SetCommandPrompt("选择点");
    if (getPoint.Get() != GetResult.Point)
        return Result.Cancel;
    Point3d pt = getPoint.Point();

    // 2. 执行操作
    Guid id = doc.Objects.AddPoint(pt);

    // 3. 更新视图
    doc.Views.Redraw();

    return Result.Success;
}
```

## 命令组织建议

- 一个文件包含一个相关命令组
- 按功能分类命令
- 使用清晰的命名

```csharp
// GeometryCommands.cs
public class CreatePointCommand : Command { }
public class CreateLineCommand : Command { }
public class CreateCircleCommand : Command { }

// TransformCommands.cs
public class MoveObjectsCommand : Command { }
public class RotateObjectsCommand : Command { }
public class ScaleObjectsCommand : Command { }
```

## 返回值指南

| 返回值 | 使用场景 |
|--------|----------|
| Success | 命令成功完成 |
| Cancel | 用户取消 (ESC)、无效输入 |
| Failure | 命令执行出错 |
| Nothing | 无操作但非错误 |
