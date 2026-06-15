# Point 命令默认值

命名空间：`Rh.Data.Command.BasicCommand`

## 目的

存储 Point 基础命令中各创建方法调用时的默认参数值。

## 数据文件

- **JSON 文件**：`Point.json`（程序运行时读取）
- **说明文档**：本文件（解释各字段含义）

## 字段说明

Point 命令参数较简单，均为必需参数（坐标、索引），无可选默认值。

| 命令 | 说明 |
|------|------|
| CreatePoint | 无默认值，坐标为必需输入 |
| CreatePoints | 无默认值 |
| CreatePointGrid | 无默认值，平面/数量/范围为必需输入 |
| CreatePointCloud | 无默认值 |
| CreatePointCloudFromMesh | 无默认值 |
| AddPointsToCloud | 无默认值 |
| RemovePointsFromCloud | 无默认值 |
| ReducePointCloud | `removeCount` 由用户指定，无默认值 |

## JSON 格式示例

```json
{
  "common": {
    "tolerance": { "dynamic": true, "source": "ModelAbsoluteTolerance" }
  }
}
```

## 实现说明

- 读取类 `DataReader<PointDefaults>` 在首次调用时加载 `Point.json` 并缓存
- Point 命令无可选默认值，JSON 文件仅包含通用字段（公差等）
- `dynamic` 字段由读取类解析 `"source"` 值，运行时从 `RhinoDoc.ActiveDoc` 动态获取
