# Math 层

命名空间：`Rh.Math`

## 目的

最基本、纯数学的运算。

## 可使用的类型

- **可以使用** Rhino 数学原语：`Point3d`、`Vector3d`、`Transform`、`Plane`、`Interval`、`Line`、`BoundingBox` 等
- **不可使用** Rhino 几何对象：`Curve`、`Brep`、`Surface`、`Mesh`、`SubD` 等

## 结构

<!-- 按数学主题组织代码文件或子文件夹 -->

（待填充）

## 规则

- 不接触 Rhino 几何对象（Curve/Brep/Surface/Mesh 等）
- 可以使用 Rhino 数学原语（Point3d、Vector3d、Transform、Plane 等）
- 不直接访问 `RhinoDoc.ActiveDoc`
- 可被 Geometry 层及更上层调用复用
- 报错：返回错误码/null 或抛出带清晰信息的异常
