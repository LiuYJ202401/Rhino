# 插件生命周期

## PlugIn 类

每个 RhinoCommon 插件必须有且只有一个继承自 `Rhino.PlugIns.PlugIn` 的类。

## 基本结构

```csharp
using Rhino;
using Rhino.PlugIns;

namespace MyPlugin
{
    public class MyPlugin : PlugIn
    {
        public MyPlugin()
        {
            Instance = this;
        }

        public static MyPlugin Instance { get; private set; }

        protected override LoadReturnCode OnLoad(ref string errorMessage)
        {
            // 插件加载时调用
            // 返回 Success 或 Failure
            return LoadReturnCode.Success;
        }

        protected override void OnUnload()
        {
            // 插件卸载时调用
            base.OnUnload();
        }
    }
}
```

## 生命周期事件

| 事件 | 说明 |
|------|------|
| OnLoad | 插件加载时 |
| OnUnload | 插件卸载时 |
| OnStartupCompleted | Rhino 启动完成 |
| OnShutdown | Rhino 关闭前 |

## 命令注册

命令类自动被发现，无需手动注册。

```csharp
// 只要继承 Command，命令会自动注册
public class MyCommand : Command
{
    public override string EnglishName => "MyCommand";
    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        return Result.Success;
    }
}
```
