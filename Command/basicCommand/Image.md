# Image 命令接口文档

命名空间：`Rh.Cmd` | 类名：`ImageCmd`

## 目的

图片到几何对象的转换命令。封装图片加载、灰度采样、几何生成的完整流程，供 Project 层调用。

## 默认值路径

`Command/basicCommand/Image.json`

---

## CreateCircleFit

对应 Rhino 命令：无（自定义功能，半调/Halftone 圆拟合）

### 自动计算逻辑

用户只需提供图片路径、放置平面和精细度，其余参数自动推导：

```
步骤 1：读取图片尺寸
  imgW = bmp.Width, imgH = bmp.Height

步骤 2：推导采样网格（保持图片宽高比，precision = 最短边圆数量）
  if imgW >= imgH:
      samplesY = precision
      samplesX = round(precision × imgW / imgH)
  else:
      samplesX = precision
      samplesY = round(precision × imgH / imgW)

步骤 3：物理尺寸（像素 × unitSize，保持图片比例）
  physWidth  = imgW × unitSize     // 如 100 像素 × 10 = 1000 单位
  physHeight = imgH × unitSize

步骤 4：调用 Geometry 层
  grayMatrix = ImageGeo.SampleGrayscale(bmp, samplesX, samplesY)
  circles    = ImageGeo.CreateCirclesFromGrayscale(grayMatrix, plane, physWidth, physHeight, minRadiusRatio)
```

### 重载 1：用户指定精细度

| 项目 | 说明 |
|------|------|
| 功能 | 根据图片灰度值生成大小不同的圆阵列，拟合原图 |
| 输入 | `string imagePath` — 图片文件路径 |
|        | `Plane plane` — 放置平面（圆心分布在平面上） |
|        | `int precision` — 精细度，图片最短边的圆数量（如 30 = 最短边 30 个圆） |
|        | `bool isPreview = false` — 预览模式（不更新 Data） |
| 输出 | `Circle[]` — 圆阵列，近白区域已跳过 |
| 报错 | 图片路径不存在 → `RhinoApp.WriteLine` + 返回 null |
|       | 图片加载失败 → `RhinoApp.WriteLine` + 返回 null |
|       | precision ≤ 0 → `RhinoApp.WriteLine` + 返回 null |
|       | Geometry 层返回 null → `RhinoApp.WriteLine` + 返回 null |
| RhinoCommon | 无直接 API，调用 `Rh.Geo.Img.ImageGeo` |

### GetDefaultPrecision

| 项目 | 说明 |
|------|------|
| 功能 | 查询精细度默认值，供 Project 层初始化 UI 选项 |
| 输入 | 无 |
| 输出 | `int` — 默认精细度（从 Data 层读取，默认 30） |
| RhinoCommon | 无，读取 `Image.json` |

---

## 方法调用关系

```
ImageCmd.CreateCircleFit(imagePath, plane, precision, isPreview)
  │
  ├─ 读取 Data 默认值（unitSize, minRadiusRatio）
  │
  ├─ new Bitmap(imagePath)                           ← 图片 I/O
  │
  ├─ ImageGeo.SampleGrayscale(bmp, samplesX, samplesY)  ← Geometry 层
  │     返回 double[,] 灰度矩阵
  │
  ├─ ImageGeo.CreateCirclesFromGrayscale(...)            ← Geometry 层
  │     返回 Circle[]
  │
  ├─ bmp.Dispose()                                   ← 释放资源
  │
  └─ if (!isPreview) UpdateDefault("precision", precision)
```

## 约束

- Command 层负责图片文件 I/O（`new Bitmap(path)`），Geometry 层只接收已加载的 `Bitmap`
- `precision` 含义是"最短边方向的圆数量"，采样网格按图片宽高比自动扩展到长边
- `unitSize`（1 像素 = N 单位）决定输出物理大小，从 Data 层读取，用户不直接指定
- `minRadiusRatio` 控制白色区域的跳过阈值，从 Data 层读取
