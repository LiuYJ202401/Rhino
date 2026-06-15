# 从插件运行 Rhino 命令

## 概述

Rhino 不允许插件命令直接运行其他命令，除非使用特殊的脚本命令模式。

## 解决方案

定义命令时添加 `ScriptRunner` 命令样式：

```csharp
[CommandStyle(Style.ScriptRunner)]
public class ScriptCommand : Command
{
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        // 现在可以运行其他 Rhino 命令
        Rhino.RhinoApp.RunScript("_-Line 0,0,0 10,10,10", false);
        return Result.Success;
    }
}
```

## 注意事项

1. **命令前缀**: 使用 `_-` 前缀让命令不显示对话框
2. **异步问题**: RunScript 可能在命令完成前返回
3. **脚本模式**: 只有标记为 ScriptRunner 的命令才能运行脚本

## 完整示例

```csharp
[CommandStyle(Style.ScriptRunner)]
public class ImportAndProcess : Command
{
    public override string EnglishName => "ImportAndProcess";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        // 运行导入命令
        string script = "_-Import \"C:\\\\file.3ds\" Enter";
        RhinoApp.RunScript(script, false);

        // 运行其他命令处理导入的对象
        RhinoApp.RunScript("_-SelAll", false);
        RhinoApp.RunScript("_-Mesh", false);

        return Result.Success;
    }
}
```
