# Curve 命令默认值

命名空间：`Rh.Data.Command.BasicCommand`

## 目的

存储 Curve 基础命令中各创建方法调用时的默认参数值。

## 数据文件

- **JSON 文件**：`Curve.json`（程序运行时读取）
- **说明文档**：本文件（解释各字段含义）

## 字段说明

### 通用字段

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `tolerance` | dynamic double | `ModelAbsoluteTolerance` | 公差，标记 dynamic，运行时从 RhinoDoc 读取 |
| `angleTolerance` | dynamic double | `ModelAngleToleranceRadians` | 角度公差，同上 |

### 线与多段线

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreatePolyline | `closed` | bool | false | 默认不闭合 |

### 圆与圆弧

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateArc | `startAngle` | double | 0.0 | 起始角（弧度） |
| CreateArc | `endAngle` | double | 1.5708 | 终止角（弧度，90°） |

### 圆锥曲线

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateConic | `rho` | double | 0.5 | rho 值，0.5=抛物线 |

### 自由曲线

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateNurbsCurve | `degree` | int | 3 | NURBS 曲线默认阶数 |
| CreateNurbsCurve | `periodic` | bool | false | 默认非周期 |
| CreateNurbsCurveAdvanced | `degree` | int | 3 | 同上 |
| CreateNurbsCurveAdvanced | `weights` | double[] | 全 1.0 | 默认无权（非有理） |
| CreateInterpCrv | `degree` | int | 3 | 插值曲线默认阶数（必须奇数） |
| CreateInterpCrv | `knots` | string | "Uniform" | CurveKnotStyle 枚举名 |
| CreateInterpCrv | `startTangent` | string | "Unset" | 不指定时为 Unset |
| CreateInterpCrv | `endTangent` | string | "Unset" | 不指定时为 Unset |
| CreateHandleCurve | `closed` | bool | false | 默认不闭合 |
| CreateCurveThroughPt | `degree` | int | 3 | 拟合曲线默认阶数 |
| CreateCurveThroughPt | `periodic` | bool | false | 默认非周期 |
| CreateCurveThroughPt | `tolerance` | dynamic double | `ModelAbsoluteTolerance` | 标记 dynamic |
| CreateCurveThroughPt | `startTangent` | string | "Unset" | 不指定 |
| CreateCurveThroughPt | `endTangent` | string | "Unset" | 不指定 |

### 螺旋线

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateHelix | `startRadius` | double | 1.0 | 起始半径 |
| CreateHelix | `endRadius` | double | 1.0 | 终止半径（等径） |
| CreateHelix | `pitch` | double | 1.0 | 每圈高度 |
| CreateSpiral | `startRadius` | double | 1.0 | 起始半径 |
| CreateSpiral | `endRadius` | double | 5.0 | 终止半径 |

### 从对象提取

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateDividePoints | `segmentCount` | int | 2 | 默认 2 段（中点） |

## JSON 格式示例

```json
{
  "common": {
    "tolerance": { "dynamic": true, "source": "ModelAbsoluteTolerance" },
    "angleTolerance": { "dynamic": true, "source": "ModelAngleToleranceRadians" }
  },
  "CreatePolyline": {
    "closed": false
  },
  "CreateArc": {
    "startAngle": 0.0,
    "endAngle": 1.5707963267948966
  },
  "CreateConic": {
    "rho": 0.5
  },
  "CreateNurbsCurve": {
    "degree": 3,
    "periodic": false
  },
  "CreateInterpCrv": {
    "degree": 3,
    "knots": "Uniform",
    "startTangent": "Unset",
    "endTangent": "Unset"
  },
  "CreateHelix": {
    "startRadius": 1.0,
    "endRadius": 1.0,
    "pitch": 1.0
  },
  "CreateSpiral": {
    "startRadius": 1.0,
    "endRadius": 5.0
  },
  "CreateDividePoints": {
    "segmentCount": 2
  }
}
```

## 实现说明

- 读取类 `DataReader<CurveDefaults>` 在首次调用时加载 `Curve.json` 并缓存
- `dynamic` 字段由读取类解析 `"source"` 值，运行时从 `RhinoDoc.ActiveDoc` 动态获取
- 枚举类型（如 `CurveKnotStyle`）存储为字符串名称，读取时通过 `Enum.Parse` 转换
- `Vector3d.Unset` 等特殊值存储为字符串 `"Unset"`，读取时判断并赋值
