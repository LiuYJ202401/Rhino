using System.Collections.Generic;
using Rhino.Geometry;

namespace Rh.Geo.FenghuangCenter
{
    /// <summary>
    /// Iso-curve 批量提取工具。
    /// 框架只有单条提取（CreateExtractIsocurve），本类提供按等参数间距批量提取。
    /// </summary>
    public static class IsoCurveGeo
    {
        /// <summary>
        /// 从曲面批量提取 iso-curve。
        /// </summary>
        /// <param name="surface">目标曲面</param>
        /// <param name="curveCount">提取数量（≥3）</param>
        /// <param name="useVDirection">true=沿 V 方向提取（固定 U），false=沿 U 方向提取（固定 V）</param>
        /// <param name="closedLoop">是否闭合环路（影响参数间距：闭合用整除，非闭合用 n-1 等分）</param>
        /// <returns>iso-curve 列表；失败返回空列表</returns>
        public static List<Curve> Extract(Surface surface, int curveCount, bool useVDirection, bool closedLoop)
        {
            var curves = new List<Curve>();

            if (surface == null || !surface.IsValid)
                return curves;

            // 确定参数域：useVDirection 时沿 U 域遍历（固定 U 提取 V 曲线）
            Interval domain = useVDirection ? surface.Domain(0) : surface.Domain(1);
            if (!domain.IsValid)
                return curves;

            for (int i = 0; i < curveCount; i++)
            {
                double t;
                if (curveCount == 1)
                    t = domain.Mid;
                else if (closedLoop)
                    t = domain.T0 + (double)i / curveCount * domain.Length;
                else
                    t = domain.T0 + (double)i / (curveCount - 1) * domain.Length;

                // direction=1 → 固定 U=t 的 V 方向曲线；direction=0 → 固定 V=t 的 U 方向曲线
                Curve isoCurve = useVDirection
                    ? surface.IsoCurve(1, t)
                    : surface.IsoCurve(0, t);

                if (isoCurve != null && isoCurve.IsValid && isoCurve.GetLength() > 0.001)
                    curves.Add(isoCurve);
            }

            return curves;
        }
    }
}
