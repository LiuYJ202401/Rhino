using System.Collections.Generic;

namespace Rh.UI
{
    /// <summary>
    /// 步骤的输入结果，开发者通过 Get&lt;T&gt; 读取输入值
    /// </summary>
    public class InputResult
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>设置值（模板内部使用）</summary>
        internal void Set<T>(string name, T value)
        {
            _values[name] = value;
        }

        /// <summary>获取值</summary>
        public T Get<T>(string name)
        {
            if (_values.TryGetValue(name, out var value))
                return (T)value;
            return default;
        }

        /// <summary>是否包含指定名称的值</summary>
        public bool Has(string name)
        {
            return _values.ContainsKey(name);
        }
    }
}
