# Geometry/Surface 说明文档

命名空间：`Rh.Geo.Srf`

## 职责

曲面几何工具，纯几何计算。不假设平面方向，不访问 ActiveDoc。

---

## SurfaceGeo

| 方法 | 输入 | 输出 | RhinoCommon |
|------|------|------|------------|
| `CreateFromPlane` | Plane, Interval, Interval | Brep | `NurbsSurface.CreateFromPlane` → `Brep.CreateFromSurface` |
| `CreateFrom3Points` | Point3d ×3 | Brep | 无直接构造，计算平面+UV 范围后调用 `CreateFromPlane` |
| `CreateVertical` | Point3d ×2, double, Vector3d | Brep | 无直接构造，计算垂直平面后调用 `CreateFromPlane` |
| `CreateThroughPoints` | IEnumerable\<Point3d\> | Brep | `Plane.FitPlaneToPoints` → `CreateFromPlane` |
| `CreateFromCorners` | Point3d ×4 (第4个可为 Unset) | Brep | `NurbsSurface.CreateFromCorners` |
| `CreateThroughPointGrid` | IEnumerable\<Point3d\>, int×4 | Brep | `NurbsSurface.CreateThroughPoints` |
| `CreateFromControlPointGrid` | IEnumerable\<Point3d\>, int×4 | Brep | `NurbsSurface.CreateFromPoints` |
| `CreateEdgeSurface` | IEnumerable\<Curve\> | Brep | `Brep.CreateEdgeSurface` |
| `CreatePlanarBreps` | IEnumerable\<Curve\>, double | Brep[] | `Brep.CreatePlanarBreps` |
| `CreateLoft` | IEnumerable\<Curve\>, Point3d×2, LoftType, bool | Brep[] | `Brep.CreateFromLoft` |
| `CreateNetworkSurface` | IEnumerable\<Curve\>, int, double×3 | Brep | `NurbsSurface.CreateNetworkSurface` |
| `CreateNetworkSurface` | IEnumerable\<Curve\>×2, int, double×3 | Brep | `NurbsSurface.CreateNetworkSurface` (手动 U/V) |
| `CreateSweep1` | Curve, IEnumerable\<Curve\>, bool, double | Brep[] | `Brep.CreateFromSweep` (单轨) |
| `CreateSweep2` | Curve×2, IEnumerable\<Curve\>, bool, double | Brep[] | `Brep.CreateFromSweep` (双轨) |
| `CreateRevolveFull` | Curve, Line | Brep | `RevSurface.Create(profile, axis, 0, 2π)` |
| `CreateRevolvePartial` | Curve, Line, double×2 | Brep | `RevSurface.Create(profile, axis, start, end)` |
| `CreateRailRevolve` | Curve×2, Line, bool | Brep | `NurbsSurface.CreateRailRevolvedSurface` |
| `CreateExtrudeDirection` | Curve, Vector3d | Brep | `Surface.CreateExtrusion` |
| `CreateExtrudeAlongCrv` | Curve×2, bool, double | Brep | `Brep.CreateFromSweep` (path 作 rail) |
| `CreateExtrudeTapered` | Curve, Vector3d, double×2, double×2 | Brep | `Brep.CreateFromTaperedExtrude` |
| `CreateExtrudeToPoint` | Curve, Point3d | Brep | `Surface.CreateExtrusionToPoint` |
| `CreatePatch` | IEnumerable\<GeometryBase\>, int×2, double | Brep | `Brep.CreatePatch` |
