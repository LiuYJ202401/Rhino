using System.Collections.Generic;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.Sld;

namespace Rh.Cmd
{
    /// <summary>
    /// 基础实体命令。
    /// 创建闭合实体对象（Brep，IsSolid=true）。
    /// 对应接口文档：Solid.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    ///   - 同一功能统一方法名，通过参数类型区分重载
    /// </summary>
    public static class SolidCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Solid.json";

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

        public static bool GetDefaultCapEnds()
        {
            return GetDefault("capEnds", true);
        }

        public static int GetDefaultPipeCapMode()
        {
            return GetDefault("pipeCapMode", 1);
        }

        // ================================================================
        // 一、单曲面实体
        // ================================================================

        public static Brep CreateSphere(Point3d center, Vector3d normal, double radius, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("CreateSphere.radius", radius);
            return SolidGeo.CreateSphere(center, normal, radius);
        }

        public static Brep CreateEllipsoid(Point3d center, Vector3d normal, Vector3d radii, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateEllipsoid.radiiX", radii.X);
                UpdateDefault("CreateEllipsoid.radiiY", radii.Y);
                UpdateDefault("CreateEllipsoid.radiiZ", radii.Z);
            }
            return SolidGeo.CreateEllipsoid(center, normal, radii);
        }

        public static Brep CreateTorus(Point3d center, Vector3d normal, double majorRadius, double minorRadius,
            bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateTorus.majorRadius", majorRadius);
                UpdateDefault("CreateTorus.minorRadius", minorRadius);
            }
            return SolidGeo.CreateTorus(center, normal, majorRadius, minorRadius);
        }

        // ================================================================
        // 二、多曲面实体
        // ================================================================

        public static Brep CreateBox(Box box, bool isPreview = false)
        {
            return SolidGeo.CreateFromBox(box);
        }

        public static Brep CreateBox(Point3d corner1, Point3d corner2, Vector3d normal, bool isPreview = false)
        {
            return SolidGeo.CreateFromCorners(corner1, corner2, normal);
        }

        public static Brep CreateCylinder(Point3d baseCenter, Vector3d normal, double radius, double height,
            bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateCylinder.radius", radius);
                UpdateDefault("CreateCylinder.height", height);
                UpdateDefault("capEnds", capEnds);
            }
            return SolidGeo.CreateCylinder(baseCenter, normal, radius, height, capEnds);
        }

        public static Brep CreateCone(Point3d baseCenter, Vector3d normal, double bottomRadius, double height,
            bool capEnd, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateCone.bottomRadius", bottomRadius);
                UpdateDefault("CreateCone.height", height);
                UpdateDefault("capEnds", capEnd);
            }
            return SolidGeo.CreateCone(baseCenter, normal, bottomRadius, height, capEnd);
        }

        public static Brep CreateTruncatedCone(Point3d baseCenter, Vector3d normal,
            double bottomRadius, double topRadius, double height, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateTruncatedCone.bottomRadius", bottomRadius);
                UpdateDefault("CreateTruncatedCone.topRadius", topRadius);
                UpdateDefault("CreateTruncatedCone.height", height);
                UpdateDefault("capEnds", capEnds);
            }
            return SolidGeo.CreateTruncatedCone(baseCenter, normal, bottomRadius, topRadius, height, capEnds, ActiveTolerance());
        }

        public static Brep CreateTube(Point3d baseCenter, Vector3d normal,
            double innerRadius, double outerRadius, double height, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateTube.innerRadius", innerRadius);
                UpdateDefault("CreateTube.outerRadius", outerRadius);
                UpdateDefault("CreateTube.height", height);
                UpdateDefault("capEnds", capEnds);
            }
            return SolidGeo.CreateTube(baseCenter, normal, innerRadius, outerRadius, height, capEnds, ActiveTolerance());
        }

        public static Brep CreatePyramid(Point3d baseCenter, Vector3d normal, int sides,
            double radius, double height, bool capBase, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreatePyramid.sides", sides);
                UpdateDefault("CreatePyramid.radius", radius);
                UpdateDefault("CreatePyramid.height", height);
            }
            return SolidGeo.CreatePyramid(baseCenter, normal, sides, radius, height, capBase, ActiveTolerance());
        }

        public static Brep CreateTruncatedPyramid(Point3d baseCenter, Vector3d normal, int sides,
            double bottomRadius, double topRadius, double height, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateTruncatedPyramid.sides", sides);
                UpdateDefault("CreateTruncatedPyramid.bottomRadius", bottomRadius);
                UpdateDefault("CreateTruncatedPyramid.topRadius", topRadius);
                UpdateDefault("CreateTruncatedPyramid.height", height);
            }
            return SolidGeo.CreateTruncatedPyramid(baseCenter, normal, sides,
                bottomRadius, topRadius, height, capEnds, ActiveTolerance());
        }

        // ================================================================
        // 三、从曲线挤出为实体
        // ================================================================

        public static Brep CreateExtrudeSolid(Curve profile, Vector3d direction, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            return SolidGeo.CreateExtrudeSolid(profile, direction, capEnds, ActiveTolerance());
        }

        public static Brep CreateRevolveSolid(Curve profile, Line axis,
            double startAngle, double endAngle, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            return SolidGeo.CreateRevolveSolid(profile, axis, startAngle, endAngle, capEnds, ActiveTolerance());
        }

        public static Brep CreateSweepSolid(Curve rail1, Curve rail2,
            IEnumerable<Curve> sections, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            return SolidGeo.CreateSweepSolid(rail1, rail2, sections, capEnds, ActiveTolerance());
        }

        public static Brep CreateLoftSolid(IEnumerable<Curve> curves, int loftType, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateLoftSolid.loftType", loftType);
                UpdateDefault("capEnds", capEnds);
            }
            return SolidGeo.CreateLoftSolid(curves, loftType, capEnds, ActiveTolerance());
        }

        // ================================================================
        // 四、Solid 专属命令
        // ================================================================

        public static Brep CreatePipe(Curve rail, double radius, int capMode, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreatePipe.radius", radius);
                UpdateDefault("pipeCapMode", capMode);
            }
            var mode = (PipeCapMode)capMode;
            return SolidGeo.CreatePipe(rail, radius, mode, ActiveTolerance(), ActiveAngleTolerance());
        }

        public static Brep CreatePipe(Curve rail, double innerRadius, double outerRadius, int capMode,
            bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreatePipe.innerRadius", innerRadius);
                UpdateDefault("CreatePipe.outerRadius", outerRadius);
                UpdateDefault("pipeCapMode", capMode);
            }
            var mode = (PipeCapMode)capMode;
            return SolidGeo.CreateThickPipe(rail, innerRadius, outerRadius, mode,
                ActiveTolerance(), ActiveAngleTolerance());
        }

        public static Brep CreateSlab(PolylineCurve profile, double offsetDistance, Vector3d direction,
            bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateSlab.offsetDistance", offsetDistance);
                UpdateDefault("capEnds", capEnds);
            }
            return SolidGeo.CreateSlab(profile, offsetDistance, direction, capEnds, ActiveTolerance());
        }

        /// <summary>TODO: CreateTextObject 需要 doc.Fonts 访问，暂未实现</summary>
        public static Brep[] CreateTextObject(string text, Plane plane, double textHeight,
            double solidThickness, string fontName, bool bold, bool italic, bool isPreview = false)
        {
            // 需要 ActiveDoc.Fonts.FindOrCreate，违反层级规则
            // 应在 Project 层获取 FontIndex 后传入
            return null;
        }

        public static Brep CreateThicken(Brep brep, double distance, bool bothSides, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateThicken.distance", distance);
                UpdateDefault("CreateThicken.bothSides", bothSides);
            }
            return SolidGeo.CreateThicken(brep, distance, bothSides, ActiveTolerance());
        }

        // ================================================================
        // 五、辅助命令
        // ================================================================

        public static Brep CreateCap(Brep brep, bool isPreview = false)
        {
            return SolidGeo.CreateCap(brep, ActiveTolerance());
        }

        public static Brep CreateSolidFromBreps(IEnumerable<Brep> breps, bool isPreview = false)
        {
            return SolidGeo.CreateSolidFromBreps(breps, ActiveTolerance());
        }
    }
}
