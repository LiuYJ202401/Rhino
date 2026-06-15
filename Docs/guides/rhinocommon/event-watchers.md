# 事件监听器

## 概述

事件监听器允许插件响应 Rhino 中发生的事件，如对象添加、修改、删除等。

## UI 线程注意

应用程序只能从主 UI 线程更新 UI 控件。从其他线程更新会导致崩溃。

## 安全更新 UI

```csharp
private void RhinoObjectAdded(object sender, RhinoObjectEventArgs e)
{
    var msg = $"对象已添加: {e.ObjectId}";

    // 检查是否在 UI 线程
    if (_label.Dispatcher.CheckAccess())
        _label.Content = msg;
    else
    {
        // 在 UI 线程上执行
        var setLabel = new Action<string>(txt => _label.Content = txt);
        _label.Dispatcher.Invoke(setLabel, new object[] { msg });
    }
}
```

## 常用事件

```csharp
// 在 PlugIn 类中订阅事件
protected override LoadReturnCode OnLoad(ref string errorMessage)
{
    RhinoDoc.ActiveDoc.Objects.AddRhinoObject += OnObjectAdded;
    RhinoDoc.ActiveDoc.Objects.Modified += OnObjectModified;
    RhinoDoc.ActiveDoc.Objects.Deleted += OnObjectDeleted;
    return LoadReturnCode.Success;
}

private void OnObjectAdded(object sender, RhinoObjectEventArgs e)
{
    RhinoApp.WriteLine($"添加对象: {e.ObjectId}");
}

private void OnObjectModified(object sender, RhinoObjectEventArgs e)
{
    RhinoApp.WriteLine($"修改对象: {e.ObjectId}");
}

private void OnObjectDeleted(object sender, RhinoObjectEventArgs e)
{
    RhinoApp.WriteLine($"删除对象: {e.ObjectId}");
}
```

## 取消订阅

```csharp
protected override void OnUnload()
{
    RhinoDoc.ActiveDoc.Objects.AddRhinoObject -= OnObjectAdded;
    RhinoDoc.ActiveDoc.Objects.Modified -= OnObjectModified;
    RhinoDoc.ActiveDoc.Objects.Deleted -= OnObjectDeleted;
    base.OnUnload();
}
```
