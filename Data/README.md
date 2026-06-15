# Data 层

命名空间：`Rh.Data`

## 目的

存储软编码的默认数据、常数、Rhino 文档设置（公差、单位等）。替代硬编码，集中管理所有可配置参数。

**调用者**：Command 层是唯一直接调用 DataReader 的层，其他层不直接接触 Data 层。

## 结构

| 子文件夹/文件 | 对应调用者 | 内容 |
|---------------|-----------|------|
| `Command/basicCommand/` | `Command/basicCommand/` | 基础命令的默认参数值 |
| `DataReader.md` | 本层所有数据文件 | 数据读取封装类的接口文档 |

`Command/basicCommand/` 内文件与 `Command/basicCommand/` 中的接口文档一一对应：

| 文件 | 内容 |
|------|------|
| `Point.md` | Point 命令默认值 |
| `Curve.md` | Curve 命令默认值（阶数、节点样式、公差等） |

`DataReader.md` 是 Data 层核心读取类的接口文档，所有其他层通过此类读取 Data 层数据。

## 数据格式

采用 **JSON 文件** 作为数据存储格式：

```
Data/
└── Command/
    └── basicCommand/
        ├── Point.json
        └── Curve.json
```

- 每个接口文档对应一个 `.json` 文件（同名）
- JSON 文件与同目录的 `.md` 说明文档一一对应
- JSON 文件结构：按命令名分组，每个命令下列出参数名和默认值

## 实现方式

1. **数据文件**：`.json` 文件存储实际默认值，可被程序运行时读取
2. **读取类**：C# 提供 `DataReader<T>` 封装类，按需加载 JSON 并反序列化为强类型对象
3. **缓存**：首次读取后缓存，避免重复 IO
4. **动态值**：公差等依赖 Rhino 文档设置的值不在 JSON 中存储固定值，标记为 `"dynamic": true`，由读取类在运行时从 `RhinoDoc.ActiveDoc` 获取

## 规则

- 子文件夹结构与调用者对应（哪里调用，就建对应子文件夹）
- 纯数据定义，不包含逻辑处理
- 所有默认值、提示词、常数通过本层管理，不散落在其他层
- 每个数据文件配有同名 `.md` 说明文档，解释各字段含义
