# Transform 命令接口文档

命名空间：`Rh.Cmd.Transform`

## 功能

对几何对象执行变换操作（不写入文档，返回变换后的几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Transform.json`

---

## 一、基本变换

### Move

对应 Rhino 命令：`Move`

| 项目 | 说明 |
|------|------|
| 功能 | 平移对象 |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d translation` — 平移向量 |
| 输出 | `GeometryBase` — 平移后的几何对象（原对象被修改） |
| 报错 | geometry 无效时返回 null |
| RhinoCommon | `Transform.Translation(Vector3d)` + `geometry.Transform()` |

### Copy

对应 Rhino 命令：`Copy`

| 项目 | 说明 |
|------|------|
| 功能 | 复制对象并平移到新位置（不修改原对象） |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d translation` — 平移向量 |
| 输出 | `GeometryBase` — 复制后的新对象 |
| 报错 | geometry 无效时返回 null |
| RhinoCommon | `geometry.Duplicate()` + `Transform.Translation()` |

### Rotate

对应 Rhino 命令：`Rotate` / `Rotate3D`

**重载 1：绕 Z 轴旋转（工作平面旋转）**

| 项目 | 说明 |
|------|------|
| 功能 | 绕垂直于工作平面的轴（Z 轴）旋转 |
| 输入 | `GeometryBase geometry` — 几何对象，`double angleRadians` — 旋转角度（弧度），`Point3d center` — 旋转中心 |
| 输出 | `GeometryBase` — 旋转后的几何对象 |
| RhinoCommon | `Transform.Rotation(angleRadians, center)` |

**重载 2：绕任意轴旋转（3D 旋转）**

| 项目 | 说明 |
|------|------|
| 功能 | 绕任意 3D 轴旋转 |
| 输入 | `GeometryBase geometry` — 几何对象，`double angleRadians` — 旋转角度（弧度），`Vector3d axis` — 旋转轴方向，`Point3d center` — 旋转中心 |
| 输出 | `GeometryBase` — 旋转后的几何对象 |
| 报错 | axis 为零向量时返回 null |
| RhinoCommon | `Transform.Rotation(angleRadians, axis, center)` |

### Scale

对应 Rhino 命令：`Scale` / `Scale1D` / `Scale2D` / `ScaleNU`

**重载 1：均匀缩放**

| 项目 | 说明 |
|------|------|
| 功能 | 三轴均匀缩放 |
| 输入 | `GeometryBase geometry` — 几何对象，`Point3d anchor` — 缩放中心，`double scaleFactor` — 缩放系数 |
| 输出 | `GeometryBase` — 缩放后的几何对象 |
| 报错 | scaleFactor ≤ 0 时返回 null |
| RhinoCommon | `Transform.Scale(anchor, scaleFactor)` |

**重载 2：非均匀缩放（三轴独立）**

| 项目 | 说明 |
|------|------|
| 功能 | 按指定平面的轴向非均匀缩放（Scale1D 传 x,1,1；Scale2D 传 x,y,1） |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane plane` — 定义轴向的平面，`double xFactor` — X 方向系数，`double yFactor` — Y 方向系数，`double zFactor` — Z 方向系数 |
| 输出 | `GeometryBase` — 缩放后的几何对象 |
| 报错 | 任一系数 ≤ 0 时返回 null |
| RhinoCommon | `Transform.Scale(plane, xFactor, yFactor, zFactor)` |

### Mirror

对应 Rhino 命令：`Mirror` / `Mirror 3Point`

**重载 1：镜像平面**

| 项目 | 说明 |
|------|------|
| 功能 | 以指定平面为对称面镜像 |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane mirrorPlane` — 镜像平面 |
| 输出 | `GeometryBase` — 镜像后的几何对象 |
| RhinoCommon | `Transform.Mirror(Plane)` |

**重载 2：点 + 法线**

| 项目 | 说明 |
|------|------|
| 功能 | 以过指定点且法线为指定方向的平面镜像 |
| 输入 | `GeometryBase geometry` — 几何对象，`Point3d pointOnPlane` — 镜像平面上的点，`Vector3d normal` — 镜像平面法线 |
| 输出 | `GeometryBase` — 镜像后的几何对象 |
| 报错 | normal 为零向量时返回 null |
| RhinoCommon | `Transform.Mirror(pointOnPlane, normal)` |

### Shear

对应 Rhino 命令：`Shear`

| 项目 | 说明 |
|------|------|
| 功能 | 剪切变形（平行于一个轴倾斜，不缩放） |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane plane` — 基平面，`Vector3d x` — X 方向，`Vector3d y` — Y 方向，`Vector3d z` — 剪切方向 |
| 输出 | `GeometryBase` — 变形后的几何对象 |
| RhinoCommon | `Transform.Shear(plane, x, y, z)` |

---

## 二、阵列

### ArrayLinear

对应 Rhino 命令：`ArrayLinear`

| 项目 | 说明 |
|------|------|
| 功能 | 沿直线方向均匀阵列 |
| 输入 | `GeometryBase geometry` — 几何对象，`Vector3d direction` — 阵列方向（长度即间距），`int count` — 数量 |
| 输出 | `GeometryBase[]` — 阵列结果数组（不含原始对象） |
| 报错 | count < 2 时返回 null；direction 为零向量时返回 null |
| RhinoCommon | 循环 `Transform.Translation(direction * i / (count - 1))` |

### ArrayRectangular

对应 Rhino 命令：`Array`（矩形阵列）

| 项目 | 说明 |
|------|------|
| 功能 | 矩形阵列（X/Y/Z 三方向） |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane plane` — 阵列平面（定义 X/Y/Z 方向），`int xCount` — X 方向数量 [可选]，`int yCount` — Y 方向数量 [可选]，`int zCount` — Z 方向数量 [可选]，`double xSpacing` — X 间距 [可选]，`double ySpacing` — Y 间距 [可选]，`double zSpacing` — Z 间距 [可选] |
| 输出 | `GeometryBase[]` — 阵列结果数组（含原始位置对象） |
| 报错 | 总数量 < 1 时返回 null |
| RhinoCommon | 三重循环 `Transform.Translation(plane.XAxis * xi * xSpacing + ...)` |

### ArrayPolar

对应 Rhino 命令：`ArrayPolar`

| 项目 | 说明 |
|------|------|
| 功能 | 环形阵列（绕轴旋转分布） |
| 输入 | `GeometryBase geometry` — 几何对象，`Line axis` — 旋转轴，`int count` — 数量，`double totalAngleRadians` — 总角度（弧度）[可选，默认 2π]，`bool rotate` — 副本是否随阵列旋转 [可选，默认 true] |
| 输出 | `GeometryBase[]` — 阵列结果数组（含原始位置对象） |
| 报错 | count < 2 时返回 null；axis 无效时返回 null |
| RhinoCommon | 循环 `Transform.Rotation(stepAngle * i, axis.Direction, axis.From)` |

### ArrayAlongCrv

对应 Rhino 命令：`ArrayCrv`

**重载 1：按数量等分**

| 项目 | 说明 |
|------|------|
| 功能 | 沿路径曲线均匀分布阵列 |
| 输入 | `GeometryBase geometry` — 几何对象，`Curve rail` — 路径曲线，`int count` — 数量，`bool orient` — 是否沿曲线方向定向 [可选，默认 false] |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | count < 2 时返回 null；rail 无效时返回 null |
| RhinoCommon | `rail.PointAt(t)` + `rail.PerpendicularFrameAt(t)` + `Transform.PlaneToPlane()` |

**重载 2：按间距分布**

| 项目 | 说明 |
|------|------|
| 功能 | 沿路径曲线按指定间距分布阵列 |
| 输入 | `GeometryBase geometry` — 几何对象，`Curve rail` — 路径曲线，`double spacing` — 间距，`bool orient` — 是否沿曲线方向定向 [可选，默认 false] |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| RhinoCommon | `rail.LengthParameter()` + `rail.PointAt()` + `Transform.PlaneToPlane()` |

### ArrayOnSrf

对应 Rhino 命令：`ArrayOnSrf` / `ArraySrf`

| 项目 | 说明 |
|------|------|
| 功能 | 在曲面 UV 方向上均匀阵列 |
| 输入 | `GeometryBase geometry` — 几何对象，`Brep surface` — 目标曲面，`int uCount` — U 向数量，`int vCount` — V 向数量 |
| 输出 | `GeometryBase[]` — 阵列结果数组 |
| 报错 | uCount 或 vCount < 1 时返回 null；surface 无效时返回 null |
| RhinoCommon | `surface.FramesAt(u, v)` + `Transform.PlaneToPlane()` |

---

## 三、定向

### Orient

对应 Rhino 命令：`Orient` / `Orient3Pt`

| 项目 | 说明 |
|------|------|
| 功能 | 将对象从源平面定向到目标平面 |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane source` — 源平面，`Plane target` — 目标平面 |
| 输出 | `GeometryBase` — 定向后的几何对象 |
| RhinoCommon | `Transform.PlaneToPlane(source, target)` |

### OrientOnSrf

对应 Rhino 命令：`OrientOnSrf`

| 项目 | 说明 |
|------|------|
| 功能 | 将对象定向到曲面上指定点（使用曲面法线确定方向） |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane source` — 源平面，`Brep surface` — 目标曲面，`Point3d targetPoint` — 曲面上的目标位置 |
| 输出 | `GeometryBase` — 定向后的几何对象 |
| 报错 | targetPoint 不在曲面上时返回 null |
| RhinoCommon | `surface.ClosestPointTo()` + `surface.FrameAt()` + `Transform.PlaneToPlane()` |

### OrientOnCrv

对应 Rhino 命令：`OrientOnCrv`

| 项目 | 说明 |
|------|------|
| 功能 | 将对象定向到曲线上指定参数位置（使用曲线垂直框架确定方向） |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane source` — 源平面，`Curve rail` — 目标曲线，`double parameter` — 曲线参数 |
| 输出 | `GeometryBase` — 定向后的几何对象 |
| 报错 | parameter 越界时返回 null |
| RhinoCommon | `rail.PerpendicularFrameAt(parameter)` + `Transform.PlaneToPlane()` |

### RemapCPlane

对应 Rhino 命令：`RemapCPlane`

| 项目 | 说明 |
|------|------|
| 功能 | 将对象从旧工作平面重映射到新工作平面 |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane oldCPlane` — 旧工作平面，`Plane newCPlane` — 新工作平面 |
| 输出 | `GeometryBase` — 重映射后的几何对象 |
| RhinoCommon | `Transform.PlaneToPlane(oldCPlane, newCPlane)` |

---

## 四、投影

### ProjectToCPlane

对应 Rhino 命令：`ProjectToCPlane`

| 项目 | 说明 |
|------|------|
| 功能 | 将对象正交投影到指定平面 |
| 输入 | `GeometryBase geometry` — 几何对象，`Plane plane` — 目标投影平面 |
| 输出 | `GeometryBase` — 投影后的几何对象 |
| RhinoCommon | `Transform.PlanarProjection(plane)` |

---

## 命令汇总

| 分类 | 方法数 | 重载数 | 命令列表 |
|------|--------|--------|---------|
| 基本变换 | 7 | 9 | Move, Copy, Rotate(2), Scale(2), Mirror(2), Shear |
| 阵列 | 5 | 6 | ArrayLinear, ArrayRectangular, ArrayPolar, ArrayAlongCrv(2), ArrayOnSrf |
| 定向 | 4 | 4 | Orient, OrientOnSrf, OrientOnCrv, RemapCPlane |
| 投影 | 1 | 1 | ProjectToCPlane |
| **合计** | **17** | **20** | |
