using System.Collections.Generic;
using Rhino;
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
            var brep = SolidGeo.CreateSphere(center, normal, radius);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateSphere] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateEllipsoid(Point3d center, Vector3d normal, Vector3d radii, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateEllipsoid.radiiX", radii.X);
                UpdateDefault("CreateEllipsoid.radiiY", radii.Y);
                UpdateDefault("CreateEllipsoid.radiiZ", radii.Z);
            }
            var brep = SolidGeo.CreateEllipsoid(center, normal, radii);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateEllipsoid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateTorus(Point3d center, Vector3d normal, double majorRadius, double minorRadius,
            bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateTorus.majorRadius", majorRadius);
                UpdateDefault("CreateTorus.minorRadius", minorRadius);
            }
            var brep = SolidGeo.CreateTorus(center, normal, majorRadius, minorRadius);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateTorus] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        // ================================================================
        // 二、多曲面实体
        // ================================================================

        public static Brep CreateBox(Box box, bool isPreview = false)
        {
            var brep = SolidGeo.CreateFromBox(box);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateBox] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateBox(Point3d corner1, Point3d corner2, Vector3d normal, bool isPreview = false)
        {
            var brep = SolidGeo.CreateFromCorners(corner1, corner2, normal);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateBox] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateCylinder(baseCenter, normal, radius, height, capEnds);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateCylinder] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateCone(baseCenter, normal, bottomRadius, height, capEnd);
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateCone] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateTruncatedCone(baseCenter, normal, bottomRadius, topRadius, height, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateTruncatedCone] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateTube(baseCenter, normal, innerRadius, outerRadius, height, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateTube] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreatePyramid(baseCenter, normal, sides, radius, height, capBase, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreatePyramid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateTruncatedPyramid(baseCenter, normal, sides,
                bottomRadius, topRadius, height, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateTruncatedPyramid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        // ================================================================
        // 三、从曲线挤出为实体
        // ================================================================

        public static Brep CreateExtrudeSolid(Curve profile, Vector3d direction, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            var brep = SolidGeo.CreateExtrudeSolid(profile, direction, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateExtrudeSolid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateRevolveSolid(Curve profile, Line axis,
            double startAngle, double endAngle, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            var brep = SolidGeo.CreateRevolveSolid(profile, axis, startAngle, endAngle, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateRevolveSolid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateSweepSolid(Curve rail1, Curve rail2,
            IEnumerable<Curve> sections, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
                UpdateDefault("capEnds", capEnds);
            var brep = SolidGeo.CreateSweepSolid(rail1, rail2, sections, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateSweepSolid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateLoftSolid(IEnumerable<Curve> curves, int loftType, bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateLoftSolid.loftType", loftType);
                UpdateDefault("capEnds", capEnds);
            }
            var brep = SolidGeo.CreateLoftSolid(curves, loftType, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateLoftSolid] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreatePipe(rail, radius, mode, ActiveTolerance(), ActiveAngleTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreatePipe] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
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
            var brep = SolidGeo.CreateThickPipe(rail, innerRadius, outerRadius, mode,
                ActiveTolerance(), ActiveAngleTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreatePipe] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        public static Brep CreateSlab(PolylineCurve profile, double offsetDistance, Vector3d direction,
            bool capEnds, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateSlab.offsetDistance", offsetDistance);
                UpdateDefault("capEnds", capEnds);
            }
            var brep = SolidGeo.CreateSlab(profile, offsetDistance, direction, capEnds, ActiveTolerance());
            if (brep == null || !brep.IsSolid)
            {
                RhinoApp.WriteLine("[CreateSlab] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return brep;
        }

        /// <summary>TODO: CreateTextObject 需要 doc.Fonts 访问，暂未实现</summary>
        public static Brep[] CreateTextObject(string text, Plane plane, double textHeight,
            double solidThickness, string fontName, bool bold, bool italic, bool isPreview = false)
        {
            // 需要 ActiveDoc.Fonts.FindOrCreate，违反层级规则
            // 应在 Project 层获取 FontIndex 后传入
            RhinoApp.WriteLine("[CreateTextObject] 错误：方法暂未实现");
            return null;
        }

        public static Brep CreateThicken(Brep brep, double distance, bool bothSides, bool isPreview = false)
        {
            if (!isPreview)
            {
                UpdateDefault("CreateThicken.distance", distance);
                UpdateDefault("CreateThicken.bothSides", bothSides);
            }
            var result = SolidGeo.CreateThicken(brep, distance, bothSides, ActiveTolerance());
            if (result == null || !result.IsSolid)
            {
                RhinoApp.WriteLine("[CreateThicken] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return result;
        }

        // ================================================================
        // 五、辅助命令
        // ================================================================

        public static Brep CreateCap(Brep brep, bool isPreview = false)
        {
            var result = SolidGeo.CreateCap(brep, ActiveTolerance());
            if (result == null || !result.IsSolid)
            {
                RhinoApp.WriteLine("[CreateCap] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return result;
        }

        public static Brep CreateSolidFromBreps(IEnumerable<Brep> breps, bool isPreview = false)
        {
            var result = SolidGeo.CreateSolidFromBreps(breps, ActiveTolerance());
            if (result == null || !result.IsSolid)
            {
                RhinoApp.WriteLine("[CreateSolidFromBreps] 错误：Geometry 层返回 null 或非实体");
                return null;
            }
            return result;
        }
    }
}
