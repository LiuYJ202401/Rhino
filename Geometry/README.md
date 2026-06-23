# Geometry 层

命名空间：`Rh.Geo`

## 目的

创建 Rhino 几何对象，进行基础几何操作。接收数据/参数，创建/处理 Rhino 几何对象，输出处理结果。

## 可使用的类型

- **可以使用** Rhino 几何对象：`Curve`、`NurbsCurve`、`Brep`、`Surface`、`Mesh`、`SubD` 等
- **可以使用** Rhino 数学原语：`Point3d`、`Vector3d`、`Transform`、`Plane` 等
- **可以使用** Math 层的所有方法

## 结构

```
Geometry/
├── PlaneGeo.cs              平面工具（PointToUV 等通用计算）
├── Curve/
│   ├── README.md            曲线工具说明文档
│   ├── CircleGeo.cs         圆创建（中心半径、直径、三点、切向）
│   ├── ArcGeo.cs            圆弧创建（中心角度、三点、起点终点方向）
│   ├── EllipseGeo.cs        椭圆创建（中心半径、直径、焦点）
│   ├── RectangleGeo.cs      矩形创建（对角点、中心宽高）
│   ├── PolygonGeo.cs        多边形顶点生成
│   ├── ConicGeo.cs          圆锥曲线手工 NURBS 构造
│   ├── CatenaryGeo.cs       悬链线牛顿迭代求解
│   ├── HelixGeo.cs          螺旋线垂直参考点计算
│   └── FilletGeo.cs         圆角相切圆/弧（含平行线半圆分支）
├── Surface/SurfaceGeo.cs    曲面创建（22 方法）
├── Solid/SolidGeo.cs        实体创建（20 方法）
├── Mesh/MeshGeo.cs          网格创建（18 方法）
├── Transform/TransformGeo.cs 变换工具（17 方法）
└── Image/
    ├── README.md            图片工具说明文档
    └── ImageGeo.cs          图片灰度采样 + 灰度→圆阵列（待实现）
```

### 核心原则

**所有方法接收 `Plane` 参数（或由输入点自动确定平面），不假设平面方向。**

### 已实现

| 文件 | 命名空间 | 方法 | 说明 |
|------|---------|------|------|
| PlaneGeo.cs | `Rh.Geo` | `PointToUV` | 世界坐标 → 平面 UV 参数 |
| Curve/CircleGeo.cs | `Rh.Geo.Crv` | `CreateFromCenterRadius` 等 4 个 | 圆创建（平面必须传入） |
| Curve/ArcGeo.cs | `Rh.Geo.Crv` | `CreateFromCenterAngle` 等 3 个 | 圆弧创建 |
| Curve/EllipseGeo.cs | `Rh.Geo.Crv` | `CreateFromCenterRadii` 等 3 个 | 椭圆创建 |
| Curve/RectangleGeo.cs | `Rh.Geo.Crv` | `CreateFromCorners` 等 2 个 | 矩形创建 |
| Curve/PolygonGeo.cs | `Rh.Geo.Crv` | `CreateRegularPolygon` 等 4 个 | 多边形生成 |
| Curve/ConicGeo.cs | `Rh.Geo.Crv` | `CreateConic`、`CreateHyperbola` | 圆锥曲线 |
| Curve/CatenaryGeo.cs | `Rh.Geo.Crv` | `Create` | 悬链线（接收重力方向） |
| Curve/HelixGeo.cs | `Rh.Geo.Crv` | `GetRadiusPoint` | 螺旋线参考点 |
| Image/ImageGeo.cs | `Rh.Geo.Img` | `SampleGrayscale`、`CreateCirclesFromGrayscale` | 图片灰度采样 + 灰度→圆阵列（待实现） |

## 规则

- 调用 Math 层进行数学复用
- 不直接与 RhinoDoc 交互（不调用 `doc.Objects.AddXxx`、不调用 `GetObject`）
- 创建/处理 Rhino 几何对象（Curve/Brep/Surface/Mesh），返回给上层
- 公差等参数从外部传入（由 Data 层 → Command 层 → 传入），不直接访问 `RhinoDoc.ActiveDoc`
- 报错：返回 null 或抛出带清晰信息的异常
