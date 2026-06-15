using Rhino;

namespace Rh
{
    /// <summary>
    /// 插件入口类。Rhino 会自动创建唯一实例，请勿手动实例化。
    /// </summary>
    public class Plugin : Rhino.PlugIns.PlugIn
    {
        /// <summary>
        /// 初始化插件实例。
        /// </summary>
        public Plugin()
        {
            Instance = this;
        }

        /// <summary>
        /// 获取插件单例。
        /// </summary>
        public static Plugin Instance { get; private set; }

        // 可在此处重写方法来改变插件的加载、卸载行为，
        // 或向 Rhino _Option 命令添加选项页。
    }
}
