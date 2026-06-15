# 代码驱动文件读写

## 概述

Rhino 8 引入了通过代码直接读写文件的功能，无需用户交互。这支持自动化和批处理场景。

## 核心概念

导入/导出插件接受一个可选的字典参数，用于指定选项而不是弹出对话框。

```csharp
// 1. 创建选项对象
var options = new FileDwgWriteOptions();
options.Version = FileDwgWriteOptions.AutocadVersion.Acad2000;

// 2. 转换为字典
var optionsDictionary = options.ToDictionary();

// 3. 传递给导入/导出方法
doc.Export(path, optionsDictionary);
```

## 命名空间

`Rhino.FileIO` - 包含各种文件格式的选项类

| 选项类 | 文件格式 |
|--------|----------|
| FileDwgWriteOptions | DWG 导出 |
| FileDwgReadOptions | DWG 导入 |
| FileStpWriteOptions | STEP 导出 |
| FileStpReadOptions | STEP 导入 |
| FileIgsWriteOptions | IGES 导出 |
| FileIgsReadOptions | IGES 导入 |
| File3mfWriteOptions | 3MF 导出 |
| File3mfReadOptions | 3MF 导入 |
| FileSkpReadOptions | SketchUp 导入 |
| FileObjReadOptions | OBJ 导入 |
| FileObjWriteOptions | OBJ 导出 |

## 文件导出

### 基本导出

```csharp
var doc = RhinoDoc.ActiveDoc;

// 创建选项
var options = new FileDwgWriteOptions();
options.UseLWPolylines = true;
options.Version = FileDwgWriteOptions.AutocadVersion.Acad2018;

// 导出
string path = @"C:\output\file.dwg";
bool success = doc.Export(path, options.ToDictionary());
```

### 常用导出格式

```csharp
// STEP 导出
var stepOptions = new FileStpWriteOptions();
stepOptions.ExportPlaneMode = FileStpWriteOptions.PlaneMode.RhinoPlanarCurves;
doc.Export(path, stepOptions.ToDictionary());

// 3MF 导出
var mf3Options = new File3mfWriteOptions();
mf3Options.Title = "My Model";
mf3Options.Designer = "Your Name";
doc.Export(path, mf3Options.ToDictionary());

// OBJ 导出
var objOptions = new FileObjWriteOptions();
objOptions.ExportCurves = true;
objOptions.ExportPoints = true;
doc.Export(path, objOptions.ToDictionary());
```

## 文件导入

### 基本导入

```csharp
var doc = RhinoDoc.ActiveDoc;

// 创建选项
var options = new FileSkpReadOptions();
options.ImportCurves = false;
options.EmbedTexturesInModel = false;

// 导入
string path = @"C:\input\file.skp";
bool success = doc.Import(path, options.ToDictionary());
```

## Headless 文档

Headless 文档是不显示在 UI 中的 RhinoDoc 实例，用于后台处理。

### 创建和使用

```csharp
// 创建 headless 文档
var headlessDoc = RhinoDoc.CreateHeadless(null);

try
{
    // 导入文件
    var options = new FileDwgReadOptions();
    headlessDoc.Import(filePath, options.ToDictionary());

    // 处理几何
    foreach (var obj in headlessDoc.Objects)
    {
        var bbox = obj.Geometry.GetBoundingBox(true);
        RhinoApp.WriteLine($"对象中心: {bbox.Center}");
    }

    // 导出
    var exportOptions = new FileStpWriteOptions();
    headlessDoc.Export(outputPath, exportOptions.ToDictionary());
}
finally
{
    // 释放内存
    headlessDoc.Dispose();
}
```

### 批处理示例

```csharp
// 批量转换文件
string inputDir = @"C:\input";
string outputDir = @"C:\output";

foreach (var file in Directory.GetFiles(inputDir, "*.skp"))
{
    using (var doc = RhinoDoc.CreateHeadless(null))
    {
        // 导入
        var importOptions = new FileSkpReadOptions();
        doc.Import(file, importOptions.ToDictionary());

        // 导出为 3MF
        string outputFile = Path.Combine(outputDir,
            Path.GetFileNameWithoutExtension(file) + ".3mf");
        var exportOptions = new File3mfWriteOptions();
        doc.Export(outputFile, exportOptions.ToDictionary());
    }
}
```

## Grasshopper 中使用

在 Grasshopper 中，使用 headless 文档处理文件：

```python
import Rhino
import System

# 创建 headless 文档
doc = Rhino.RhinoDoc.CreateHeadless(None)

# 添加几何体到文档
if not objects is None:
    for o in objects:
        doc.Objects.Add(o)

# 导出
options = Rhino.FileIO.File3mfWriteOptions()
options.Title = title
options.Designer = "Name"

if not path is None:
    success = doc.Export(path, options.ToDictionary())

# 释放
doc.Dispose()
```

## 常见选项类属性

### FileDwgWriteOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| Version | AutocadVersion | AutoCAD 版本 (R12, R14, 2000, ...) |
| UseLWPolylines | bool | 使用轻量级多段线 |
| ExportPaperSpace | bool | 导出图纸空间 |
| ZoomExtents | bool | 缩放到范围 |

### FileStpWriteOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| ExportPlaneMode | PlaneMode | 平面曲线导出模式 |
| UnitSystem | UnitSystem | 单位系统 |

### File3mfWriteOptions

| 属性 | 类型 | 说明 |
|------|------|------|
| Title | string | 模型标题 |
| Designer | string | 设计者名称 |
| Metadata | Dictionary<string,string> | 自定义元数据 |
| Compression | CompressionLevel | 压缩级别 |
