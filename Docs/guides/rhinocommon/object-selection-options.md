# 带选项的对象选择

## 概述

创建支持命令选项的对象选择器，允许用户在选对象和设置选项之间切换。

## 关键属性

| 属性 | 说明 |
|------|------|
| `EnableClearObjectsOnEntry(bool)` | 进入时清除选择 |
| `EnableUnselectObjectsOnExit(bool)` | 退出时允许取消选择 |
| `DeselectAllBeforePostSelect` | 后选前取消所有选择 |
| `EnablePreSelect(bool, bool)` | 启用预选择 |

## 基本模式

```csharp
protected override Result RunCommand(RhinoDoc doc, RunMode mode)
{
    int option1 = 300;
    int option2 = 300;

    var opt1 = new OptionInteger(option1, 200, 900);
    var opt2 = new OptionInteger(option2, 200, 900);

    var go = new GetObject();
    go.SetCommandPrompt("选择曲面");
    go.GeometryFilter = ObjectType.Surface | ObjectType.PolysrfFilter | ObjectType.Mesh;
    go.AddOptionInteger("Option1", ref opt1);
    go.AddOptionInteger("Option2", ref opt2);
    go.GroupSelect = true;
    go.SubObjectSelect = false;
    go.EnableClearObjectsOnEntry(false);
    go.EnableUnselectObjectsOnExit(false);
    go.DeselectAllBeforePostSelect = false;

    bool havePreselected = false;

    while (true)
    {
        GetResult res = go.GetMultiple(1, 0);

        if (res == GetResult.Option)
        {
            go.EnablePreSelect(false, true);
            continue;
        }
        else if (res != GetResult.Object)
            return Result.Cancel;

        if (go.ObjectsWerePreselected)
        {
            havePreselected = true;
            go.EnablePreSelect(false, true);
            continue;
        }

        break;
    }

    // 处理选中的对象
    for (int i = 0; i < go.ObjectCount; i++)
    {
        ObjRef objRef = go.Object(i);
    }

    RhinoApp.WriteLine($"Option1 = {opt1.CurrentValue}");
    RhinoApp.WriteLine($"Option2 = {opt2.CurrentValue}");

    return Result.Success;
}
```
