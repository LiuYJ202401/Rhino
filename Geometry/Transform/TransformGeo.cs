using System;
using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.Trs
{
    /// <summary>
    /// 变换几何工具。
    /// 对已有几何对象施加变换（平移/旋转/缩放/镜像/阵列/定向/投影）。
    /// 纯几何计算，不假设平面方向，不访问 ActiveDoc。
    /// </summary>
    public static class TransformGeo
    {
        // ================================================================
        // 内部工具方法
        // ================================================================

        /// <summary>获取几何对象包围盒中心对应的源平面（WorldXY 方向）</summary>
        private static Plane GetSourcePlane(GeometryBase geometry)
        {
            BoundingBox bbox = geometry.GetBoundingBox(false);
            return new Plane(bbox.Center, Vector3d.XAxis, Vector3d.YAxis);
        }

        /// <summary>将模板对象的副本放置到曲线上的指定参数位置</summary>
        /// <param name="orient">true=沿曲线垂直框架定向，false=仅平移</param>
        private static GeometryBase PlaceOnCrv(GeometryBase template, Curve rail, double t, bool orient)
        {
            var dup = template.Duplicate();

            if (orient)
            {
                Plane frame;
                if (rail.PerpendicularFrameAt(t, out frame))
                {
                    var source = GetSourcePlane(template);
                    dup.Transform(Transform.PlaneToPlane(source, frame));
                    return dup;
                }
            }

            // 不定向或框架获取失败：仅平移到曲线点
            Point3d pt = rail.PointAt(t);
            BoundingBox bbox = template.GetBoundingBox(false);
            Vector3d offset = pt - bbox.Center;
            dup.Transform(Transform.Translation(offset));
            return dup;
        }

        /// <summary>将模板对象的副本放置到曲面上的指定 UV 参数位置</summary>
        private static GeometryBase PlaceOnSrf(GeometryBase template, BrepFace face, double u, double v)
        {
            var dup = template.Duplicate();
            Plane frame;

            if (face.FrameAt(u, v, out frame))
            {
                var source = GetSourcePlane(template);
                dup.Transform(Transform.PlaneToPlane(source, frame));
                return dup;
            }

            // 回退：仅平移到曲面点
            Point3d pt = face.PointAt(u, v);
            BoundingBox bbox = template.GetBoundingBox(false);
            Vector3d offset = pt - bbox.Center;
            dup.Transform(Transform.Translation(offset));
            return dup;
        }

        // ================================================================
        // 基本变换
        // ================================================================

        /// <summary>平移几何对象（修改原对象）</summary>
        public static GeometryBase Move(GeometryBase geometry, Vector3d translation)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.Translation(translation);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>复制几何对象并平移到新位置（不修改原对象）</summary>
        public static GeometryBase Copy(GeometryBase geometry, Vector3d translation)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var dup = geometry.Duplicate();
            var xform = Transform.Translation(translation);
            dup.Transform(xform);
            return dup;
        }

        /// <summary>绕 Z 轴旋转（工作平面旋转）</summary>
        public static GeometryBase Rotate(GeometryBase geometry, double angleRadians, Point3d center)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.Rotation(angleRadians, center);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>绕任意轴旋转（3D 旋转）</summary>
        public static GeometryBase Rotate(GeometryBase geometry, double angleRadians, Vector3d axis, Point3d center)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (!axis.IsValid || axis.IsZero)
                return null;

            var xform = Transform.Rotation(angleRadians, axis, center);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>均匀缩放</summary>
        public static GeometryBase Scale(GeometryBase geometry, Point3d anchor, double scaleFactor)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (scaleFactor <= 0)
                return null;

            var xform = Transform.Scale(anchor, scaleFactor);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>非均匀缩放（按平面定义三轴方向）</summary>
        public static GeometryBase Scale(GeometryBase geometry, Plane plane, double xFactor, double yFactor, double zFactor)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (xFactor <= 0 || yFactor <= 0 || zFactor <= 0)
                return null;

            var xform = Transform.Scale(plane, xFactor, yFactor, zFactor);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>以指定平面镜像</summary>
        public static GeometryBase Mirror(GeometryBase geometry, Plane mirrorPlane)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.Mirror(mirrorPlane);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>以过指定点且法线为指定方向的平面镜像</summary>
        public static GeometryBase Mirror(GeometryBase geometry, Point3d pointOnPlane, Vector3d normal)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (!normal.IsValid || normal.IsZero)
                return null;

            var xform = Transform.Mirror(pointOnPlane, normal);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>剪切变形</summary>
        public static GeometryBase Shear(GeometryBase geometry, Plane plane, Vector3d x, Vector3d y, Vector3d z)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.Shear(plane, x, y, z);
            geometry.Transform(xform);
            return geometry;
        }

        // ================================================================
        // 阵列
        // ================================================================

        /// <summary>沿直线方向均匀阵列（按间距）</summary>
        /// <param name="direction">方向向量，其长度=相邻副本间距</param>
        /// <returns>阵列结果数组（含原始位置对象在索引 0）</returns>
        public static GeometryBase[] ArrayLinear(GeometryBase geometry, Vector3d direction, int count)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (count < 2)
                return null;
            if (!direction.IsValid || direction.IsZero)
                return null;

            var results = new GeometryBase[count];
            // direction.Length = 相邻副本间距（与 Rhino 原生 ArrayLinear 一致）
            // 第 0 个在原位，第 i 个偏移 i * direction
            for (int i = 0; i < count; i++)
            {
                var dup = geometry.Duplicate();
                var xform = Transform.Translation(direction * i);
                dup.Transform(xform);
                results[i] = dup;
            }
            return results;
        }

        /// <summary>沿直线方向均匀阵列（按总跨度）</summary>
        /// <param name="from">分布起点</param>
        /// <param name="to">分布终点</param>
        /// <param name="count">副本数量（含起点位置）</param>
        /// <returns>阵列结果数组，第 0 个在 from，最后一个在 to，中间均匀分布</returns>
        public static GeometryBase[] ArrayLinear(GeometryBase geometry, Point3d from, Point3d to, int count)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (count < 2)
                return null;

            var results = new GeometryBase[count];
            // 将几何中心对齐到 from，然后在 from→to 之间均匀分布
            BoundingBox bbox = geometry.GetBoundingBox(false);
            Point3d center = bbox.Center;
            Vector3d totalSpan = to - from;

            for (int i = 0; i < count; i++)
            {
                var dup = geometry.Duplicate();
                double t = (double)i / (count - 1);
                // 先把中心移到 from，再沿 totalSpan 偏移 t
                Vector3d offset = (from - center) + totalSpan * t;
                var xform = Transform.Translation(offset);
                dup.Transform(xform);
                results[i] = dup;
            }
            return results;
        }

        /// <summary>矩形阵列（按平面定义 X/Y/Z 方向）</summary>
        /// <returns>阵列结果数组（含原始位置对象在 [0,0,0]）</returns>
        public static GeometryBase[] ArrayRectangular(GeometryBase geometry, Plane plane,
            int xCount, int yCount, int zCount,
            double xSpacing, double ySpacing, double zSpacing)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (xCount < 1 || yCount < 1 || zCount < 1)
                return null;

            var results = new List<GeometryBase>(xCount * yCount * zCount);

            for (int zi = 0; zi < zCount; zi++)
            {
                for (int yi = 0; yi < yCount; yi++)
                {
                    for (int xi = 0; xi < xCount; xi++)
                    {
                        var dup = geometry.Duplicate();
                        Vector3d offset = plane.XAxis * (xi * xSpacing)
                                        + plane.YAxis * (yi * ySpacing)
                                        + plane.ZAxis * (zi * zSpacing);
                        var xform = Transform.Translation(offset);
                        dup.Transform(xform);
                        results.Add(dup);
                    }
                }
            }
            return results.ToArray();
        }

        /// <summary>环形阵列（绕轴旋转分布）</summary>
        /// <returns>阵列结果数组（含原始位置对象在索引 0）</returns>
        public static GeometryBase[] ArrayPolar(GeometryBase geometry, Line axis, int count,
            double totalAngleRadians, bool rotate)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (count < 2)
                return null;
            if (!axis.IsValid)
                return null;

            var results = new GeometryBase[count];
            double stepAngle = totalAngleRadians / count;
            Vector3d axisDir = axis.Direction;
            axisDir.Unitize();

            // 预计算原始包围盒中心（不随循环变化）
            BoundingBox bbox = geometry.GetBoundingBox(false);
            Point3d originalCenter = bbox.Center;

            for (int i = 0; i < count; i++)
            {
                var dup = geometry.Duplicate();
                double angle = stepAngle * i;

                if (rotate)
                {
                    // 副本随阵列旋转
                    var xform = Transform.Rotation(angle, axisDir, axis.From);
                    dup.Transform(xform);
                }
                else
                {
                    // 副本不旋转，仅将中心移动到旋转后的位置
                    Point3d rotatedCenter = originalCenter;
                    rotatedCenter.Transform(Transform.Rotation(angle, axisDir, axis.From));
                    Vector3d translation = rotatedCenter - originalCenter;
                    dup.Transform(Transform.Translation(translation));
                }
                results[i] = dup;
            }
            return results;
        }

        /// <summary>沿曲线按数量等分阵列</summary>
        public static GeometryBase[] ArrayAlongCrv(GeometryBase geometry, Curve rail, int count, bool orient)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (rail == null || !rail.IsValid)
                return null;
            if (count < 2)
                return null;

            var results = new GeometryBase[count];
            double domainStart = rail.Domain.Min;
            double domainEnd = rail.Domain.Max;

            for (int i = 0; i < count; i++)
            {
                double t = domainStart + (domainEnd - domainStart) * (double)i / (count - 1);
                results[i] = PlaceOnCrv(geometry, rail, t, orient);
            }
            return results;
        }

        /// <summary>沿曲线按间距分布阵列</summary>
        public static GeometryBase[] ArrayAlongCrv(GeometryBase geometry, Curve rail, double spacing, bool orient)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (rail == null || !rail.IsValid)
                return null;
            if (spacing <= 0)
                return null;

            // 根据曲线长度和间距计算数量
            double length = rail.GetLength();
            int count = (int)Math.Floor(length / spacing) + 1;
            if (count < 2)
                count = 2;

            var results = new GeometryBase[count];
            double domainStart = rail.Domain.Min;
            double domainEnd = rail.Domain.Max;

            for (int i = 0; i < count; i++)
            {
                double arcLen = spacing * i;
                double t;
                if (!rail.LengthParameter(arcLen, out t))
                    t = domainStart + (domainEnd - domainStart) * (double)i / (count - 1);

                results[i] = PlaceOnCrv(geometry, rail, t, orient);
            }
            return results;
        }

        /// <summary>在曲面 UV 方向上均匀阵列</summary>
        public static GeometryBase[] ArrayOnSrf(GeometryBase geometry, Brep surface, int uCount, int vCount)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (surface == null || !surface.IsValid || surface.Faces.Count == 0)
                return null;
            if (uCount < 1 || vCount < 1)
                return null;

            var results = new GeometryBase[uCount * vCount];
            var face = surface.Faces[0];
            double uMin = face.Domain(0).Min;
            double uMax = face.Domain(0).Max;
            double vMin = face.Domain(1).Min;
            double vMax = face.Domain(1).Max;
            int idx = 0;

            for (int ui = 0; ui < uCount; ui++)
            {
                for (int vi = 0; vi < vCount; vi++)
                {
                    double u = uCount == 1 ? uMin : uMin + (uMax - uMin) * (double)ui / (uCount - 1);
                    double v = vCount == 1 ? vMin : vMin + (vMax - vMin) * (double)vi / (vCount - 1);
                    results[idx++] = PlaceOnSrf(geometry, face, u, v);
                }
            }
            return results;
        }

        // ================================================================
        // 定向
        // ================================================================

        /// <summary>从源平面定向到目标平面</summary>
        public static GeometryBase Orient(GeometryBase geometry, Plane source, Plane target)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.PlaneToPlane(source, target);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>定向到曲面上指定点（使用曲面法线确定方向）</summary>
        public static GeometryBase OrientOnSrf(GeometryBase geometry, Plane source, Brep surface, Point3d targetPoint)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (surface == null || !surface.IsValid || surface.Faces.Count == 0)
                return null;

            // 使用面找到最近参数点
            double u, v;
            var face = surface.Faces[0];
            if (!face.ClosestPoint(targetPoint, out u, out v))
                return null;

            // 获取曲面框架
            Plane frame;
            if (!face.FrameAt(u, v, out frame))
                return null;

            var xform = Transform.PlaneToPlane(source, frame);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>定向到曲线上指定参数位置（使用曲线垂直框架）</summary>
        public static GeometryBase OrientOnCrv(GeometryBase geometry, Plane source, Curve rail, double parameter)
        {
            if (geometry == null || !geometry.IsValid)
                return null;
            if (rail == null || !rail.IsValid)
                return null;

            Plane frame;
            if (!rail.PerpendicularFrameAt(parameter, out frame))
                return null;

            var xform = Transform.PlaneToPlane(source, frame);
            geometry.Transform(xform);
            return geometry;
        }

        /// <summary>从旧工作平面重映射到新工作平面</summary>
        public static GeometryBase RemapCPlane(GeometryBase geometry, Plane oldCPlane, Plane newCPlane)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.PlaneToPlane(oldCPlane, newCPlane);
            geometry.Transform(xform);
            return geometry;
        }

        // ================================================================
        // 投影
        // ================================================================

        /// <summary>正交投影到指定平面</summary>
        public static GeometryBase ProjectToCPlane(GeometryBase geometry, Plane plane)
        {
            if (geometry == null || !geometry.IsValid)
                return null;

            var xform = Transform.PlanarProjection(plane);
            geometry.Transform(xform);
            return geometry;
        }
    }
}
