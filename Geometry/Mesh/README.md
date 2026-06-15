# Geometry/Mesh 说明文档

命名空间：`Rh.Geo.Msh`

## 功能

网格几何工具，提供网格图元创建、几何体转换、点集网格化和重网格化。纯几何计算，不访问 ActiveDoc。

---

## 方法一览

### 图元创建

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateFromBoundingBox` | BoundingBox, int x3 | Mesh | `Mesh.CreateFromBox` |
| `CreateFromBox` | Box, int x3 | Mesh | `Mesh.CreateFromBox` |
| `CreateFromCorners` | Point3d×2, Vector3d, int x3 | Mesh | Geometry 层构造 Box → `Mesh.CreateFromBox` |
| `CreateFromSphere` | Point3d, Vector3d, double, int×2 | Mesh | `Mesh.CreateFromSphere` |
| `CreateIcoSphere` | Point3d, Vector3d, double, int | Mesh | `Mesh.CreateIcoSphere` |
| `CreateFromCylinder` | Point3d, Vector3d, double×2, int×2, bool | Mesh | `Mesh.CreateFromCylinder` |
| `CreateFromCone` | Point3d, Vector3d, double×2, int×2 | Mesh | `Mesh.CreateFromCone` |
| `CreateFromTorus` | Point3d, Vector3d, double×2, int×2 | Mesh | `Mesh.CreateFromTorus` |
| `CreateEllipsoid` | Point3d, Vector3d, Vector3d, int×2 | Mesh | 球体 + `Transform.Scale` |
| `CreateFromPlane` | Plane, Interval×2, int×2 | Mesh | `Mesh.CreateFromPlane` |

### 从几何体转换

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateFromBrep` | Brep, MeshingParameters | Mesh[] | `Mesh.CreateFromBrep` |
| `CreateFromSurface` | Surface, MeshingParameters | Mesh[] | `Mesh.CreateFromSurface` |
| `CreateFromClosedPolyline` | Polyline | Mesh | `Mesh.CreateFromClosedPolyline` |
| `CreateFromPlanarBoundary` | Curve, double | Mesh | `Mesh.CreateFromPlanarBoundary` |
| `CreateExtrusion` | Curve, Vector3d | Mesh | `Mesh.CreateExtrusion` |

### 从点集创建

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `CreateConvexHull` | IEnumerable\<Point3d\>, double×2 | Mesh | `Mesh.CreateConvexHull3D` |
| `CreateFromTessellation` | IEnumerable\<Point3d\>, edges, Plane, bool | Mesh | `Mesh.CreateFromTessellation` |
| `CreatePatch` | IEnumerable\<Point3d\>, double | Mesh | 凸包近似 + 平面三角化退化 |

### 重网格化

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|-------------|
| `QuadRemeshFromBrep` | Brep, int, double | Mesh | `Mesh.QuadRemesh` |
| `QuadRemeshFromMesh` | Mesh, int, double | Mesh | `Mesh.QuadRemesh` |

---

## 注意事项

- **MeshingParameters**：默认使用 `MeshingParameters.Default`，Project 层可传入自定义参数（如 `FastRenderMesh`、`QualityRenderMesh`）
- **CreatePatch**：无直接 RhinoCommon API，用凸包 + 平面三角化近似。如需精确补面，使用 `RhinoApp.RunScript("_-MeshPatch")`
- **CreateEllipsoid**：先创建单位球再非均匀缩放，顶点分布可能不理想（赤道方向密集，极轴方向稀疏）
