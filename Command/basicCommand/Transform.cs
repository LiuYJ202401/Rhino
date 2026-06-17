using System;
using Rhino;
using Rhino.Geometry;
using Rh.Data;
using Rh.Geo.Trs;

namespace Rh.Cmd
{
    /// <summary>
    /// 基础变换命令。
    /// 对几何对象执行变换操作（不写入文档，返回变换后的几何对象）。
    /// 对应接口文档：Transform.md
    ///
    /// 规则：
    ///   - 不直接与 RhinoDoc 交互（不调用 doc.Objects.AddXxx）
    ///   - 不获取用户输入
    ///   - 是唯一直接调用 DataReader 的层
    ///   - 同一功能统一方法名，通过参数类型区分重载
    /// </summary>
    public static class TransformCmd
    {
        // ================================================================
        // 内部常量与工具方法
        // ================================================================

        private const string DataPath = "Command/basicCommand/Transform.json";

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

        public static int GetDefaultArrayCount()
        {
            return GetDefault("Array.count", 3);
        }

        public static double GetDefaultArraySpacing()
        {
            return GetDefault("Array.spacing", 1.0);
        }

        public static double GetDefaultArrayPolarAngle()
        {
            return GetDefault("ArrayPolar.totalAngle", Math.PI * 2);
        }

        public static bool GetDefaultArrayPolarRotate()
        {
            return GetDefault("ArrayPolar.rotate", true);
        }

        public static bool GetDefaultArrayOrient()
        {
            return GetDefault("ArrayAlongCrv.orient", false);
        }

        public static double GetDefaultScaleFactor()
        {
            return GetDefault("Scale.factor", 2.0);
        }

        // ================================================================
        // 一、基本变换
        // ================================================================

        /// <summary>平移对象</summary>
        public static GeometryBase Move(GeometryBase geometry, Vector3d translation)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Move] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.Move(geometry, translation);
        }

        /// <summary>复制对象并平移到新位置</summary>
        public static GeometryBase Copy(GeometryBase geometry, Vector3d translation)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Copy] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.Copy(geometry, translation);
        }

        /// <summary>绕 Z 轴旋转（工作平面旋转）</summary>
        public static GeometryBase Rotate(GeometryBase geometry, double angleRadians, Point3d center, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Rotate] 错误：geometry 为 null 或无效");
                return null;
            }

            if (!isPreview)
                UpdateDefault("Rotate.angle", angleRadians);

            return TransformGeo.Rotate(geometry, angleRadians, center);
        }

        /// <summary>绕任意轴旋转（3D 旋转）</summary>
        public static GeometryBase Rotate(GeometryBase geometry, double angleRadians, Vector3d axis, Point3d center, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Rotate] 错误：geometry 为 null 或无效");
                return null;
            }
            if (!axis.IsValid || axis.IsZero)
            {
                RhinoApp.WriteLine("[Rotate] 错误：旋转轴无效或为零向量");
                return null;
            }

            if (!isPreview)
                UpdateDefault("Rotate.angle", angleRadians);

            return TransformGeo.Rotate(geometry, angleRadians, axis, center);
        }

        /// <summary>均匀缩放</summary>
        public static GeometryBase Scale(GeometryBase geometry, Point3d anchor, double scaleFactor, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Scale] 错误：geometry 为 null 或无效");
                return null;
            }
            if (scaleFactor <= 0)
            {
                RhinoApp.WriteLine("[Scale] 错误：缩放系数必须大于 0");
                return null;
            }

            if (!isPreview)
                UpdateDefault("Scale.factor", scaleFactor);

            return TransformGeo.Scale(geometry, anchor, scaleFactor);
        }

        /// <summary>非均匀缩放（三轴独立）</summary>
        public static GeometryBase Scale(GeometryBase geometry, Plane plane, double xFactor, double yFactor, double zFactor, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Scale] 错误：geometry 为 null 或无效");
                return null;
            }
            if (xFactor <= 0 || yFactor <= 0 || zFactor <= 0)
            {
                RhinoApp.WriteLine("[Scale] 错误：缩放系数必须大于 0");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("Scale.xFactor", xFactor);
                UpdateDefault("Scale.yFactor", yFactor);
                UpdateDefault("Scale.zFactor", zFactor);
            }

            return TransformGeo.Scale(geometry, plane, xFactor, yFactor, zFactor);
        }

        /// <summary>以指定平面镜像</summary>
        public static GeometryBase Mirror(GeometryBase geometry, Plane mirrorPlane)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Mirror] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.Mirror(geometry, mirrorPlane);
        }

        /// <summary>以过指定点且法线为指定方向的平面镜像</summary>
        public static GeometryBase Mirror(GeometryBase geometry, Point3d pointOnPlane, Vector3d normal)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Mirror] 错误：geometry 为 null 或无效");
                return null;
            }
            if (!normal.IsValid || normal.IsZero)
            {
                RhinoApp.WriteLine("[Mirror] 错误：法线方向无效或为零向量");
                return null;
            }
            return TransformGeo.Mirror(geometry, pointOnPlane, normal);
        }

        /// <summary>剪切变形</summary>
        public static GeometryBase Shear(GeometryBase geometry, Plane plane, Vector3d x, Vector3d y, Vector3d z)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Shear] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.Shear(geometry, plane, x, y, z);
        }

        // ================================================================
        // 二、阵列
        // ================================================================

        /// <summary>沿直线方向均匀阵列（按间距）</summary>
        public static GeometryBase[] ArrayLinear(GeometryBase geometry, Vector3d direction, int count, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayLinear] 错误：geometry 为 null 或无效");
                return null;
            }
            if (count < 2 || !direction.IsValid || direction.IsZero)
            {
                RhinoApp.WriteLine("[ArrayLinear] 错误：数量小于 2 或方向无效");
                return null;
            }

            if (!isPreview)
                UpdateDefault("Array.count", count);

            return TransformGeo.ArrayLinear(geometry, direction, count);
        }

        /// <summary>沿直线方向均匀阵列（按总跨度：from→to 均匀分布）</summary>
        public static GeometryBase[] ArrayLinear(GeometryBase geometry, Point3d from, Point3d to, int count, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayLinear] 错误：geometry 为 null 或无效");
                return null;
            }
            if (count < 2)
            {
                RhinoApp.WriteLine("[ArrayLinear] 错误：数量小于 2");
                return null;
            }

            if (!isPreview)
                UpdateDefault("Array.count", count);

            return TransformGeo.ArrayLinear(geometry, from, to, count);
        }

        /// <summary>矩形阵列（X/Y/Z 三方向）</summary>
        public static GeometryBase[] ArrayRectangular(GeometryBase geometry, Plane plane,
            int xCount, int yCount, int zCount,
            double xSpacing, double ySpacing, double zSpacing,
            bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayRectangular] 错误：geometry 为 null 或无效");
                return null;
            }
            if (xCount < 1 || yCount < 1 || zCount < 1)
            {
                RhinoApp.WriteLine("[ArrayRectangular] 错误：各方向数量必须大于 0");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("ArrayRectangular.xCount", xCount);
                UpdateDefault("ArrayRectangular.yCount", yCount);
                UpdateDefault("ArrayRectangular.zCount", zCount);
                UpdateDefault("ArrayRectangular.xSpacing", xSpacing);
                UpdateDefault("ArrayRectangular.ySpacing", ySpacing);
                UpdateDefault("ArrayRectangular.zSpacing", zSpacing);
            }

            return TransformGeo.ArrayRectangular(geometry, plane, xCount, yCount, zCount, xSpacing, ySpacing, zSpacing);
        }

        /// <summary>环形阵列（绕轴旋转分布）</summary>
        public static GeometryBase[] ArrayPolar(GeometryBase geometry, Line axis, int count,
            double totalAngleRadians, bool rotate, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayPolar] 错误：geometry 为 null 或无效");
                return null;
            }
            if (count < 2 || !axis.IsValid)
            {
                RhinoApp.WriteLine("[ArrayPolar] 错误：数量小于 2 或旋转轴无效");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("ArrayPolar.count", count);
                UpdateDefault("ArrayPolar.totalAngle", totalAngleRadians);
                UpdateDefault("ArrayPolar.rotate", rotate);
            }

            return TransformGeo.ArrayPolar(geometry, axis, count, totalAngleRadians, rotate);
        }

        /// <summary>沿曲线按数量等分阵列</summary>
        public static GeometryBase[] ArrayAlongCrv(GeometryBase geometry, Curve rail, int count, bool orient, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayAlongCrv] 错误：geometry 为 null 或无效");
                return null;
            }
            if (rail == null || !rail.IsValid || count < 2)
            {
                RhinoApp.WriteLine("[ArrayAlongCrv] 错误：路径曲线无效或数量小于 2");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("ArrayAlongCrv.count", count);
                UpdateDefault("ArrayAlongCrv.orient", orient);
            }

            return TransformGeo.ArrayAlongCrv(geometry, rail, count, orient);
        }

        /// <summary>沿曲线按间距分布阵列</summary>
        public static GeometryBase[] ArrayAlongCrv(GeometryBase geometry, Curve rail, double spacing, bool orient, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayAlongCrv] 错误：geometry 为 null 或无效");
                return null;
            }
            if (rail == null || !rail.IsValid || spacing <= 0)
            {
                RhinoApp.WriteLine("[ArrayAlongCrv] 错误：路径曲线无效或间距小于等于 0");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("ArrayAlongCrv.spacing", spacing);
                UpdateDefault("ArrayAlongCrv.orient", orient);
            }

            return TransformGeo.ArrayAlongCrv(geometry, rail, spacing, orient);
        }

        /// <summary>在曲面 UV 方向上均匀阵列</summary>
        public static GeometryBase[] ArrayOnSrf(GeometryBase geometry, Brep surface, int uCount, int vCount, bool isPreview = false)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ArrayOnSrf] 错误：geometry 为 null 或无效");
                return null;
            }
            if (surface == null || !surface.IsValid || surface.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[ArrayOnSrf] 错误：曲面无效或无面");
                return null;
            }
            if (uCount < 1 || vCount < 1)
            {
                RhinoApp.WriteLine("[ArrayOnSrf] 错误：UV 方向数量必须大于 0");
                return null;
            }

            if (!isPreview)
            {
                UpdateDefault("ArrayOnSrf.uCount", uCount);
                UpdateDefault("ArrayOnSrf.vCount", vCount);
            }

            return TransformGeo.ArrayOnSrf(geometry, surface, uCount, vCount);
        }

        // ================================================================
        // 三、定向
        // ================================================================

        /// <summary>从源平面定向到目标平面</summary>
        public static GeometryBase Orient(GeometryBase geometry, Plane source, Plane target)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[Orient] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.Orient(geometry, source, target);
        }

        /// <summary>定向到曲面上指定点（使用曲面法线确定方向）</summary>
        public static GeometryBase OrientOnSrf(GeometryBase geometry, Plane source, Brep surface, Point3d targetPoint)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[OrientOnSrf] 错误：geometry 为 null 或无效");
                return null;
            }
            if (surface == null || !surface.IsValid || surface.Faces.Count == 0)
            {
                RhinoApp.WriteLine("[OrientOnSrf] 错误：曲面无效或无面");
                return null;
            }
            return TransformGeo.OrientOnSrf(geometry, source, surface, targetPoint);
        }

        /// <summary>定向到曲线上指定参数位置</summary>
        public static GeometryBase OrientOnCrv(GeometryBase geometry, Plane source, Curve rail, double parameter)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[OrientOnCrv] 错误：geometry 为 null 或无效");
                return null;
            }
            if (rail == null || !rail.IsValid)
            {
                RhinoApp.WriteLine("[OrientOnCrv] 错误：路径曲线为 null 或无效");
                return null;
            }
            return TransformGeo.OrientOnCrv(geometry, source, rail, parameter);
        }

        /// <summary>从旧工作平面重映射到新工作平面</summary>
        public static GeometryBase RemapCPlane(GeometryBase geometry, Plane oldCPlane, Plane newCPlane)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[RemapCPlane] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.RemapCPlane(geometry, oldCPlane, newCPlane);
        }

        // ================================================================
        // 四、投影
        // ================================================================

        /// <summary>正交投影到指定平面</summary>
        public static GeometryBase ProjectToCPlane(GeometryBase geometry, Plane plane)
        {
            if (geometry == null || !geometry.IsValid)
            {
                RhinoApp.WriteLine("[ProjectToCPlane] 错误：geometry 为 null 或无效");
                return null;
            }
            return TransformGeo.ProjectToCPlane(geometry, plane);
        }
    }
}
