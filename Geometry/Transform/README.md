# TransformGeo 几何工具

命名空间：`Rh.Geo.Trs`

## 功能

对已有几何对象施加变换（平移/旋转/缩放/镜像/阵列/定向/投影）。
纯几何计算，不假设平面方向，不访问 ActiveDoc。

## 方法表

### 基本变换

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `Move` | GeometryBase, Vector3d | GeometryBase | `Transform.Translation()` |
| `Copy` | GeometryBase, Vector3d | GeometryBase | `Duplicate()` + `Transform.Translation()` |
| `Rotate` (Z轴) | GeometryBase, double, Point3d | GeometryBase | `Transform.Rotation(angle, center)` |
| `Rotate` (任意轴) | GeometryBase, double, Vector3d, Point3d | GeometryBase | `Transform.Rotation(angle, axis, center)` |
| `Scale` (均匀) | GeometryBase, Point3d, double | GeometryBase | `Transform.Scale(anchor, factor)` |
| `Scale` (非均匀) | GeometryBase, Plane, double, double, double | GeometryBase | `Transform.Scale(plane, x, y, z)` |
| `Mirror` (平面) | GeometryBase, Plane | GeometryBase | `Transform.Mirror(Plane)` |
| `Mirror` (点+法线) | GeometryBase, Point3d, Vector3d | GeometryBase | `Transform.Mirror(point, normal)` |
| `Shear` | GeometryBase, Plane, Vector3d, Vector3d, Vector3d | GeometryBase | `Transform.Shear(plane, x, y, z)` |

### 阵列

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `ArrayLinear` | GeometryBase, Vector3d, int | GeometryBase[] | 循环 `Transform.Translation()` |
| `ArrayRectangular` | GeometryBase, Plane, int×3, double×3 | GeometryBase[] | 三重循环 `Transform.Translation()` |
| `ArrayPolar` | GeometryBase, Line, int, double, bool | GeometryBase[] | 循环 `Transform.Rotation()` |
| `ArrayAlongCrv` (按数量) | GeometryBase, Curve, int, bool | GeometryBase[] | `Curve.PerpendicularFrameAt()` + `PlaneToPlane()` |
| `ArrayAlongCrv` (按间距) | GeometryBase, Curve, double, bool | GeometryBase[] | `Curve.LengthParameter()` + `PlaneToPlane()` |
| `ArrayOnSrf` | GeometryBase, Brep, int, int | GeometryBase[] | `Surface.FrameAt()` + `PlaneToPlane()` |

### 定向

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `Orient` | GeometryBase, Plane, Plane | GeometryBase | `Transform.PlaneToPlane()` |
| `OrientOnSrf` | GeometryBase, Plane, Brep, Point3d | GeometryBase | `BrepFace.ClosestPoint()` + `FrameAt()` + `PlaneToPlane()` |
| `OrientOnCrv` | GeometryBase, Plane, Curve, double | GeometryBase | `Curve.PerpendicularFrameAt()` + `PlaneToPlane()` |
| `RemapCPlane` | GeometryBase, Plane, Plane | GeometryBase | `Transform.PlaneToPlane()` |

### 投影

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `ProjectToCPlane` | GeometryBase, Plane | GeometryBase | `Transform.PlanarProjection()` |
