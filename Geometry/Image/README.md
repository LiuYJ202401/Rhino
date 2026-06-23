# ImageGeo 接口文档

命名空间：`Rh.Geo.Img`

## 目的

图片像素数据到几何对象的转换。提供通用的图片灰度采样工具，以及从灰度矩阵生成几何对象的方法。

**核心设计**：将"图片采样"与"几何生成"解耦——采样方法输出灰度矩阵，几何方法只接收灰度矩阵，不依赖图片文件。这样几何生成可以被任何灰度数据来源复用。

## 方法

### SampleGrayscale

将图片采样为灰度矩阵，采用单像素中心采样（与 Heightfield 一致）。

| 项目 | 说明 |
|------|------|
| 功能 | 读取图片，按指定密度采样为灰度矩阵 |
| 输入 | `Bitmap bmp` — System.Drawing.Bitmap，图片对象 |
|        | `int samplesX` — X 方向（图片宽度方向）采样数 |
|        | `int samplesY` — Y 方向（图片高度方向）采样数 |
| 输出 | `double[,]` — 灰度矩阵，维度 `[samplesX, samplesY]`，值域 `[0.0=黑, 1.0=白]` |
| 报错 | `bmp` 为 null → 返回 null；`samplesX` 或 `samplesY` ≤ 0 → 返回 null |
| RhinoCommon | 无直接 API，使用 `System.Drawing.Bitmap.GetPixel` |

**灰度公式**：

```
gray = (R × 0.299 + G × 0.587 + B × 0.114) / 255.0
```

**采样方式**：将图片宽高等分为 samplesX × samplesY 个区域，取每个区域中心位置的单个像素。区域中心坐标计算：

```
stepX = bmp.Width / samplesX
stepY = bmp.Height / samplesY
px = (int)((i + 0.5) × stepX)    // 第 i 列的中心像素 x 坐标
py = (int)((j + 0.5) × stepY)    // 第 j 行的中心像素 y 坐标
```

**输出矩阵约定**：`grayMatrix[i, j]` 中 `i` 对应图片横向（X），`j` 对应图片纵向（Y）。`j=0` 是图片顶部第一行。

### CreateCirclesFromGrayscale

从灰度矩阵生成圆阵列（半调/Halftone 效果：越暗越大，越白越小或跳过）。

| 项目 | 说明 |
|------|------|
| 功能 | 将灰度矩阵映射为物理空间中的圆阵列 |
| 输入 | `double[,] grayMatrix` — 灰度矩阵（来自 SampleGrayscale 或其他来源） |
|        | `Plane plane` — 放置平面，圆心在平面坐标系内分布 |
|        | `double physWidth` — 灰度矩阵对应的物理总宽度 |
|        | `double physHeight` — 灰度矩阵对应的物理总高度 |
|        | `double minRadiusRatio` — 最小半径比例（0~1），半径低于 `maxRadius × minRadiusRatio` 的圆跳过 |
| 输出 | `Circle[]` — 圆阵列（已跳过近白区域），每个圆在 `plane` 上 |
| 报错 | `grayMatrix` 为 null → 返回 null |
| RhinoCommon | `new Circle(Plane, Point3d, double)` 直接构造 |

**单元尺寸计算**：

```
samplesX = grayMatrix.GetLength(0)    // 矩阵列数 = X 方向采样数
samplesY = grayMatrix.GetLength(1)    // 矩阵行数 = Y 方向采样数
cellW = physWidth / samplesX          // 每个单元的物理宽度
cellH = physHeight / samplesY         // 每个单元的物理高度
maxRadius = min(cellW, cellH) / 2     // 最大半径 = 单元内切圆半径
```

**灰度→半径映射**：

```
gray = grayMatrix[i, j]
darkness = 1.0 - gray                 // 黑=1.0（满格），白=0.0（无圆）
radius = maxRadius × darkness
if radius < maxRadius × minRadiusRatio → 跳过（近白区域不生成圆）
```

**圆心位置计算**（坐标翻转）：

图片像素行 `j=0` 在顶部，但 Rhino Y 轴向上。因此翻转 Y 方向，使图片顶部对应平面 Y 最大值：

```
centerLocalX = i × cellW + cellW / 2
centerLocalY = physHeight - (j × cellH + cellH / 2)    // Y 翻转
center = plane.Origin + plane.XAxis × centerLocalX + plane.YAxis × centerLocalY
```

## 复用说明

| 方法 | 可被复用的场景 |
|------|--------------|
| `SampleGrayscale` | 圆拟合、方块拟合、点密度图、Heightfield 重构等所有"图片→几何"功能 |
| `CreateCirclesFromGrayscale` | 接收任意灰度矩阵，数据来源不限于图片（也可来自数学函数生成的矩阵） |

## 规则

- 不直接与 RhinoDoc 交互（不调用 `doc.Objects`、不调用 `GetObject`）
- 不读取文件路径（`SampleGrayscale` 接收已加载的 `Bitmap`，文件 I/O 由上层处理）
- 纯几何计算，所有平面参数从外部传入
