# Command - 命令类

## 命名空间
`Rhino.Commands`

## 摘要
所有 Rhino 命令的基类。继承此类以创建自定义命令。

## 必须实现

| 成员 | 类型 | 说明 |
|------|------|------|
| EnglishName | string | 命令英文名称 (用于命令行调用) |
| RunCommand | Result | 命令执行逻辑 |

## 可选重写

| 方法 | 说明 |
|------|------|
| `LoadCommandLineHelp()` | 返回命令帮助字符串 |
| `LocalizedName()` | 返回本地化名称 |

## Result 枚举

命令执行结果：

| 值 | 说明 |
|-----|------|
| Success | 命令成功执行 |
| Cancel | 用户取消操作 |
| Failure | 命令执行失败 |
| Nothing | 无操作 |

## RunMode 枚举

| 值 | 说明 |
|-----|------|
| Interactive | 交互模式 - 可以使用 GUI 和命令行 |
| Scripted | 脚本模式 - 仅命令行，无 GUI |

## 命令结构模板

```csharp
using Rhino;
using Rhino.Commands;

namespace MyPlugin
{
    public class MyCommand : Command
    {
        // 必需: 命令英文名称
        public override string EnglishName => "MyCommand";

        // 必需: 命令执行逻辑
        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            // 1. 获取用户输入
            var getPoint = new Rhino.Input.Custom.GetPoint();
            getPoint.SetCommandPrompt("选择点");
            if (getPoint.Get() != GetResult.Point)
                return Result.Cancel;
            Point3d pt = getPoint.Point();

            // 2. 执行操作
            doc.Objects.AddPoint(pt);

            // 3. 返回结果
            return Result.Success;
        }
    }
}
```

## 命令命名规范

- 英文名使用 PascalCase
- 命令名应描述功能
- 避免与 Rhino 内置命令冲突

```csharp
// 好的命名
public override string EnglishName => "CreateStair";
public override string EnglishName => "DrawRailing";

// 不好的命名
public override string EnglishName => "cmd1";
public override string EnglishName => "Line";  // 与内置命令冲突
```

## 命令样式属性

```csharp
[CommandStyle(Style.ScriptRunner)]
public class MyCommand : Command
{
    // 允许命令运行其他 Rhino 命令
}
```
