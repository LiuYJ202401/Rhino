namespace Rh.UI
{
    /// <summary>
    /// 步骤定义，包含三个回调函数
    /// </summary>
    public class StepDef
    {
        /// <summary>步骤名称（用于调试）</summary>
        public string Name { get; }

        /// <summary>声明输入</summary>
        internal SetupHandler Setup { get; }

        /// <summary>处理逻辑（参数：输入结果，是否预览）</summary>
        internal ProcessHandler Process { get; }

        /// <summary>预览函数（返回要绘制的几何对象数组）</summary>
        internal PreviewHandler Preview { get; }

        internal StepDef(string name, SetupHandler setup, ProcessHandler process, PreviewHandler preview)
        {
            Name = name;
            Setup = setup;
            Process = process;
            Preview = preview;
        }
    }

    /// <summary>声明输入的委托</summary>
    public delegate void SetupHandler(InputBuilder input);

    /// <summary>处理输入的委托（isPreview=true 表示预览模式）</summary>
    public delegate void ProcessHandler(InputResult result, bool isPreview);

    /// <summary>返回预览几何的委托</summary>
    public delegate object[] PreviewHandler();

    /// <summary>
    /// 创建步骤的辅助类
    /// </summary>
    public static class Step
    {
        /// <summary>创建步骤</summary>
        public static StepDef Create(string name, SetupHandler setup, ProcessHandler process)
        {
            return new StepDef(name, setup, process, null);
        }

        /// <summary>创建步骤（带预览）</summary>
        public static StepDef Create(string name, SetupHandler setup, ProcessHandler process, PreviewHandler preview)
        {
            return new StepDef(name, setup, process, preview);
        }
    }
}
