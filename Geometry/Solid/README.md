# Geometry/Solid 说明文档

命名空间：`Rh.Geo.Sld`

## 功能

实体几何工具，创建闭合实体（Brep，`IsSolid=true`）。与 SurfaceGeo 的核心区别：强制调用 `CapPlanarHoles` 或使用带 `cap` 参数的 API。纯几何计算，不访问 ActiveDoc。

---

## 方法一览

### 单曲面实体

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateSphere` | Point3d, Vector3d, double | Brep | `Sphere.ToBrep()` |
| `CreateEllipsoid` | Point3d, Vector3d, Vector3d | Brep | Sphere + `Transform.Scale` |
| `CreateTorus` | Point3d, Vector3d, double×2 | Brep | `Torus.ToRevSurface()` → `Brep.CreateFromSurface` |

### 多曲面实体

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateFromBox` | Box | Brep | `Brep.CreateFromBox` |
| `CreateFromCorners` | Point3d×2, Vector3d | Brep | 构造 Box → `Brep.CreateFromBox` |
| `CreateCylinder` | Point3d, Vector3d, double×2, bool | Brep | `Cylinder.ToBrep(cap)` |
| `CreateCone` | Point3d, Vector3d, double×2, bool | Brep | `Cone.ToBrep(cap)` |
| `CreateTruncatedCone` | Point3d, Vector3d, double×3, bool | Brep | loft 上下两圆 + CapPlanarHoles |
| `CreateTube` | Point3d, Vector3d, double×3, bool | Brep | 双 Cylinder + `CreateBooleanDifference` |
| `CreatePyramid` | Point3d, Vector3d, int, double×2, bool | Brep | 多边形 + 三角侧面 + CapPlanarHoles |
| `CreateTruncatedPyramid` | Point3d, Vector3d, int, double×3, bool | Brep | 双多边形 + 四边侧面 |

### 从曲线挤出为实体

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateExtrudeSolid` | Curve, Vector3d, bool | Brep | `Surface.CreateExtrusion` + `CapPlanarHoles` |
| `CreateRevolveSolid` | Curve, Line, double×2, bool | Brep | `RevSurface.Create` + `CapPlanarHoles` |
| `CreateSweepSolid` | Curve×2, IEnumerable\<Curve\>, bool | Brep | `Brep.CreateFromSweep` + `CapPlanarHoles` |
| `CreateLoftSolid` | IEnumerable\<Curve\>, int, bool | Brep | `Brep.CreateFromLoft` + `CapPlanarHoles` |

### Solid 专属命令

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreatePipe` | Curve, double, PipeCapMode | Brep | `Brep.CreatePipe` |
| `CreateThickPipe` | Curve, double×2, PipeCapMode | Brep | `Brep.CreateThickPipe` |
| `CreateSlab` | PolylineCurve, double, Vector3d, bool | Brep | `Curve.CreateOffset` + `CreateRuledSurface` + `CapPlanarHoles` |
| `CreateThicken` | Brep, double, bool | Brep | `Brep.CreateFromOffsetFace(createSolid=true)` |

### 辅助命令

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateCap` | Brep | Brep | `brep.CapPlanarHoles` |
| `CreateSolidFromBreps` | IEnumerable\<Brep\> | Brep | `Brep.CreateSolid` |

---

## TODO

### CreateTextObject

`TextEntity.CreateCurves` 需要 `DimensionStyle` 中的 `FontIndex`，而 `FontIndex` 必须通过 `doc.Fonts.FindOrCreate` 获取——这违反了 Geometry 层不访问 ActiveDoc 的规则。

**解决方案**：在 Project 层获取 `FontIndex`，传入 Command 层作为 `int fontIndex` 参数，Command 层构造 `DimensionStyle` 后传入 Geometry 层。待 Project 层实现时补充。

---

## 注意事项

- **CapPlanarHoles 依赖公差**：封盖成功与否取决于公差参数，从 Data 层动态获取
- **CreateTube 使用布尔差集**：需要公差参数，结果可能因精度产生缝隙
- **Pyramid/TruncatedPyramid 使用 Brep.Append**：手动组装面后需 `JoinNakedEdges` 合并开放边
