using System;
using Rhino.Geometry;

namespace Rh.Project.Test.Framework
{
    /// <summary>
    /// 断言失败异常。由 Assert 方法在失败时抛出，被 Step 的 try-catch 捕获，
    /// 使当前步骤标记为 FAIL 但不崩溃，继续执行下一步骤。
    /// </summary>
    public class AssertFailedException : Exception
    {
        public AssertFailedException(string message) : base(message) { }
    }

    /// <summary>
    /// 轻量断言工具。断言失败时抛出 AssertFailedException，由 Step 捕获后继续下一步骤。
    ///
    /// 使用规则：
    /// - 值类型几何（Circle/Arc/Line/Polyline/Ellipse）用 NotNull
    /// - 引用类型几何（Curve/Brep/Mesh/Surface）用 IsValid
    /// </summary>
    public static class Assert
    {
        /// <summary>记录最近一次断言是否通过</summary>
        public static bool LastResult { get; private set; } = true;

        /// <summary>非空检查 — 用于值类型几何（Circle/Arc/Line/Polyline/Ellipse）</summary>
        public static void NotNull(object value, string message = "")
        {
            LastResult = value != null;
            if (!LastResult)
                throw new AssertFailedException($"{message}: 结果为 null");
        }

        /// <summary>有效性检查 — 用于引用类型几何（Curve/Brep/Mesh/Surface）</summary>
        public static void IsValid(GeometryBase geo, string message = "")
        {
            LastResult = geo != null && geo.IsValid;
            if (!LastResult)
                throw new AssertFailedException($"{message}: 几何对象无效或为 null");
        }

        /// <summary>布尔断言</summary>
        public static void IsTrue(bool condition, string message = "")
        {
            LastResult = condition;
            if (!condition)
                throw new AssertFailedException(message);
        }

        /// <summary>数值相等断言（默认容差 1e-6）</summary>
        public static void AreEqual(double expected, double actual, string message = "", double tolerance = 1e-6)
        {
            LastResult = Math.Abs(expected - actual) < tolerance;
            if (!LastResult)
                throw new AssertFailedException($"{message}: 期望 {expected}, 实际 {actual}");
        }

        /// <summary>数组长度断言</summary>
        public static void Count(int expected, int actual, string message = "")
        {
            LastResult = expected == actual;
            if (!LastResult)
                throw new AssertFailedException($"{message}: 期望 {expected} 项, 实际 {actual} 项");
        }

        /// <summary>大于零检查</summary>
        public static void GreaterThanZero(double value, string message = "")
        {
            LastResult = value > 0;
            if (!LastResult)
                throw new AssertFailedException($"{message}: 值 {value} 不大于 0");
        }
    }
}
