# Surface 命令默认值

对应接口文档：`Command/basicCommand/Surface.md`

## 数据格式

存储为 `Surface.json`，与本文档同目录。使用 `DataReader` 按字段读取。

## 字段说明

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateSrfPtGrid | degreeU | int | 3 | U 向阶数 |
| CreateSrfPtGrid | degreeV | int | 3 | V 向阶数 |
| CreateSrfControlPtGrid | degreeU | int | 3 | U 向阶数 |
| CreateSrfControlPtGrid | degreeV | int | 3 | V 向阶数 |
| CreateLoft | loftType | enum (LoftType) | Normal | 放样类型 |
| CreateLoft | closed | bool | false | 是否闭合 |
| CreateNetworkSrf | tolerance | double | dynamic:ModelAbsoluteTolerance | 边界公差 |
| CreateNetworkSrf | interiorTolerance | double | dynamic:ModelAbsoluteTolerance | 内部公差 |
| CreateNetworkSrf | angleTolerance | double | dynamic:ModelAngleToleranceRadians | 角度公差 |
| CreateSweep1 | closed | bool | false | 是否闭合 |
| CreateSweep1 | rebuildType | enum (SweepRebuild) | None | 重建类型 |
| CreateSweep1 | miterType | enum (SweepMiterType) | Natural | 拐角类型 |
| CreateSweep2 | closed | bool | false | 是否闭合 |
| CreateSweep2 | rebuildType | enum (SweepRebuild) | None | 重建类型 |
| CreateSweep2 | maintainHeight | bool | true | 保持高度 |
| CreateRevolve | startAngle | double | 0.0 | 起始角（弧度） |
| CreateRevolve | endAngle | double | 6.283185307179586 | 终止角（弧度，2π） |
| CreateExtrudeAlongCrv | cap | bool | false | 是否加盖 |
| CreateExtrudeTapered | cap | bool | false | 是否加盖 |
| CreateExtrudeToPoint | cap | bool | false | 是否加盖 |
| CreatePatch | uSpans | int | 10 | U 向跨度数 |
| CreatePatch | vSpans | int | 10 | V 向跨度数 |
| CreatePatch | trim | bool | true | 是否自动修剪 |
| CreatePatch | pointSpacing | double | 0.1 | 采样点间距 |
| CreatePatch | flexibility | double | 1.0 | 柔度 |
| CreatePatch | surfacePull | double | 1.0 | 起始曲面拉力 |
| CreatePatch | fixEdges | bool | false | 固定边缘 |
| CreateRibbon | cornerType | enum (CornerType) | None | 角类型 |
| CreateRibbon | bothSides | bool | false | 两侧 |
| CreateDrape | uSpacing | int | 10 | U 向采样间距 |
| CreateDrape | vSpacing | int | 10 | V 向采样间距 |
| CreateHeightfield | samplesX | int | 50 | X 向采样数 |
| CreateHeightfield | samplesY | int | 50 | Y 向采样数 |

## JSON 示例

```json
{
  "CreateSrfPtGrid": {
    "degreeU": 3,
    "degreeV": 3
  },
  "CreateSrfControlPtGrid": {
    "degreeU": 3,
    "degreeV": 3
  },
  "CreateLoft": {
    "loftType": "Normal",
    "closed": false
  },
  "CreateNetworkSrf": {
    "tolerance": { "dynamic": true, "source": "ModelAbsoluteTolerance" },
    "interiorTolerance": { "dynamic": true, "source": "ModelAbsoluteTolerance" },
    "angleTolerance": { "dynamic": true, "source": "ModelAngleToleranceRadians" }
  },
  "CreateSweep1": {
    "closed": false,
    "rebuildType": "None",
    "miterType": "Natural"
  },
  "CreateSweep2": {
    "closed": false,
    "rebuildType": "None",
    "maintainHeight": true
  },
  "CreateRevolve": {
    "startAngle": 0.0,
    "endAngle": 6.283185307179586
  },
  "CreateExtrudeAlongCrv": {
    "cap": false
  },
  "CreateExtrudeTapered": {
    "cap": false
  },
  "CreateExtrudeToPoint": {
    "cap": false
  },
  "CreatePatch": {
    "uSpans": 10,
    "vSpans": 10,
    "trim": true,
    "pointSpacing": 0.1,
    "flexibility": 1.0,
    "surfacePull": 1.0,
    "fixEdges": false
  },
  "CreateRibbon": {
    "cornerType": "None",
    "bothSides": false
  },
  "CreateDrape": {
    "uSpacing": 10,
    "vSpacing": 10
  },
  "CreateHeightfield": {
    "samplesX": 50,
    "samplesY": 50
  }
}
```

## 实现说明

- 所有公差类参数（tolerance/interiorTolerance/angleTolerance）为动态值，运行时从 `RhinoDoc.ActiveDoc` 获取
- 枚举值存储为字符串（如 "Normal"），由 `DataReader.ParseEnum<T>()` 转换
- 用户通过命令交互修改任何可选参数后，实际使用的值自动更新到本文件
