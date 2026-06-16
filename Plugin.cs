using Rhino;
using Rhino.PlugIns;

namespace Rh
{
    /// <summary>
    /// 插件入口类。Rhino 会自动创建唯一实例，请勿手动实例化。
    /// </summary>
    public class Plugin : PlugIn
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

        // 开发期间可重写 LoadTime => AtStartup，确保新命令在 Rhino 启动后立即可用。
        // 发布时应保持默认 WhenNeeded，避免拖慢 Rhino 启动。
        // 详见 SKILL.md 2.9 节「Rhino 插件命令发现机制」。
    }
}
