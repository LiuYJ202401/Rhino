# Image 命令默认值

对应接口文档：`Command/basicCommand/Image.md`

## 数据格式

存储为 `Image.json`，与本文档同目录。使用 `DataReader` 按字段读取。

## 字段说明

| 命令 | 字段 | 类型 | 默认值 | 说明 |
|------|------|------|--------|------|
| CreateCircleFit | precision | int | 30 | 精细度（最短边方向的圆数量） |
| CreateCircleFit | unitSize | double | 10.0 | 像素到物理单位的缩放比（1 像素 = N 单位，如 100px 宽图片 → 1000 单位宽） |
| CreateCircleFit | minRadiusRatio | double | 0.1 | 最小半径比例（radius < maxRadius × 此值时跳过，避免近白区域生成微小圆） |

## JSON 示例

```json
{
  "CreateCircleFit": {
    "precision": 30,
    "unitSize": 10.0,
    "minRadiusRatio": 0.1
  }
}
```

## 实现说明

- `precision` 由用户在命令交互中指定，执行后自动更新为用户实际使用的值
- `unitSize` 和 `minRadiusRatio` 为内部参数，用户不直接修改，可通过编辑 JSON 文件调整
