# Transform 命令接口文档

命名空间：`Rh.Cmd.Transform`

## 功能

对几何对象执行变换操作（不写入文档，返回变换后的几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Transform.md`

---

## 一、基本变换

### Move

| 项目 | 内容 |
|------|------|
| 功能 | 平移对象 |
| 对应命令 | Move |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d translation` — 平移向量 |
| 输出 | `GeometryBase` — 平移后的几何对象 |
| 报错 | GeometryInvalidException — geometry 无效；ZeroVectorException — 平移向量为零 |

### Copy

| 项目 | 内容 |
|------|------|
| 功能 | 复制对象并平移到新位置 |
| 对应命令 | Copy |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d translation` — 平移向量 |
| 输出 | `GeometryBase` — 复制后的新对象 |
| 报错 | GeometryInvalidException — geometry 无效 |

### Rotate

| 项目 | 内容 |
|------|------|
| 功能 | 旋转对象 |
| 对应命令 | Rotate |
| 输入 | `GeometryBase geometry` — 几何对象，`double angleRadians` — 旋转角度（弧度），`Vector3d axis` — 旋转轴，`Point3d center` — 旋转中心 |
| 输出 | `GeometryBase` — 旋转后的几何对象 |
| 报错 | GeometryInvalidException — geometry 无效；ZeroAxisException — 旋转轴为零向量 |

### Scale

| 项目 | 内容 |
|------|------|
| 功能 | 均匀缩放对象 |
| 对应命令 | Scale（均匀模式） |
| 输入 | `GeometryBase geometry` — 几何对象，`Point3d center` — 缩放中心，`double scaleFactor` — 均匀缩放系数 |
| 输出 | `GeometryBase` — 缩放后的几何对象 |
| 报错 | ScaleInvalidException — scaleFactor ≤ 0 |

### ScaleNonUniform

| 项目 | 内容 |
|------|------|
| 功能 | 非均匀缩放对象（三轴独立） |
| 对应命令 | Scale（非均匀模式 / Scale3D） |
| 输入 | `GeometryBase geometry` — 几何对象，`Point3d center` — 缩放中心，`double x` — X 方向系数，`double y` — Y 方向系数，`double z` — Z 方向系数 |
| 输出 | `GeometryBase` — 缩放后的几何对象 |
| 报错 | ScaleInvalidException — 任一系数 ≤ 0 |

### Mirror

| 项目 | 内容 |
|------|------|
| 功能 | 镜像对象 |
| 对应命令 | Mirror |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane mirrorPlane` — 镜像平面 |
| 输出 | `GeometryBase` — 镜像后的几何对象 |
| 报错 | PlaneInvalidException — 镜像平面无效 |

---

## 二、阵列

### ArrayLinear

| 项目 | 内容 |
|------|------|
| 功能 | 沿直线方向阵列 |
| 对应命令 | ArrayLinear |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d direction` — 阵列方向，`int count` — 数量，`double spacing` — 间距 [可选，默认方向向量长度] |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | CountInvalidException — count < 1；ZeroVectorException — 方向为零向量 |

### ArrayRectangular

| 项目 | 内容 |
|------|------|
| 功能 | 矩形阵列（X/Y/Z 三方向） |
| 对应命令 | ArrayRectangular |
| 输入 | `GeometryBase geometry` — 几何对象，`int xCount` — X 方向数量 [可选，默认 1]，`int yCount` — Y 方向数量 [可选，默认 1]，`int zCount` — Z 方向数量 [可选，默认 1]，`double xSpacing` — X 间距，`double ySpacing` — Y 间距，`double zSpacing` — Z 间距 |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | CountInvalidException — 任一方向数量 < 1 |

### ArrayPolar

| 项目 | 内容 |
|------|------|
| 功能 | 环形阵列（绕轴旋转） |
| 对应命令 | ArrayPolar |
| 输入 | `GeometryBase geometry` — 几何对象，`Line axis` — 旋转轴，`int count` — 数量，`double totalAngle` — 总角度（弧度）[可选，默认 2π] |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | CountInvalidException — count < 1；AxisInvalidException — 轴无效 |

### ArrayAlongCrv

| 项目 | 内容 |
|------|------|
| 功能 | 沿曲线阵列 |
| 对应命令 | ArrayCrv |
| 输入 | `GeometryBase geometry` — 几何对象，`Curve rail` — 路径曲线，`int count` — 数量，`bool orient` — 是否沿曲线方向定向 [可选，默认 true] |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | CountInvalidException — count < 1；RailInvalidException — 路径无效 |

### ArrayOnSrf

| 项目 | 内容 |
|------|------|
| 功能 | 沿曲面阵列（沿曲面 UV 方向分布） |
| 对应命令 | ArrayOnSrf |
| 输入 | `GeometryBase geometry` — 几何对象，`Brep surface` — 目标曲面，`int uCount` — U 向数量，`int vCount` — V 向数量 |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | CountInvalidException — 数量 < 1；SurfaceInvalidException — 曲面无效 |

---

## 三、定向

### Orient

| 项目 | 内容 |
|------|------|
| 功能 | 将对象从源平面定向到目标平面 |
| 对应命令 | Orient |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane source` — 源平面，`Plane target` — 目标平面 |
| 输出 | `GeometryBase` — 定向后的几何对象 |
| 报错 | PlaneInvalidException — 平面无效 |

### OrientOnSrf

| 项目 | 内容 |
|------|------|
| 功能 | 将对象从源平面定向到曲面上指定点（使用曲面法线） |
| 对应命令 | OrientOnSrf |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane source` — 源平面，`Brep surface` — 目标曲面，`Point3d targetPoint` — 目标位置 |
| 输出 | `GeometryBase` — 定向后的几何对象 |
| 报错 | PlaneInvalidException — 平面无效；PointNotOnSrfException — 点不在曲面上 |

---

## 命令汇总

| 分类 | 命令数 | 命令列表 |
|------|--------|---------|
| 基本变换 | 6 | Move, Copy, Rotate, Scale, ScaleNonUniform, Mirror |
| 阵列 | 5 | ArrayLinear, ArrayRectangular, ArrayPolar, ArrayAlongCrv, ArrayOnSrf |
| 定向 | 2 | Orient, OrientOnSrf |
| **合计** | **13** | |
