using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rhino;

namespace Rh.Data
{
    // ================================================================
    // 异常类型
    // ================================================================

    /// <summary>数据文件未找到</summary>
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException(string path)
            : base(string.Format("数据文件未找到：{0}", path)) { }
    }

    /// <summary>key 路径在 JSON 中不存在</summary>
    public class DataKeyException : Exception
    {
        public DataKeyException(string key)
            : base(string.Format("数据 key 不存在：{0}", key)) { }
    }

    /// <summary>动态值来源不被识别</summary>
    public class DynamicSourceException : Exception
    {
        public DynamicSourceException(string source)
            : base(string.Format("未知的动态值来源：{0}", source)) { }
    }

    /// <summary>文件写入失败</summary>
    public class DataWriteException : Exception
    {
        public DataWriteException(string path, string reason)
            : base(string.Format("写入数据文件失败：{0}（{1}）", path, reason)) { }
    }

    // ================================================================
    // DataReader 封装类
    // ================================================================

    /// <summary>
    /// JSON 数据文件的读取、修改、写入和缓存。
    /// 按需读写单个字段，不反序列化整个对象。
    ///
    /// 使用方式：
    ///   DataReader.GetValue&lt;double&gt;("Command/basicCommand/Curve.json", "CreateCircle.radius")
    ///   DataReader.SetValue&lt;double&gt;("Command/basicCommand/Curve.json", "CreateCircle.radius", 3.2)
    /// </summary>
    public static class DataReader
    {
        // ================================================================
        // 缓存与锁
        // ================================================================

        /// <summary>文件级缓存：relativePath → JObject</summary>
        private static readonly ConcurrentDictionary<string, JObject> _cache =
            new ConcurrentDictionary<string, JObject>();

        /// <summary>文件写入锁（每个文件一把锁，避免并发写入冲突）</summary>
        private static readonly ConcurrentDictionary<string, object> _fileLocks =
            new ConcurrentDictionary<string, object>();

        // ================================================================
        // 路径解析
        // ================================================================

        /// <summary>
        /// Data 目录的绝对路径。
        /// 取 Plugin 所在程序集目录下的 Data 文件夹。
        /// </summary>
        private static string DataRoot
        {
            get
            {
                string assemblyDir = Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().Location);
                return Path.Combine(assemblyDir, "Data");
            }
        }

        /// <summary>
        /// 将相对路径转换为绝对路径。
        /// </summary>
        private static string ResolveFullPath(string relativePath)
        {
            return Path.Combine(DataRoot, relativePath);
        }

        /// <summary>
        /// 获取或创建文件级锁对象。
        /// </summary>
        private static object GetFileLock(string relativePath)
        {
            return _fileLocks.GetOrAdd(relativePath, _ => new object());
        }

        // ================================================================
        // 文件加载
        // ================================================================

        /// <summary>
        /// 加载 JSON 文件为 JObject（首次读磁盘，后续从缓存取）。
        /// 文件不存在时自动创建空 JObject。
        /// </summary>
        private static JObject LoadFile(string relativePath)
        {
            return _cache.GetOrAdd(relativePath, path =>
            {
                string fullPath = ResolveFullPath(path);

                if (!File.Exists(fullPath))
                    throw new DataNotFoundException(path);

                string json = File.ReadAllText(fullPath);

                if (string.IsNullOrWhiteSpace(json))
                    return new JObject();

                return JObject.Parse(json);
            });
        }

        // ================================================================
        // 读取
        // ================================================================

        /// <summary>
        /// 按文件路径和 key 从缓存的 JObject 中取单个值。
        /// key 用点号分隔层级，如 "CreateNurbsCurve.degree"。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="relativePath">相对于 Data/ 目录的路径（如 Command/basicCommand/Curve.json）</param>
        /// <param name="key">点号分隔的层级路径（如 CreateCircle.radius）</param>
        /// <returns>读取到的值</returns>
        public static T GetValue<T>(string relativePath, string key)
        {
            JObject jObject = LoadFile(relativePath);

            JToken token = jObject.SelectToken(key);

            if (token == null)
                throw new DataKeyException(key);

            return token.Value<T>();
        }

        /// <summary>
        /// 按文件路径和 key 从缓存的 JObject 中取单个值。
        /// key 不存在时返回 defaultValue（不抛异常）。
        /// </summary>
        public static T GetValue<T>(string relativePath, string key, T defaultValue)
        {
            JObject jObject = LoadFile(relativePath);

            JToken token = jObject.SelectToken(key);

            if (token == null)
                return defaultValue;

            return token.Value<T>();
        }

        // ================================================================
        // 动态值
        // ================================================================

        /// <summary>
        /// 从 RhinoDoc 运行时获取动态值（如公差）。
        /// </summary>
        /// <param name="source">动态值来源名称</param>
        /// <returns>运行时动态值</returns>
        public static double GetDynamicValue(string source)
        {
            RhinoDoc doc = RhinoDoc.ActiveDoc;

            if (doc == null)
                throw new DynamicSourceException("RhinoDoc 未就绪");

            switch (source)
            {
                case "ModelAbsoluteTolerance":
                    return doc.ModelAbsoluteTolerance;

                case "ModelAngleToleranceRadians":
                    return doc.ModelAngleToleranceRadians;

                default:
                    throw new DynamicSourceException(source);
            }
        }

        // ================================================================
        // 写入
        // ================================================================

        /// <summary>
        /// 修改 JObject 中指定 key 的值，同步更新缓存和磁盘文件。
        /// key 路径不存在时自动创建嵌套结构。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="relativePath">相对于 Data/ 目录的路径</param>
        /// <param name="key">点号分隔的层级路径</param>
        /// <param name="value">要写入的值</param>
        public static void SetValue<T>(string relativePath, string key, T value)
        {
            JObject jObject = LoadFile(relativePath);

            object fileLock = GetFileLock(relativePath);

            lock (fileLock)
            {
                // 更新 JObject 中的值（自动创建嵌套结构）
                SetNestedValue(jObject, key, JToken.FromObject(value));

                // 写入磁盘
                WriteToDisk(relativePath, jObject);
            }
        }

        // ================================================================
        // 管理
        // ================================================================

        /// <summary>
        /// 清除指定文件缓存并重新加载。
        /// </summary>
        /// <param name="relativePath">相对于 Data/ 目录的路径</param>
        public static void Reload(string relativePath)
        {
            JObject removed;
            _cache.TryRemove(relativePath, out removed);
            LoadFile(relativePath);
        }

        // ================================================================
        // 内部方法
        // ================================================================

        /// <summary>
        /// 在 JObject 中设置嵌套 key 的值。
        /// 如 key="CreateCircle.radius"，会确保 CreateCircle 对象存在。
        /// </summary>
        private static void SetNestedValue(JObject root, string key, JToken value)
        {
            string[] parts = key.Split('.');

            JToken current = root;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                string part = parts[i];

                JToken child = current.SelectToken(part);

                if (child == null)
                {
                    var newObj = new JObject();
                    current[part] = newObj;
                    current = newObj;
                }
                else if (child.Type == JTokenType.Object)
                {
                    current = child;
                }
                else
                {
                    // 原始值不是 Object，替换为新 JObject
                    var newObj = new JObject();
                    current[part] = newObj;
                    current = newObj;
                }
            }

            current[parts[parts.Length - 1]] = value;
        }

        /// <summary>
        /// 将 JObject 写入磁盘文件。
        /// </summary>
        private static void WriteToDisk(string relativePath, JObject jObject)
        {
            string fullPath = ResolveFullPath(relativePath);

            try
            {
                string directory = Path.GetDirectoryName(fullPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                File.WriteAllText(fullPath, json);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new DataWriteException(relativePath, ex.Message);
            }
            catch (IOException ex)
            {
                throw new DataWriteException(relativePath, ex.Message);
            }
        }
    }
}
