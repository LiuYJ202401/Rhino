using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo;
using Rh.Geo.Crv;

namespace Rh.Cmd
{
    /// <summary>
    /// 基础曲线命令。
    /// 创建各类曲线对象（不写入文档，返回几何对象）。
    /// 对应接口文档：Curve.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    ///   - 同一功能统一方法名，通过参数类型区分重载
    /// </summary>
    public static class CurveCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Curve.json";

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

        // ================================================================
        // 默认值查询（供 Project 层初始化 UI）
        // ================================================================

        public static double GetDefaultCircleRadius()
        {
            return GetDefault("CreateCircle.radius", 1.0);
        }

        public static double GetDefaultArcStartAngle()
        {
            return GetDefault("CreateArc.startAngle", 0.0);
        }

        public static double GetDefaultArcEndAngle()
        {
            return GetDefault("CreateArc.endAngle", System.Math.PI / 2.0);
        }

        public static bool GetDefaultPolylineClosed()
        {
            return GetDefault("CreatePolyline.closed", false);
        }

        public static double GetDefaultConicRho()
        {
            return GetDefault("CreateConic.rho", 0.5);
        }

        public static int GetDefaultNurbsDegree()
        {
            return GetDefault("CreateNurbsCurve.degree", 3);
        }

        public static bool GetDefaultNurbsPeriodic()
        {
            return GetDefault("CreateNurbsCurve.periodic", false);
        }

        public static int GetDefaultInterpDegree()
        {
            return GetDefault("CreateInterpCrv.degree", 3);
        }

        public static double GetDefaultHelixStartRadius()
        {
            return GetDefault("CreateHelix.startRadius", 1.0);
        }

        public static double GetDefaultHelixEndRadius()
        {
            return GetDefault("CreateHelix.endRadius", 1.0);
        }

        public static double GetDefaultHelixPitch()
        {
            return GetDefault("CreateHelix.pitch", 1.0);
        }

        public static double GetDefaultSpiralStartRadius()
        {
            return GetDefault("CreateSpiral.startRadius", 1.0);
        }

        public static double GetDefaultSpiralEndRadius()
        {
            return GetDefault("CreateSpiral.endRadius", 5.0);
        }

        public static int GetDefaultDivideSegmentCount()
        {
            return GetDefault("CreateDividePoints.segmentCount", 2);
        }

        // ================================================================
        // 圆
        // ================================================================

        /// <summary>
        /// 创建圆（重载 1：平面 + 圆心 + 半径）。
        /// 调用 Geometry 层 CircleGeo.CreateFromCenterRadius。
        /// </summary>
        public static Circle CreateCircle(Plane plane, Point3d center, double radius, bool isPreview = false)
        {
            if (radius <= 0)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：半径 ≤ 0");
                return Circle.Unset;
            }

            Circle circle = CircleGeo.CreateFromCenterRadius(plane, center, radius);

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 2：圆心 + 法向量 + 半径）。
        /// 由法向量构造平面后调用 Geometry 层。
        /// </summary>
        public static Circle CreateCircle(Point3d center, Vector3d normal, double radius, bool isPreview = false)
        {
            if (radius <= 0 || normal.IsZero)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：半径 ≤ 0 或法向量为零");
                return Circle.Unset;
            }

            Plane plane = new Plane(center, normal);
            Circle circle = CircleGeo.CreateFromCenterRadius(plane, center, radius);

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 3：平面 + 半径，原点为圆心）。
        /// 调用 Geometry 层 CircleGeo.CreateFromCenterRadius。
        /// </summary>
        public static Circle CreateCircle(Plane plane, double radius, bool isPreview = false)
        {
            if (radius <= 0)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：半径 ≤ 0");
                return Circle.Unset;
            }

            Circle circle = CircleGeo.CreateFromCenterRadius(plane, plane.Origin, radius);

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 4：平面 + 直径两端点）。
        /// 调用 Geometry 层 CircleGeo.CreateFromDiameter。
        /// </summary>
        public static Circle CreateCircle(Plane plane, Point3d point1, Point3d point2, bool isPreview = false)
        {
            if (point1 == point2)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：直径两端点重合");
                return Circle.Unset;
            }

            Circle circle = CircleGeo.CreateFromDiameter(plane, point1, point2);

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", circle.Radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 5：三点，平面自动确定）。
        /// 调用 Geometry 层 CircleGeo.CreateFrom3Points。
        /// </summary>
        public static Circle CreateCircle(Point3d p1, Point3d p2, Point3d p3, bool isPreview = false)
        {
            Circle circle = CircleGeo.CreateFrom3Points(p1, p2, p3);

            if (!circle.IsValid)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：三点无法确定有效圆");
                return Circle.Unset;
            }

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", circle.Radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 6：起点 + 切向 + 终点，平面自动确定）。
        /// 调用 Geometry 层 CircleGeo.CreateFromTangent。
        /// </summary>
        public static Circle CreateCircle(Point3d startPoint, Vector3d tangentAtStart, Point3d endPoint, bool isPreview = false)
        {
            if (tangentAtStart.IsZero || startPoint == endPoint)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：切向量为零或起点与终点重合");
                return Circle.Unset;
            }

            Circle circle = CircleGeo.CreateFromTangent(startPoint, tangentAtStart, endPoint);

            if (!circle.IsValid)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：起点切向终点无法构造有效圆");
                return Circle.Unset;
            }

            if (!isPreview)
                UpdateDefault("CreateCircle.radius", circle.Radius);

            return circle;
        }

        /// <summary>
        /// 创建圆（重载 7：与两条曲线相切的圆角圆）。
        /// RhinoCommon：Curve.CreateFillet(curve1, curve2, radius, t0, t1) 返回 Arc，
        /// 由 Arc 可获得对应的圆。
        /// </summary>
        public static Circle[] CreateCircle(Curve curve1, Curve curve2, double radius,
            double tolerance, bool isPreview = false)
        {
            if (curve1 == null || curve2 == null || radius <= 0)
            {
                RhinoApp.WriteLine("[CreateCircle] 错误：曲线为空或半径 ≤ 0");
                return new Circle[0];
            }

            // 在曲线中点附近搜索相切圆角
            double t0 = curve1.Domain.Mid;
            double t1 = curve2.Domain.Mid;

            Arc filletArc = Curve.CreateFillet(curve1, curve2, radius, t0, t1);

            if (filletArc.IsValid)
                return new Circle[] { new Circle(filletArc.Plane, filletArc.Center, filletArc.Radius) };

            RhinoApp.WriteLine("[CreateCircle] 错误：未能生成相切圆角圆");
            return new Circle[0];
        }

        // ================================================================
        // 圆弧
        // ================================================================

        /// <summary>
        /// 创建圆弧（重载 1：圆心 + 半径 + 角度）。
        /// 调用 Geometry 层 ArcGeo.CreateFromCenterAngle。
        /// </summary>
        public static Arc CreateArc(Plane plane, Point3d center, double radius,
            double startAngle, double endAngle, bool isPreview = false)
        {
            if (radius <= 0 || endAngle <= startAngle)
            {
                RhinoApp.WriteLine("[CreateArc] 错误：半径 ≤ 0 或终止角度不大于起始角度");
                return Arc.Unset;
            }

            Arc arc = ArcGeo.CreateFromCenterAngle(plane, center, radius, startAngle, endAngle);

            if (!isPreview)
            {
                UpdateDefault("CreateArc.startAngle", startAngle);
                UpdateDefault("CreateArc.endAngle", endAngle);
            }

            return arc;
        }

        /// <summary>
        /// 创建圆弧（重载 2：三点，平面自动确定）。
        /// 调用 Geometry 层 ArcGeo.CreateFrom3Points。
        /// </summary>
        public static Arc CreateArc(Point3d start, Point3d pointOnArc, Point3d end, bool isPreview = false)
        {
            Arc arc = ArcGeo.CreateFrom3Points(start, pointOnArc, end);

            if (!arc.IsValid)
            {
                RhinoApp.WriteLine("[CreateArc] 错误：三点无法确定有效圆弧");
                return Arc.Unset;
            }

            return arc;
        }

        /// <summary>
        /// 创建圆弧（重载 3：起点 + 终点 + 起点方向，平面自动确定）。
        /// 调用 Geometry 层 ArcGeo.CreateFromStartEndDir。
        /// </summary>
        public static Arc CreateArc(Point3d start, Point3d end, Vector3d directionAtStart, bool isPreview = false)
        {
            if (directionAtStart.IsZero || start == end)
            {
                RhinoApp.WriteLine("[CreateArc] 错误：方向向量为零或起点与终点重合");
                return Arc.Unset;
            }

            Arc arc = ArcGeo.CreateFromStartEndDir(start, end, directionAtStart);

            if (!arc.IsValid)
            {
                RhinoApp.WriteLine("[CreateArc] 错误：起点终点方向无法构造有效圆弧");
                return Arc.Unset;
            }

            return arc;
        }

        /// <summary>
        /// 创建圆弧（重载 4：与两条曲线相切的圆角弧）。
        /// RhinoCommon：Curve.CreateFillet(curve1, curve2, radius, t0, t1)
        /// </summary>
        public static Arc[] CreateArc(Curve curve1, Curve curve2, double radius,
            double tolerance, bool isPreview = false)
        {
            if (curve1 == null || curve2 == null || radius <= 0)
            {
                RhinoApp.WriteLine("[CreateArc] 错误：曲线为空或半径 ≤ 0");
                return new Arc[0];
            }

            double t0 = curve1.Domain.Mid;
            double t1 = curve2.Domain.Mid;

            Arc filletArc = Curve.CreateFillet(curve1, curve2, radius, t0, t1);

            if (filletArc.IsValid)
                return new Arc[] { filletArc };

            RhinoApp.WriteLine("[CreateArc] 错误：未能生成相切圆角弧");
            return new Arc[0];
        }

        // ================================================================
        // 线
        // ================================================================

        /// <summary>
        /// 创建直线（重载 1：两点）。
        /// RhinoCommon：new Line(start, end)
        /// </summary>
        public static Line CreateLine(Point3d start, Point3d end, bool isPreview = false)
        {
            if (start == end)
            {
                RhinoApp.WriteLine("[CreateLine] 错误：起点与终点重合");
                return Line.Unset;
            }

            return new Line(start, end);
        }

        /// <summary>
        /// 创建直线（重载 2：多点拟合）。
        /// RhinoCommon：Line.TryFitLineToPoints(points, out line)
        /// </summary>
        public static Line CreateLine(IEnumerable<Point3d> points, bool isPreview = false)
        {
            if (points == null)
            {
                RhinoApp.WriteLine("[CreateLine] 错误：点集合为空");
                return Line.Unset;
            }

            Line line;
            bool success = Line.TryFitLineToPoints(points, out line);

            if (!success)
            {
                RhinoApp.WriteLine("[CreateLine] 错误：无法将点拟合为直线");
                return Line.Unset;
            }

            return line;
        }

        // ================================================================
        // 多段线
        // ================================================================

        /// <summary>
        /// 创建多段线。
        /// RhinoCommon：new Polyline(points)
        /// </summary>
        public static Polyline CreatePolyline(IEnumerable<Point3d> points, bool closed = false, bool isPreview = false)
        {
            if (points == null)
            {
                RhinoApp.WriteLine("[CreatePolyline] 错误：点集合为空");
                return null;
            }

            var pointList = new List<Point3d>(points);

            if (pointList.Count < 2)
            {
                RhinoApp.WriteLine("[CreatePolyline] 错误：点数少于 2");
                return null;
            }

            var poly = new Polyline(pointList);

            if (closed)
                poly.Add(pointList[0]);

            if (!isPreview)
                UpdateDefault("CreatePolyline.closed", closed);

            return poly;
        }

        // ================================================================
        // 矩形
        // ================================================================

        /// <summary>
        /// 创建矩形（重载 1：对角点）。
        /// 调用 Geometry 层 RectangleGeo.CreateFromCorners。
        /// </summary>
        public static Polyline CreateRectangle(Plane plane, Point3d corner1, Point3d corner2, bool isPreview = false)
        {
            return RectangleGeo.CreateFromCorners(plane, corner1, corner2);
        }

        /// <summary>
        /// 创建矩形（重载 2：中心 + 宽高）。
        /// 调用 Geometry 层 RectangleGeo.CreateFromCenter。
        /// </summary>
        public static Polyline CreateRectangle(Plane plane, Point3d center, double width, double height, bool isPreview = false)
        {
            if (width <= 0 || height <= 0)
            {
                RhinoApp.WriteLine("[CreateRectangle] 错误：宽度或高度 ≤ 0");
                return null;
            }

            return RectangleGeo.CreateFromCenter(plane, center, width, height);
        }

        // ================================================================
        // 多边形
        // ================================================================

        /// <summary>
        /// 创建正多边形（重载 1：外接圆方式）。
        /// 调用 Geometry 层 PolygonGeo.CreateRegularPolygon。
        /// </summary>
        public static Polyline CreatePolygon(Plane plane, Point3d center, int sides, double radius, bool isPreview = false)
        {
            return PolygonGeo.CreateRegularPolygon(plane, center, sides, radius);
        }

        /// <summary>
        /// 创建正多边形（重载 2：边长方式）。
        /// 调用 Geometry 层 PolygonGeo.EdgeToCenter 计算中心后生成。
        /// </summary>
        public static Polyline CreatePolygon(Plane plane, Point3d start, Point3d end, int sides, bool isPreview = false)
        {
            if (sides < 3 || start == end)
            {
                RhinoApp.WriteLine("[CreatePolygon] 错误：边数 < 3 或起点与终点重合");
                return null;
            }

            Point3d center;
            double radius, startAngle;
            PolygonGeo.EdgeToCenter(plane, start, end, sides, out center, out radius, out startAngle);

            return PolygonGeo.CreateRegularPolygon(plane, center, sides, radius, startAngle);
        }

        /// <summary>
        /// 创建星形多边形（重载 3：内外半径）。
        /// 调用 Geometry 层 PolygonGeo.CreateStarPolygon。
        /// </summary>
        public static Polyline CreatePolygon(Plane plane, Point3d center, int sides,
            double outerRadius, double innerRadius, bool isPreview = false)
        {
            return PolygonGeo.CreateStarPolygon(plane, center, sides, outerRadius, innerRadius);
        }

        // ================================================================
        // 椭圆
        // ================================================================

        /// <summary>
        /// 创建椭圆（重载 1：中心 + 两半轴）。
        /// 调用 Geometry 层 EllipseGeo.CreateFromCenterRadii。
        /// </summary>
        public static NurbsCurve CreateEllipse(Plane plane, Point3d center,
            double radius1, double radius2, bool isPreview = false)
        {
            if (radius1 <= 0 || radius2 <= 0)
            {
                RhinoApp.WriteLine("[CreateEllipse] 错误：半轴长度 ≤ 0");
                return null;
            }

            return EllipseGeo.CreateFromCenterRadii(plane, center, radius1, radius2);
        }

        /// <summary>
        /// 创建椭圆（重载 2：直径两端点 + 第二轴）。
        /// 调用 Geometry 层 EllipseGeo.CreateFromDiameter。
        /// </summary>
        public static NurbsCurve CreateEllipse(Plane plane, Point3d point1, Point3d point2,
            double secondRadius, bool isPreview = false)
        {
            if (point1 == point2 || secondRadius <= 0)
            {
                RhinoApp.WriteLine("[CreateEllipse] 错误：直径两端点重合或第二半径 ≤ 0");
                return null;
            }

            return EllipseGeo.CreateFromDiameter(plane, point1, point2, secondRadius);
        }

        /// <summary>
        /// 创建椭圆（重载 3：两焦点 + 椭圆上一点，平面自动确定）。
        /// 调用 Geometry 层 EllipseGeo.CreateFromFoci。
        /// </summary>
        public static NurbsCurve CreateEllipse(Point3d focus1, Point3d focus2,
            Point3d pointOnEllipse, bool isPreview = false)
        {
            if (focus1 == focus2 || !pointOnEllipse.IsValid)
            {
                RhinoApp.WriteLine("[CreateEllipse] 错误：两焦点重合或椭圆上点无效");
                return null;
            }

            return EllipseGeo.CreateFromFoci(focus1, focus2, pointOnEllipse);
        }

        // ================================================================
        // 圆锥曲线（抛物线、双曲线、Conic）
        // ================================================================

        /// <summary>
        /// 创建抛物线（三点在抛物线上）。
        /// RhinoCommon：NurbsCurve.CreateParabolaFromPoints(start, innerPoint, end)
        /// 注：焦点方式（CreateParabolaFromFocus）签名同为 (Point3d×3)，无法重载区分，
        /// 如需焦点方式请直接调用 NurbsCurve.CreateParabolaFromFocus。
        /// </summary>
        public static NurbsCurve CreateParabola(Point3d start, Point3d pointOn, Point3d end, bool isPreview = false)
        {
            return NurbsCurve.CreateParabolaFromPoints(start, pointOn, end);
        }

        /// <summary>
        /// 创建双曲线。
        /// RhinoCommon 无直接 API，调用 Geometry 层 ConicGeo.CreateHyperbola。
        /// </summary>
        public static NurbsCurve CreateHyperbola(Point3d focus, Point3d vertex,
            Point3d endPoint, bool isPreview = false)
        {
            if (focus == vertex)
            {
                RhinoApp.WriteLine("[CreateHyperbola] 错误：焦点与顶点重合");
                return null;
            }

            return ConicGeo.CreateHyperbola(focus, vertex, endPoint);
        }

        /// <summary>
        /// 创建圆锥截面曲线。
        /// RhinoCommon 无直接 API，调用 Geometry 层 ConicGeo.CreateConic。
        /// rho < 0.5 椭圆，rho = 0.5 抛物线，rho > 0.5 双曲线。
        /// </summary>
        public static NurbsCurve CreateConic(Point3d start, Point3d end, Point3d apex,
            double rho, bool isPreview = false)
        {
            if (rho <= 0 || rho >= 1)
            {
                RhinoApp.WriteLine("[CreateConic] 错误：rho 不在 (0, 1) 范围内");
                return null;
            }

            if (!isPreview)
                UpdateDefault("CreateConic.rho", rho);

            return ConicGeo.CreateConic(start, end, apex, rho);
        }

        // ================================================================
        // 自由曲线
        // ================================================================

        /// <summary>
        /// 创建 NURBS 曲线（重载 1：控制点方式）。
        /// RhinoCommon：NurbsCurve.Create(periodic, degree, points)
        /// </summary>
        public static NurbsCurve CreateNurbsCurve(IEnumerable<Point3d> points, int degree = 3,
            bool periodic = false, bool isPreview = false)
        {
            if (points == null)
            {
                RhinoApp.WriteLine("[CreateNurbsCurve] 错误：点集合为空");
                return null;
            }

            var pointArray = new List<Point3d>(points).ToArray();

            if (pointArray.Length <= degree || degree < 1)
            {
                RhinoApp.WriteLine("[CreateNurbsCurve] 错误：控制点数不足或次数 < 1");
                return null;
            }

            NurbsCurve curve = NurbsCurve.Create(periodic, degree, pointArray);

            if (!isPreview)
            {
                UpdateDefault("CreateNurbsCurve.degree", degree);
                UpdateDefault("CreateNurbsCurve.periodic", periodic);
            }

            return curve;
        }

        /// <summary>
        /// 创建 NURBS 曲线（重载 2：高级方式，含节点向量和权重）。
        /// 手动构造 NurbsCurve 并设置节点和权重。
        /// </summary>
        public static NurbsCurve CreateNurbsCurve(IEnumerable<Point3d> points,
            IEnumerable<double> knots, int degree = 3,
            IEnumerable<double> weights = null, bool isPreview = false)
        {
            if (points == null || knots == null)
            {
                RhinoApp.WriteLine("[CreateNurbsCurve] 错误：点集合或节点向量为空");
                return null;
            }

            var pointList = new List<Point3d>(points);
            var knotList = new List<double>(knots);

            if (pointList.Count < 2 || degree < 1)
            {
                RhinoApp.WriteLine("[CreateNurbsCurve] 错误：点数少于 2 或次数 < 1");
                return null;
            }

            NurbsCurve curve = NurbsCurve.Create(false, degree, pointList.ToArray());

            if (curve == null)
            {
                RhinoApp.WriteLine("[CreateNurbsCurve] 错误：NURBS 曲线创建失败");
                return null;
            }

            if (knotList.Count == curve.Knots.Count)
            {
                for (int i = 0; i < knotList.Count; i++)
                    curve.Knots[i] = knotList[i];
            }

            if (weights != null)
            {
                var weightList = new List<double>(weights);
                if (weightList.Count == curve.Points.Count)
                {
                    for (int i = 0; i < weightList.Count; i++)
                    {
                        if (weightList[i] > 0)
                        {
                            Point3d loc = curve.Points[i].Location;
                            curve.Points.SetPoint(i, loc, weightList[i]);
                        }
                    }
                }
            }

            return curve;
        }

        /// <summary>
        /// 创建插值曲线（通过所有给定点）。
        /// RhinoCommon：Curve.CreateInterpolatedCurve(points, degree, knots)
        /// </summary>
        public static NurbsCurve CreateInterpCrv(IEnumerable<Point3d> points, int degree = 3,
            CurveKnotStyle knots = CurveKnotStyle.Uniform,
            Vector3d startTangent = new Vector3d(), Vector3d endTangent = new Vector3d(),
            bool isPreview = false)
        {
            if (points == null)
            {
                RhinoApp.WriteLine("[CreateInterpCrv] 错误：点集合为空");
                return null;
            }

            var pointList = new List<Point3d>(points);

            if (pointList.Count < 2)
            {
                RhinoApp.WriteLine("[CreateInterpCrv] 错误：点数少于 2");
                return null;
            }

            if (degree % 2 == 0)
                degree += 1;

            NurbsCurve curve;

            if (startTangent.IsValid || endTangent.IsValid)
            {
                curve = Curve.CreateInterpolatedCurve(pointList, degree, knots, startTangent, endTangent).ToNurbsCurve();
            }
            else
            {
                curve = Curve.CreateInterpolatedCurve(pointList, degree, knots).ToNurbsCurve();
            }

            if (!isPreview)
                UpdateDefault("CreateInterpCrv.degree", degree);

            return curve;
        }

        /// <summary>
        /// 创建手柄曲线（链式三次贝塞尔）。
        /// RhinoCommon 无直接 API，用 PolyCurve 拼接 BezierCurve 段。
        /// </summary>
        public static NurbsCurve CreateHandleCurve(IEnumerable<System.Tuple<Point3d, Point3d>> handlePoints,
            bool closed = false, bool isPreview = false)
        {
            if (handlePoints == null)
            {
                RhinoApp.WriteLine("[CreateHandleCurve] 错误：控制柄点集合为空");
                return null;
            }

            var handleList = new List<System.Tuple<Point3d, Point3d>>(handlePoints);

            if (handleList.Count < 2)
            {
                RhinoApp.WriteLine("[CreateHandleCurve] 错误：控制柄点数少于 2");
                return null;
            }

            // 每段三次贝塞尔：P0=anchor[i], P1=handle[i], P2=-handle[i+1]+anchor[i+1], P3=anchor[i+1]
            var poly = new PolyCurve();

            for (int i = 0; i < handleList.Count - 1; i++)
            {
                Point3d p0 = handleList[i].Item1;
                Point3d p1 = handleList[i].Item2;
                Point3d p3 = handleList[i + 1].Item1;
                // 对称控制柄
                Point3d p2 = p3 + (p3 - handleList[i + 1].Item2);

                var bez = new BezierCurve(new Point3d[] { p0, p1, p2, p3 });
                poly.Append(bez.ToNurbsCurve());
            }

            if (closed)
            {
                Point3d p0 = handleList[handleList.Count - 1].Item1;
                Point3d p1 = handleList[handleList.Count - 1].Item2;
                Point3d p3 = handleList[0].Item1;
                Point3d p2 = p3 + (p3 - handleList[0].Item2);

                var bez = new BezierCurve(new Point3d[] { p0, p1, p2, p3 });
                poly.Append(bez.ToNurbsCurve());
            }

            if (!isPreview)
                UpdateDefault("CreateHandleCurve.closed", closed);

            return poly.ToNurbsCurve();
        }

        /// <summary>
        /// 创建通过点对象/点云的拟合曲线。
        /// RhinoCommon：Curve.CreateInterpolatedCurve
        /// </summary>
        public static NurbsCurve CreateCurveThroughPt(IEnumerable<Point3d> points, int degree = 3,
            bool periodic = false, double tolerance = 0.0,
            Vector3d startTangent = new Vector3d(), Vector3d endTangent = new Vector3d(),
            bool isPreview = false)
        {
            if (points == null)
            {
                RhinoApp.WriteLine("[CreateCurveThroughPt] 错误：点集合为空");
                return null;
            }

            var pointList = new List<Point3d>(points);

            if (pointList.Count < degree + 1)
            {
                RhinoApp.WriteLine("[CreateCurveThroughPt] 错误：点数少于 degree + 1");
                return null;
            }

            if (tolerance <= 0)
                tolerance = ActiveTolerance();

            NurbsCurve curve = Curve.CreateInterpolatedCurve(pointList, degree, CurveKnotStyle.Uniform).ToNurbsCurve();

            if (!isPreview)
            {
                UpdateDefault("CreateCurveThroughPt.degree", degree);
                UpdateDefault("CreateCurveThroughPt.periodic", periodic);
            }

            return curve;
        }

        /// <summary>
        /// 创建悬链线曲线。
        /// 调用 Geometry 层 CatenaryGeo.Create，重力方向由调用方传入。
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="end">终点</param>
        /// <param name="length">曲线总长度（>两点距离）</param>
        /// <param name="gravity">重力方向（默认 -Z）</param>
        public static NurbsCurve CreateCatenary(Point3d start, Point3d end, double length,
            Vector3d gravity, bool isPreview = false)
        {
            if (start == end || gravity.IsZero)
            {
                RhinoApp.WriteLine("[CreateCatenary] 错误：起点与终点重合或重力方向为零");
                return null;
            }

            return CatenaryGeo.Create(start, end, length, gravity);
        }

        // ================================================================
        // 螺旋线
        // ================================================================

        /// <summary>
        /// 创建螺旋线（重载 1：沿轴线）。
        /// RhinoCommon：NurbsCurve.CreateSpiral(axisStart, axisDir, radiusPoint, pitch, turns, r0, r1)
        /// </summary>
        public static NurbsCurve CreateHelix(Line axis, double startRadius, double endRadius,
            double turns, double pitch, bool isPreview = false)
        {
            if (turns <= 0 || startRadius <= 0 || endRadius <= 0)
            {
                RhinoApp.WriteLine("[CreateHelix] 错误：圈数 ≤ 0 或半径 ≤ 0");
                return null;
            }

            Point3d axisStart = axis.From;
            Vector3d axisDir = axis.Direction;
            axisDir.Unitize();

            // 调用 Geometry 层计算垂直参考点
            Point3d radiusPoint = HelixGeo.GetRadiusPoint(axisStart, axisDir, startRadius);

            NurbsCurve curve = NurbsCurve.CreateSpiral(
                axisStart, axisDir, radiusPoint, pitch, turns, startRadius, endRadius);

            if (!isPreview)
            {
                UpdateDefault("CreateHelix.startRadius", startRadius);
                UpdateDefault("CreateHelix.endRadius", endRadius);
                UpdateDefault("CreateHelix.pitch", pitch);
            }

            return curve;
        }

        /// <summary>
        /// 创建螺旋线（重载 2：沿任意曲线）。
        /// RhinoCommon：NurbsCurve.CreateSpiral(rail, t0, t1, radiusPoint, pitch, turns, r0, r1, pointsPerTurn)
        /// </summary>
        public static NurbsCurve CreateHelix(Curve rail, double startRadius, double endRadius,
            double turns, bool isPreview = false)
        {
            if (rail == null || turns <= 0 || startRadius <= 0 || endRadius <= 0)
            {
                RhinoApp.WriteLine("[CreateHelix] 错误：路径曲线为空、圈数 ≤ 0 或半径 ≤ 0");
                return null;
            }

            double t0 = rail.Domain.Min;
            double t1 = rail.Domain.Max;

            Vector3d railDir = rail.TangentAt(t0);

            Point3d radiusPoint = HelixGeo.GetRadiusPoint(rail.PointAt(t0), railDir, startRadius);

            NurbsCurve curve = NurbsCurve.CreateSpiral(
                rail, t0, t1, radiusPoint, 0.0, turns, startRadius, endRadius, 12);

            if (!isPreview)
            {
                UpdateDefault("CreateHelix.startRadius", startRadius);
                UpdateDefault("CreateHelix.endRadius", endRadius);
            }

            return curve;
        }

        /// <summary>
        /// 创建平面螺旋线。
        /// RhinoCommon：NurbsCurve.CreateSpiral(center, normal, radiusPoint, pitch=0, turns, r0, r1)
        /// </summary>
        public static NurbsCurve CreateSpiral(Plane plane, Point3d center,
            double startRadius, double endRadius, double turns, bool isPreview = false)
        {
            if (turns <= 0 || startRadius <= 0 || endRadius <= 0)
            {
                RhinoApp.WriteLine("[CreateSpiral] 错误：圈数 ≤ 0 或半径 ≤ 0");
                return null;
            }

            Point3d radiusPoint = center + plane.XAxis * startRadius;

            NurbsCurve curve = NurbsCurve.CreateSpiral(
                center, plane.Normal, radiusPoint, 0.0, turns, startRadius, endRadius);

            if (!isPreview)
            {
                UpdateDefault("CreateSpiral.startRadius", startRadius);
                UpdateDefault("CreateSpiral.endRadius", endRadius);
            }

            return curve;
        }

        // ================================================================
        // 从对象提取
        // ================================================================

        /// <summary>
        /// 沿曲线等分生成点（重载 1：等分段数）。
        /// RhinoCommon：curve.DivideByCount(segmentCount, true)
        /// </summary>
        public static List<Point3d> CreateDividePoints(Curve curve, int segmentCount, bool isPreview = false)
        {
            var result = new List<Point3d>();

            if (curve == null || segmentCount < 1)
            {
                RhinoApp.WriteLine("[CreateDividePoints] 错误：曲线为空或段数 < 1");
                return result;
            }

            Point3d[] points;
            curve.DivideByCount(segmentCount, true, out points);

            if (points != null)
            {
                result.AddRange(points);

                if (!isPreview)
                    UpdateDefault("CreateDividePoints.segmentCount", segmentCount);
            }

            return result;
        }

        /// <summary>
        /// 沿曲线等分生成点（重载 2：按长度）。
        /// RhinoCommon：curve.DivideByLength(segmentLength, true)
        /// </summary>
        public static List<Point3d> CreateDividePoints(Curve curve, double segmentLength, bool isPreview = false)
        {
            var result = new List<Point3d>();

            if (curve == null || segmentLength <= 0)
            {
                RhinoApp.WriteLine("[CreateDividePoints] 错误：曲线为空或段长度 ≤ 0");
                return result;
            }

            Point3d[] points;
            curve.DivideByLength(segmentLength, true, out points);

            if (points != null)
                result.AddRange(points);

            return result;
        }

        /// <summary>
        /// 获取曲线起点。
        /// RhinoCommon：curve.PointAtStart
        /// </summary>
        public static Point3d GetCurveStart(Curve curve)
        {
            if (curve == null)
                return Point3d.Unset;

            return curve.PointAtStart;
        }

        /// <summary>
        /// 获取曲线终点。
        /// RhinoCommon：curve.PointAtEnd
        /// </summary>
        public static Point3d GetCurveEnd(Curve curve)
        {
            if (curve == null)
                return Point3d.Unset;

            return curve.PointAtEnd;
        }

        // ================================================================
        // 从对象派生曲线
        // ================================================================

        /// <summary>
        /// 将曲线沿方向投影到曲面。
        /// RhinoCommon：Curve.ProjectToBrep(curve, brep, direction, tolerance)
        /// </summary>
        public static Curve[] CreateProjectCrv(IEnumerable<Curve> curves, Brep target,
            Vector3d direction, bool isPreview = false)
        {
            if (curves == null || target == null)
            {
                RhinoApp.WriteLine("[CreateProjectCrv] 错误：曲线集合或目标曲面为空");
                return new Curve[0];
            }

            double tolerance = ActiveTolerance();
            var result = new List<Curve>();

            foreach (Curve crv in curves)
            {
                Curve[] projected = Curve.ProjectToBrep(crv, target, direction, tolerance);
                if (projected != null)
                    result.AddRange(projected);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将曲线拉到曲面上（最近点方式）。
        /// RhinoCommon：Curve.PullToBrepFace(face, tolerance)
        /// </summary>
        public static Curve[] CreatePullCrv(IEnumerable<Curve> curves, Brep target,
            double tolerance, bool isPreview = false)
        {
            if (curves == null || target == null || target.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[CreatePullCrv] 错误：曲线集合为空、目标曲面为空或无面");
                return new Curve[0];
            }

            if (tolerance <= 0)
                tolerance = ActiveTolerance();

            var result = new List<Curve>();

            foreach (Curve crv in curves)
            {
                foreach (BrepFace face in target.Faces)
                {
                    Curve[] pulled = crv.PullToBrepFace(face, tolerance);
                    if (pulled != null)
                        result.AddRange(pulled);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 将曲线包裹映射到曲面上（保持 UV 关系）。
        /// RhinoCommon：Surface.Pushup(2D curve, tolerance) — 需先将 3D 曲线转为 UV 参数曲线。
        /// 实现方式：先 Pullback 获取 2D 曲线，再 Pushup 映射回 3D 曲面曲线。
        /// </summary>
        public static Curve[] CreateApplyCrv(IEnumerable<Curve> curves, Brep target, bool isPreview = false)
        {
            if (curves == null || target == null || target.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[CreateApplyCrv] 错误：曲线集合为空、目标曲面为空或无面");
                return new Curve[0];
            }

            double tolerance = ActiveTolerance();
            var result = new List<Curve>();

            foreach (Curve crv in curves)
            {
                BrepFace face = target.Faces[0];

                // 将 3D 曲线拉到 UV 参数空间
                Curve curve2d = face.Pullback(crv, tolerance);

                if (curve2d != null)
                {
                    // 再 Pushup 回 3D 曲面
                    Curve applied = face.Pushup(curve2d, tolerance);
                    if (applied != null)
                        result.Add(applied);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 复制曲面/多重曲面的边缘为独立曲线。
        /// 重载 1：指定边缘 — 复制指定的边（不限于裸露边）。
        /// 重载 2：全部裸露边 — 自动提取所有裸露边缘。
        /// </summary>
        public static Curve[] CreateDupEdge(Brep brep, IEnumerable<BrepEdge> edges, bool isPreview = false)
        {
            if (brep == null || edges == null)
            {
                RhinoApp.WriteLine("[CreateDupEdge] 错误：曲面或边集合为空");
                return new Curve[0];
            }

            var result = new List<Curve>();
            foreach (BrepEdge edge in edges)
            {
                Curve dup = edge.DuplicateCurve();
                if (dup != null)
                    result.Add(dup);
            }

            return result.ToArray();
        }

        public static Curve[] CreateDupEdge(Brep brep, bool isPreview = false)
        {
            if (brep == null)
            {
                RhinoApp.WriteLine("[CreateDupEdge] 错误：曲面为空");
                return new Curve[0];
            }

            var result = new List<Curve>();

            foreach (BrepEdge edge in brep.Edges)
            {
                if (edge.Valence == EdgeAdjacency.Naked)
                {
                    Curve dup = edge.DuplicateCurve();
                    if (dup != null)
                        result.Add(dup);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 从曲面提取等参线。
        /// RhinoCommon：face.IsoCurve(direction, parameter)
        /// </summary>
        public static Curve CreateExtractIsocurve(Brep brep, Point3d point, int direction, bool isPreview = false)
        {
            if (brep == null || brep.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[CreateExtractIsocurve] 错误：曲面为空或无面");
                return null;
            }

            BrepFace face = brep.Faces[0];
            double u, v;
            brep.Surfaces[0].ClosestPoint(point, out u, out v);

            return face.IsoCurve(direction, direction == 0 ? u : v);
        }

        /// <summary>
        /// 在曲面/网格上生成等高线。
        /// RhinoCommon：Brep.CreateContourCurves / Mesh.CreateContourCurves
        /// </summary>
        public static Curve[] CreateContour(GeometryBase geometry, Point3d startPt, Point3d endPt,
            double interval, bool isPreview = false)
        {
            if (geometry == null || interval <= 0)
            {
                RhinoApp.WriteLine("[CreateContour] 错误：几何对象为空或间距 ≤ 0");
                return new Curve[0];
            }

            Brep brep = geometry as Brep;
            if (brep != null)
                return Brep.CreateContourCurves(brep, startPt, endPt, interval);

            Mesh mesh = geometry as Mesh;
            if (mesh != null)
                return Mesh.CreateContourCurves(mesh, startPt, endPt, interval);

            RhinoApp.WriteLine("[CreateContour] 错误：几何对象既非 Brep 也非 Mesh");
            return new Curve[0];
        }

        /// <summary>
        /// 用平面切割对象生成截面线。
        /// RhinoCommon：Intersection.BrepPlane(brep, plane, tolerance)
        /// </summary>
        public static Curve[] CreateSection(GeometryBase geometry, Plane cutPlane, bool isPreview = false)
        {
            if (geometry == null)
            {
                RhinoApp.WriteLine("[CreateSection] 错误：几何对象为空");
                return new Curve[0];
            }

            Brep brep = geometry as Brep;
            if (brep != null)
            {
                Curve[] intersectionCurves;
                Point3d[] intersectionPoints;
                Rhino.Geometry.Intersect.Intersection.BrepPlane(
                    brep, cutPlane, ActiveTolerance(),
                    out intersectionCurves, out intersectionPoints);

                return intersectionCurves ?? new Curve[0];
            }

            RhinoApp.WriteLine("[CreateSection] 错误：几何对象非 Brep，无法生成截面");
            return new Curve[0];
        }
    }
}
