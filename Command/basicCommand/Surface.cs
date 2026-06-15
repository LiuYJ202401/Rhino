using System.Collections.Generic;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.Srf;

namespace Rh.Cmd
{
    /// <summary>
    /// 基础曲面命令。
    /// 创建各类曲面/多重曲面对象（不写入文档，返回几何对象）。
    /// 对应接口文档：Surface.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    ///   - 同一功能统一方法名，通过参数类型区分重载
    /// </summary>
    public static class SurfaceCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Surface.json";

        private static void UpdateDefault<T>(string key, T value)
        {
            DataReader.SetValue<T>(DataPath, key, value);
        }

        private static T GetDefault<T>(string key, T defaultValue)
        {
            return DataReader.GetValue<T>(DataPath, key, defaultValue);
        }

        private static double ActiveTolerance()
        {
            return DataReader.GetDynamicValue("ModelAbsoluteTolerance");
        }

        private static double ActiveAngleTolerance()
        {
            return DataReader.GetDynamicValue("ModelAngleToleranceRadians");
        }

        // ================================================================
        // 默认值查询（供 Project 层初始化 UI）
        // ================================================================

        public static int GetDefaultSrfDegree()
        {
            return GetDefault("CreateSrf.degree", 3);
        }

        public static int GetDefaultPatchSpans()
        {
            return GetDefault("CreatePatch.spans", 10);
        }

        public static int GetDefaultDrapeSpacing()
        {
            return GetDefault("CreateDrape.spacing", 10);
        }

        public static int GetDefaultHeightfieldSamples()
        {
            return GetDefault("CreateHeightfield.samples", 50);
        }

        // ================================================================
        // 一、平面类
        // ================================================================

        // ------------------------------------------------------------
        // CreatePlane
        // ------------------------------------------------------------

        /// <summary>
        /// 重载 1：平面 + UV 范围
        /// </summary>
        public static Brep CreatePlane(Plane plane, Interval domainU, Interval domainV, bool isPreview = false)
        {
            return SurfaceGeo.CreateFromPlane(plane, domainU, domainV);
        }

        /// <summary>
        /// 重载 2：三点
        /// </summary>
        public static Brep CreatePlane(Point3d first, Point3d second, Point3d third, bool isPreview = false)
        {
            return SurfaceGeo.CreateFrom3Points(first, second, third);
        }

        /// <summary>
        /// 重载 3：垂直平面
        /// </summary>
        public static Brep CreatePlane(Point3d start, Point3d end, double height, Vector3d workPlaneNormal, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("CreatePlane.verticalHeight", height);

            return SurfaceGeo.CreateVertical(start, end, height, workPlaneNormal);
        }

        public static double GetDefaultPlaneVerticalHeight()
        {
            return GetDefault("CreatePlane.verticalHeight", 1.0);
        }

        // ------------------------------------------------------------
        // CreatePlaneThroughPt
        // ------------------------------------------------------------

        public static Brep CreatePlaneThroughPt(IEnumerable<Point3d> points, bool isPreview = false)
        {
            return SurfaceGeo.CreateThroughPoints(points);
        }

        // ================================================================
        // 二、从点创建
        // ================================================================

        // ------------------------------------------------------------
        // CreateSrfPt
        // ------------------------------------------------------------

        public static Brep CreateSrfPt(Point3d p1, Point3d p2, Point3d p3, Point3d p4, bool isPreview = false)
        {
            return SurfaceGeo.CreateFromCorners(p1, p2, p3, p4);
        }

        // ------------------------------------------------------------
        // CreateSrfThroughPts
        // ------------------------------------------------------------

        public static Brep CreateSrfThroughPts(IEnumerable<Point3d> points, int uCount, int vCount,
            int uDegree = 3, int vDegree = 3, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateSrfThroughPts.uDegree", uDegree);
                UpdateDefault("CreateSrfThroughPts.vDegree", vDegree);
            }

            return SurfaceGeo.CreateThroughPointGrid(points, uCount, vCount, uDegree, vDegree);
        }

        public static int GetDefaultSrfThroughPtsDegree()
        {
            return GetDefault("CreateSrfThroughPts.uDegree", 3);
        }

        // ------------------------------------------------------------
        // CreateSrfControlPts
        // ------------------------------------------------------------

        public static Brep CreateSrfControlPts(IEnumerable<Point3d> points, int uCount, int vCount,
            int uDegree = 3, int vDegree = 3, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateSrfControlPts.uDegree", uDegree);
                UpdateDefault("CreateSrfControlPts.vDegree", vDegree);
            }

            return SurfaceGeo.CreateFromControlPointGrid(points, uCount, vCount, uDegree, vDegree);
        }

        public static int GetDefaultSrfControlPtsDegree()
        {
            return GetDefault("CreateSrfControlPts.uDegree", 3);
        }

        // ================================================================
        // 三、从曲线创建
        // ================================================================

        // ------------------------------------------------------------
        // CreateEdgeSrf
        // ------------------------------------------------------------

        public static Brep CreateEdgeSrf(IEnumerable<Curve> edges, bool isPreview = false)
        {
            var edgeList = new List<Curve>(edges);
            if (edgeList.Count < 2 || edgeList.Count > 4)
                return null;

            return SurfaceGeo.CreateEdgeSurface(edgeList);
        }

        // ------------------------------------------------------------
        // CreatePlanarSrf
        // ------------------------------------------------------------

        public static Brep[] CreatePlanarSrf(IEnumerable<Curve> curves, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreatePlanarBreps(curves, tol);
        }

        // ------------------------------------------------------------
        // CreateLoft
        // ------------------------------------------------------------

        public static Brep[] CreateLoft(IEnumerable<Curve> curves,
            LoftType loftType = LoftType.Normal, bool closed = false,
            Point3d start = default(Point3d), Point3d end = default(Point3d),
            bool isPreview = false)
        {
            var curveList = new List<Curve>(curves);
            if (curveList.Count < 2)
                return null;

            Point3d s = start == default(Point3d) ? Point3d.Unset : start;
            Point3d e = end == default(Point3d) ? Point3d.Unset : end;

            if (!isPreview)
            {
                UpdateDefault("CreateLoft.loftType", (int)loftType);
                UpdateDefault("CreateLoft.closed", closed);
            }

            return SurfaceGeo.CreateLoft(curveList, s, e, loftType, closed);
        }

        public static LoftType GetDefaultLoftType()
        {
            return (LoftType)GetDefault("CreateLoft.loftType", (int)LoftType.Normal);
        }

        public static bool GetDefaultLoftClosed()
        {
            return GetDefault("CreateLoft.closed", false);
        }

        // ------------------------------------------------------------
        // CreateNetworkSrf
        // ------------------------------------------------------------

        /// <summary>
        /// 重载 1：自动排序
        /// </summary>
        public static Brep CreateNetworkSrf(IEnumerable<Curve> curves,
            int continuity = 1,
            double edgeTolerance = -1, double interiorTolerance = -1, double angleTolerance = -1,
            bool isPreview = false)
        {
            double eTol = edgeTolerance < 0 ? ActiveTolerance() : edgeTolerance;
            double iTol = interiorTolerance < 0 ? ActiveTolerance() : interiorTolerance;
            double aTol = angleTolerance < 0 ? ActiveAngleTolerance() : angleTolerance;

            return SurfaceGeo.CreateNetworkSurface(curves, continuity, eTol, iTol, aTol);
        }

        /// <summary>
        /// 重载 2：手动指定 U/V
        /// </summary>
        public static Brep CreateNetworkSrf(IEnumerable<Curve> uCurves, IEnumerable<Curve> vCurves,
            int continuity = 1,
            double edgeTolerance = -1, double interiorTolerance = -1, double angleTolerance = -1,
            bool isPreview = false)
        {
            double eTol = edgeTolerance < 0 ? ActiveTolerance() : edgeTolerance;
            double iTol = interiorTolerance < 0 ? ActiveTolerance() : interiorTolerance;
            double aTol = angleTolerance < 0 ? ActiveAngleTolerance() : angleTolerance;

            return SurfaceGeo.CreateNetworkSurface(uCurves, vCurves, continuity, eTol, iTol, aTol);
        }

        // ================================================================
        // 四、扫掠类
        // ================================================================

        // ------------------------------------------------------------
        // CreateSweep
        // ------------------------------------------------------------

        /// <summary>
        /// 重载 1：单轨
        /// </summary>
        public static Brep[] CreateSweep(Curve rail, IEnumerable<Curve> shapes,
            bool closed = false, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateSweep1(rail, shapes, closed, tol);
        }

        /// <summary>
        /// 重载 2：双轨
        /// </summary>
        public static Brep[] CreateSweep(Curve rail1, Curve rail2, IEnumerable<Curve> shapes,
            bool closed = false, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateSweep2(rail1, rail2, shapes, closed, tol);
        }

        // ================================================================
        // 五、旋转类
        // ================================================================

        // ------------------------------------------------------------
        // CreateRevolve
        // ------------------------------------------------------------

        /// <summary>
        /// 重载 1：完整旋转 360°
        /// </summary>
        public static Brep CreateRevolve(Curve profile, Line axis, bool isPreview = false)
        {
            return SurfaceGeo.CreateRevolveFull(profile, axis);
        }

        /// <summary>
        /// 重载 2：部分旋转
        /// </summary>
        public static Brep CreateRevolve(Curve profile, Line axis,
            double startAngle = 0.0, double endAngle = -1, bool isPreview = false)
        {
            double end = endAngle < 0 ? 2.0 * System.Math.PI : endAngle;

            if (!isPreview)
            {
                UpdateDefault("CreateRevolve.startAngle", startAngle);
                UpdateDefault("CreateRevolve.endAngle", end);
            }

            return SurfaceGeo.CreateRevolvePartial(profile, axis, startAngle, end);
        }

        public static double GetDefaultRevolveStartAngle()
        {
            return GetDefault("CreateRevolve.startAngle", 0.0);
        }

        public static double GetDefaultRevolveEndAngle()
        {
            return GetDefault("CreateRevolve.endAngle", 2.0 * System.Math.PI);
        }

        // ------------------------------------------------------------
        // CreateRailRevolve
        // ------------------------------------------------------------

        public static Brep CreateRailRevolve(Curve profile, Curve rail, Line axis,
            bool scaleHeight = false, bool isPreview = false)
        {
            return SurfaceGeo.CreateRailRevolve(profile, rail, axis, scaleHeight);
        }

        // ================================================================
        // 六、挤出类
        // ================================================================

        // ------------------------------------------------------------
        // CreateExtrude
        // ------------------------------------------------------------

        /// <summary>
        /// 重载 1：方向挤出
        /// </summary>
        public static Brep CreateExtrude(Curve profile, Vector3d direction, bool isPreview = false)
        {
            return SurfaceGeo.CreateExtrudeDirection(profile, direction);
        }

        /// <summary>
        /// 重载 2：沿曲线挤出
        /// </summary>
        public static Brep CreateExtrude(Curve profile, Curve path, bool cap = false, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateExtrudeAlongCrv(profile, path, cap, tol);
        }

        /// <summary>
        /// 重载 3：锥状挤出
        /// </summary>
        public static Brep CreateExtrude(Curve profile, Vector3d direction, double distance,
            double draftAngle, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("CreateExtrudeTapered.draftAngle", draftAngle);

            double tol = ActiveTolerance();
            double aTol = ActiveAngleTolerance();
            return SurfaceGeo.CreateExtrudeTapered(profile, direction, distance, draftAngle, tol, aTol);
        }

        public static double GetDefaultExtrudeDraftAngle()
        {
            return GetDefault("CreateExtrudeTapered.draftAngle", 0.0873); // ~5°
        }

        /// <summary>
        /// 重载 4：挤出到点
        /// </summary>
        public static Brep CreateExtrude(Curve profile, Point3d apex, bool isPreview = false)
        {
            return SurfaceGeo.CreateExtrudeToPoint(profile, apex);
        }

        // ================================================================
        // 七、补面与拟合
        // ================================================================

        // ------------------------------------------------------------
        // CreatePatch
        // ------------------------------------------------------------

        public static Brep CreatePatch(IEnumerable<GeometryBase> geometry,
            int uSpans = 10, int vSpans = 10, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreatePatch.uSpans", uSpans);
                UpdateDefault("CreatePatch.vSpans", vSpans);
            }

            double tol = ActiveTolerance();
            return SurfaceGeo.CreatePatch(geometry, uSpans, vSpans, tol);
        }

        // ================================================================
        // 八、特殊曲面
        // ================================================================

        // ------------------------------------------------------------
        // CreateCutPlane
        // ------------------------------------------------------------

        public static Brep CreateCutPlane(Plane plane, IEnumerable<GeometryBase> objects, bool isPreview = false)
        {
            return SurfaceGeo.CreateCutPlane(plane, objects);
        }

        // ------------------------------------------------------------
        // CreateRibbon
        // ------------------------------------------------------------

        public static Brep CreateRibbon(Curve curve, double distance, Plane plane, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateRibbon(curve, distance, plane, tol);
        }

        // ------------------------------------------------------------
        // CreateFin
        // ------------------------------------------------------------

        public static Brep CreateFin(Curve curve, BrepFace face, double height, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateFin(curve, face, height, tol);
        }

        // ------------------------------------------------------------
        // CreateDrape
        // ------------------------------------------------------------

        public static Brep CreateDrape(IEnumerable<GeometryBase> objects, Plane plane,
            int uSpacing = 10, int vSpacing = 10, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateDrape.uSpacing", uSpacing);
                UpdateDefault("CreateDrape.vSpacing", vSpacing);
            }

            double tol = ActiveTolerance();
            return SurfaceGeo.CreateDrape(objects, plane, uSpacing, vSpacing, tol);
        }

        // ------------------------------------------------------------
        // CreateHeightfield
        // ------------------------------------------------------------

        public static Brep CreateHeightfield(string imagePath, Plane plane,
            double width, double heightSize, double maxHeight,
            int samplesX = 50, int samplesY = 50, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateHeightfield.samplesX", samplesX);
                UpdateDefault("CreateHeightfield.samplesY", samplesY);
            }

            return SurfaceGeo.CreateHeightfield(imagePath, plane, width, heightSize, maxHeight, samplesX, samplesY);
        }

        // ------------------------------------------------------------
        // CreateDevLoft
        // ------------------------------------------------------------

        public static Brep CreateDevLoft(Curve rail1, Curve rail2, bool isPreview = false)
        {
            double tol = ActiveTolerance();
            return SurfaceGeo.CreateDevLoft(rail1, rail2, tol);
        }
    }
}
