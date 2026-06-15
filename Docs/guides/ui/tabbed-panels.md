# 选项卡面板

## 概述

选项卡面板是 Rhino 中的可停靠面板，类似于图层、属性面板。插件可以创建自定义面板。

## 面板类型

| 类型 | 说明 | 实例生命周期 |
|------|------|-------------|
| **PerDocument** | 每个文档一个实例 | 文档创建时创建，关闭时销毁 |
| **System** | 全局单例 | Rhino 会话中只有一个实例 |

## 创建面板

### 基本注册

```csharp
using Rhino.UI;
using Rhino.UI.Panels;

public class MyPlugin : PlugIn
{
    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        // 注册面板
        Guid panelId = new Guid("{YOUR_PANEL_GUID}");
        Panels.RegisterPanel(this, typeof(MyPanel), "我的面板", panelId);
        
        return LoadReturnCode.Success;
    }
}
```

### 面板类构造函数

Rhino 按以下顺序查找构造函数：

```csharp
public class MyPanel : UserControl
{
    // 1. 优先：接受 RhinoDoc
    public MyPanel(RhinoDoc doc) { }

    // 2. 次选：接受文档序列号
    public MyPanel(uint docRuntimeSerialNumber) { }

    // 3. 最后：无参数构造函数
    public MyPanel() { }
}
```

## 实现 IPanel 接口

```csharp
using Rhino.UI;
using System.Windows.Forms;
using System.Drawing;

public class MyPanel : UserControl, IPanel
{
    private Label m_label;

    public MyPanel()
    {
        m_label = new Label();
        m_label.Text = "我的面板";
        m_label.Dock = DockStyle.Fill;
        Controls.Add(m_label);
    }

    // 面板显示时调用
    public void OnPanelShown(DockStyle dockStyle)
    {
        RhinoApp.WriteLine($"面板已显示: {dockStyle}");
        // 订阅文档事件
        RhinoDoc.ActiveDoc.Objects.Added += OnObjectAdded;
    }

    // 面板隐藏时调用
    public void OnPanelHidden(DockStyle dockStyle)
    {
        RhinoApp.WriteLine($"面板已隐藏");
        // 取消订阅
        RhinoDoc.ActiveDoc.Objects.Added -= OnObjectAdded;
    }

    private void OnObjectAdded(object sender, RhinoObjectEventArgs e)
    {
        m_label.Text = $"对象数量: {RhinoDoc.ActiveDoc.Objects.Count}";
    }
}
```

## Eto 面板 (跨平台)

Eto 是跨平台 UI 框架，同时支持 Windows 和 Mac：

```csharp
using Eto.Forms;
using Rhino.UI;

public class MyEtoPanel : Panel, IPanel
{
    private Label m_label;

    public MyEtoPanel()
    {
        m_label = new Label { Text = "我的面板" };
        Content = m_label;
    }

    public void OnPanelShown(DockStyle dockStyle)
    {
        RhinoApp.WriteLine("Eto 面板已显示");
    }

    public void OnPanelHidden(DockStyle dockStyle)
    {
        RhinoApp.WriteLine("Eto 面板已隐藏");
    }
}
```

## 面板生命周期

### PerDocument 面板

```
文档创建 → 构造面板 → OnPanelShown
└─ 文档操作 → 面板更新
└─ 文档关闭 → OnPanelHidden → Dispose
```

### System 面板

```
Rhino 启动 → 构造面板 → OnPanelShown (首次可见时)
└─ 文档切换 → 面板保持可见
└─ 面板关闭 → OnPanelHidden
└─ 面板重新打开 → OnPanelShown (复用实例)
```

## 面板同步

当面板需要与文档状态同步时：

```csharp
public class MyPanel : UserControl, IPanel
{
    private RhinoDoc m_doc;

    public MyPanel(RhinoDoc doc)
    {
        m_doc = doc;
        
        // 订阅文档事件
        m_doc.Objects.Modified += OnObjectsModified;
    }

    private void OnObjectsModified(object sender, RhinoObjectEventArgs e)
    {
        // 更新面板 UI
        UpdateContent();
    }

    private void UpdateContent()
    {
        // 在 UI 线程上更新
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateContent));
            return;
        }

        // 更新控件...
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && m_doc != null)
        {
            m_doc.Objects.Modified -= OnObjectsModified;
        }
        base.Dispose(disposing);
    }
}
```

## 注册选项

```csharp
// 完整注册示例
Guid panelId = new Guid("{YOUR-GUID-HERE}");
var panelType = PanelType.PerDocument; // 或 PanelType.System

Panels.RegisterPanel(
    this,           // 插件实例
    typeof(MyPanel), // 面板类型
    "我的面板",       // 面板标题
    panelId,        // 面板 ID
    panelType       // 面板类型
);
```

## 面板状态保存

使用 RhinoPlugIn.GetSettings() 保存面板状态：

```csharp
public class MyPanel : UserControl, IPanel
{
    private void SaveSettings()
    {
        var settings = RhinoPlugIn.GetSettings();
        settings.SetInteger("MyPanel_Width", Width);
        settings.SetInteger("MyPanel_Height", Height);
    }

    private void LoadSettings()
    {
        var settings = RhinoPlugIn.GetSettings();
        int width = settings.GetInteger("MyPanel_Width", 300);
        int height = settings.GetInteger("MyPanel_Height", 400);
        Size = new Size(width, height);
    }

    public void OnPanelHidden(DockStyle dockStyle)
    {
        SaveSettings();
    }

    public void OnPanelShown(DockStyle dockStyle)
    {
        LoadSettings();
    }
}
```

## 注意事项

1. **线程安全**: 面板 UI 操作必须在 UI 线程执行
2. **事件订阅**: 务必在 Dispose 时取消订阅，避免内存泄漏
3. **PerDocument vs System**: 
   - 显示文档特定内容用 PerDocument
   - 显示全局内容用 System
4. **跨平台**: 使用 Eto 确保跨平台兼容性
