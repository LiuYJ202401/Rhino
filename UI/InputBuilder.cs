using System.Collections.Generic;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace Rh.UI
{
    // ================================================================
    // 输入项基类和子类
    // ================================================================

    /// <summary>
    /// 输入项基类
    /// </summary>
    internal abstract class InputItem
    {
        public string Name { get; }
        public string Prompt { get; }

        protected InputItem(string name, string prompt)
        {
            Name = name;
            Prompt = prompt;
        }

        public virtual void AddToGetPoint(GetPoint gp) { }
        public virtual void AddToGetObject(GetObject go) { }
        public abstract object CurrentValue { get; }

        /// <summary>Get() 返回 Option 后同步内部状态（列表选项需要覆盖）</summary>
        public virtual void UpdateFromGetter(GetPoint gp) { }
        public virtual void UpdateFromGetter(GetObject go) { }
    }

    /// <summary>点输入</summary>
    internal class PointInputItem : InputItem
    {
        public PointInputItem(string name, string prompt) : base(name, prompt) { }
        public override object CurrentValue => null;
    }

    /// <summary>多点输入</summary>
    internal class PointsInputItem : InputItem
    {
        public int MinCount { get; }

        public PointsInputItem(string name, string prompt, int minCount) : base(name, prompt)
        {
            MinCount = minCount;
        }

        public override object CurrentValue => null;
    }

    /// <summary>对象输入</summary>
    internal class ObjectInputItem : InputItem
    {
        public ObjectType Filter { get; }
        public int MinCount { get; }
        public bool Multiple { get; }

        public ObjectInputItem(string name, string prompt, ObjectType filter, bool multiple, int minCount)
            : base(name, prompt)
        {
            Filter = filter;
            Multiple = multiple;
            MinCount = minCount;
        }

        public override object CurrentValue => null;
    }

    /// <summary>数值选项</summary>
    internal class DoubleInputItem : InputItem
    {
        private OptionDouble _option;

        public DoubleInputItem(string name, string prompt, double defaultValue)
            : base(name, prompt)
        {
            _option = new OptionDouble(defaultValue);
        }

        public override void AddToGetPoint(GetPoint gp)
        {
            gp.AddOptionDouble(Name, ref _option);
        }

        public override void AddToGetObject(GetObject go)
        {
            go.AddOptionDouble(Name, ref _option);
        }

        public override object CurrentValue => _option.CurrentValue;
    }

    /// <summary>整数选项</summary>
    internal class IntegerInputItem : InputItem
    {
        private OptionInteger _option;

        public IntegerInputItem(string name, string prompt, int defaultValue)
            : base(name, prompt)
        {
            _option = new OptionInteger(defaultValue);
        }

        public override void AddToGetPoint(GetPoint gp)
        {
            gp.AddOptionInteger(Name, ref _option);
        }

        public override void AddToGetObject(GetObject go)
        {
            go.AddOptionInteger(Name, ref _option);
        }

        public override object CurrentValue => _option.CurrentValue;
    }

    /// <summary>开关选项</summary>
    internal class ToggleInputItem : InputItem
    {
        private OptionToggle _option;

        public ToggleInputItem(string name, string prompt, bool defaultValue)
            : base(name, prompt)
        {
            _option = new OptionToggle(defaultValue, "Off", "On");
        }

        public override void AddToGetPoint(GetPoint gp)
        {
            gp.AddOptionToggle(Name, ref _option);
        }

        public override void AddToGetObject(GetObject go)
        {
            go.AddOptionToggle(Name, ref _option);
        }

        public override object CurrentValue => _option.CurrentValue;
    }

    /// <summary>列表选项（从多个预设值中选择一个，返回选中索引）</summary>
    internal class ListInputItem : InputItem
    {
        private readonly List<string> _values;
        private int _currentIndex;

        public ListInputItem(string name, string prompt, string[] values, int defaultIndex)
            : base(name, prompt)
        {
            _values = new List<string>(values);
            _currentIndex = defaultIndex;
        }

        public override void AddToGetPoint(GetPoint gp)
        {
            gp.AddOptionList(Name, _values, _currentIndex);
        }

        public override void AddToGetObject(GetObject go)
        {
            go.AddOptionList(Name, _values, _currentIndex);
        }

        public override void UpdateFromGetter(GetPoint gp)
        {
            var opt = gp.Option();
            if (opt != null && opt.EnglishName == Name)
                _currentIndex = opt.CurrentListOptionIndex;
        }

        public override void UpdateFromGetter(GetObject go)
        {
            var opt = go.Option();
            if (opt != null && opt.EnglishName == Name)
                _currentIndex = opt.CurrentListOptionIndex;
        }

        public override object CurrentValue => _currentIndex;
    }

    // ================================================================
    // InputBuilder
    // ================================================================

    /// <summary>
    /// 输入声明构建器，开发者在步骤的 Setup 函数中使用
    /// </summary>
    public class InputBuilder
    {
        private readonly List<InputItem> _items = new List<InputItem>();
        private Point3d? _basePoint;
        private bool _showBasePointDistance;

        internal string Prompt => _items.Count > 0 ? _items[0].Prompt : "";

        internal bool HasPoint => _items.Find(i => i is PointInputItem) != null;
        internal bool HasPoints => _items.Find(i => i is PointsInputItem) != null;
        internal bool HasObject => _items.Find(i => i is ObjectInputItem) != null;

        internal PointInputItem PointItem => _items.Find(i => i is PointInputItem) as PointInputItem;
        internal PointsInputItem PointsItem => _items.Find(i => i is PointsInputItem) as PointsInputItem;
        internal ObjectInputItem ObjectItem => _items.Find(i => i is ObjectInputItem) as ObjectInputItem;

        internal bool HasBasePoint => _basePoint.HasValue;
        internal Point3d BasePointValue => _basePoint ?? Point3d.Unset;
        internal bool ShowBasePointDistance => _showBasePointDistance;

        internal IEnumerable<InputItem> OptionItems
        {
            get
            {
                foreach (var item in _items)
                {
                    if (!(item is PointInputItem) && !(item is PointsInputItem) && !(item is ObjectInputItem))
                        yield return item;
                }
            }
        }

        /// <summary>声明点输入</summary>
        public InputBuilder Point(string name, string prompt)
        {
            _items.Add(new PointInputItem(name, prompt));
            return this;
        }

        /// <summary>声明多点输入</summary>
        public InputBuilder Points(string name, string prompt, int minCount)
        {
            _items.Add(new PointsInputItem(name, prompt, minCount));
            return this;
        }

        /// <summary>声明单对象输入</summary>
        public InputBuilder Object(string name, string prompt, ObjectType filter)
        {
            _items.Add(new ObjectInputItem(name, prompt, filter, false, 1));
            return this;
        }

        /// <summary>声明多对象输入</summary>
        public InputBuilder Objects(string name, string prompt, ObjectType filter, int minCount)
        {
            _items.Add(new ObjectInputItem(name, prompt, filter, true, minCount));
            return this;
        }

        /// <summary>声明数值选项</summary>
        public InputBuilder Double(string name, string prompt, double defaultValue)
        {
            _items.Add(new DoubleInputItem(name, prompt, defaultValue));
            return this;
        }

        /// <summary>声明整数选项</summary>
        public InputBuilder Integer(string name, string prompt, int defaultValue)
        {
            _items.Add(new IntegerInputItem(name, prompt, defaultValue));
            return this;
        }

        /// <summary>声明开关选项</summary>
        public InputBuilder Toggle(string name, string prompt, bool defaultValue)
        {
            _items.Add(new ToggleInputItem(name, prompt, defaultValue));
            return this;
        }

        /// <summary>声明列表选项（返回选中索引，从 0 开始）</summary>
        public InputBuilder List(string name, string prompt, string[] options, int defaultIndex = 0)
        {
            _items.Add(new ListInputItem(name, prompt, options, defaultIndex));
            return this;
        }

        /// <summary>设置基点（鼠标拖动时显示距离和角度，约束从基点出发）</summary>
        public InputBuilder BasePoint(Point3d point, bool showDistance = true)
        {
            _basePoint = point;
            _showBasePointDistance = showDistance;
            return this;
        }
    }
}
