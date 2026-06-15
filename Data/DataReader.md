# DataReader 数据读取封装类

命名空间：`Rh.Data`

## 功能

封装 JSON 数据文件的读取、修改、写入和缓存。按需读写单个字段，不反序列化整个对象。

## 核心设计

- **按字段读写**：每次只读取或修改需要的单个值，不加载整个强类型对象
- **JObject 缓存**：首次访问某文件时解析为 `JObject`（Newtonsoft.Json 轻量对象）并缓存，后续直接从内存按 key 取值
- **自动持久化**：命令执行时使用的参数值自动更新为新的默认值，下次调用直接生效
- **无需强类型类**：不需要为每个 JSON 文件定义 C# 类，通过 key 路径直接访问

## 接口

### 读取

| 方法 | 说明 |
|------|------|
| `T GetValue<T>(string relativePath, string key)` | 按文件路径和 key 从缓存的 JObject 中取单个值。首次访问时加载并缓存文件 |
| `double GetDynamicValue(string source)` | 从 RhinoDoc 运行时获取动态值（如公差） |

### 写入

| 方法 | 说明 |
|------|------|
| `void SetValue<T>(string relativePath, string key, T value)` | 修改 JObject 中指定 key 的值，同步更新缓存和磁盘文件 |

### 管理

| 方法 | 说明 |
|------|------|
| `void Reload(string relativePath)` | 清除指定文件缓存并重新加载 |

## 参数说明

| 参数 | 类型 | 说明 |
|------|------|------|
| `relativePath` | string | 相对于 `Data/` 目录的路径（如 `Command/basicCommand/Curve.json`） |
| `key` | string | 点号分隔的层级路径（如 `CreateNurbsCurve.degree`） |
| `value` | T | 要写入的值 |
| `source` | string | 动态值来源（如 `ModelAbsoluteTolerance`） |

## 报错

| 错误类型 | 触发条件 |
|----------|---------|
| `DataNotFoundException` | 文件路径不存在 |
| `DataKeyException` | key 路径在 JSON 中不存在 |
| `DynamicSourceException` | source 不被识别或 RhinoDoc 未就绪 |
| `DataWriteException` | 文件写入失败（权限/占用） |

## 缓存机制

- 首次访问某文件时：读磁盘 → 解析为 `JObject` → 存入 `ConcurrentDictionary<string, JObject>` 缓存
- 后续访问：直接从内存 JObject 按 key 路径取值，不读磁盘
- `SetValue()` 同时更新内存 JObject 和磁盘文件
- `Reload()` 清除单个文件缓存

## 动态值

| source | 来源 |
|--------|------|
| `ModelAbsoluteTolerance` | `RhinoDoc.ActiveDoc.ModelAbsoluteTolerance` |
| `ModelAngleToleranceRadians` | `RhinoDoc.ActiveDoc.ModelAngleToleranceRadians` |

## 使用流程

以 `CreateCircle` 命令为例（Command 层是唯一直接调用 DataReader 的层）：

### 预览阶段（isPreview: true）— 只读不写

```
1. Project 层初始化 UI，需要默认值
   → Project 调用 Command.GetDefaultRadius()
   → Command 内部 DataReader.GetValue<double>(...) → 返回 1.0

2. 用户修改数值，触发预览
   → Project 调用 Command.CreateCircle(center, 3.2, isPreview: true)
   → Command 不调用 DataReader.SetValue（预览不更新 Data）
```

### 执行阶段（isPreview: false）— 读写都做

```
3. 用户确定，最终执行
   → Project 调用 Command.CreateCircle(center, 3.2)  // isPreview 默认 false
   → Command 内部 DataReader.SetValue<double>(..., 3.2)
   → 更新缓存 JObject + 写入磁盘文件

4. 下次 Project 层调用
   → Command 层 GetValue → 从缓存直接返回 3.2（上次使用的值）
```

### 设计要点

- **预览时不调用 SetValue**：用户取消命令时 Data 不被修改
- **只有最终执行时才 SetValue**：确保只有用户确认的值才被持久化

## 使用示例

```csharp
// === Command 层内部实现（唯一直接调用 DataReader 的层）===

public static Circle CreateCircle(Plane plane, Point3d center, double radius = double.NaN)
{
    // 未传入时自动从 Data 获取默认值
    if (double.IsNaN(radius))
        radius = DataReader.GetValue<double>(
            "Command/basicCommand/Curve.json",
            "CreateCircle.radius");

    // 创建几何对象...

    // 执行完毕，自动更新默认值为本次使用的值
    DataReader.SetValue<double>(
        "Command/basicCommand/Curve.json",
        "CreateCircle.radius",
        radius);

    return circle;
}

// === Project 层调用（不接触 DataReader）===

// 方式一：用默认值（快速开发）
var circle = BasicCommand.CreateCircle(plane, center);

// 方式二：显式指定（精确控制）
var circle = BasicCommand.CreateCircle(plane, center, 5.0);
```

## 实现备注

- 使用 `Newtonsoft.Json.Linq.JObject` 作为缓存单元，按 key 路径访问字段
- `key` 用 `SelectToken("CreateNurbsCurve.degree")` 方式解析层级路径
- 写入时用 `JsonConvert.SerializeObject(jObject, Formatting.Indented)` 保持可读格式
- 线程安全：缓存用 `ConcurrentDictionary`，文件写入加锁
- JSON 文件从磁盘读写，路径基于 `Data/` 根目录
