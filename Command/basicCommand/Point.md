# Point 命令接口文档

命名空间：`Rh.Cmd.BasicCommand`

## 功能

创建点和点云对象（不写入文档，返回几何对象）。

## 默认值机制

- 标注 `[可选]` 的参数可不传入，Command 层自动从 Data 层读取默认值
- 命令执行后，实际使用的值自动更新为新的默认值
- 对应默认值定义见 `Data/Command/basicCommand/Point.md`
- Point 命令参数较简单，大多为必需参数

## 命令列表

### CreatePoint

对应 Rhino 命令：`Point`

创建单个点。提供两个重载。

**重载 1：Point3d**

| 项目 | 说明 |
|------|------|
| 功能 | 从 Point3d 创建点 |
| 输入 | `Point3d point` — 三维坐标 |
| 输出 | `Point3d` — 创建的点 |
| 报错 | 输入无效（`!point.IsValid`）时返回 `Point3d.Unset` |
| RhinoCommon | Point3d 本身即为坐标值，无需构造 |

**重载 2：三分量坐标**

| 项目 | 说明 |
|------|------|
| 功能 | 从三个坐标分量创建点 |
| 输入 | `double x` — X 坐标，`double y` — Y 坐标，`double z` — Z 坐标 |
| 输出 | `Point3d` — 创建的点 |
| 报错 | NaN 或无穷大时返回 `Point3d.Unset` |
| RhinoCommon | `new Point3d(x, y, z)` |

### CreatePoints

对应 Rhino 命令：`Points`

| 项目 | 说明 |
|------|------|
| 功能 | 创建多个独立点 |
| 输入 | `IEnumerable<Point3d> points` — 点集合 |
| 输出 | `List<Point3d>` — 点列表 |
| 报错 | 空集合时返回空列表；无效点被过滤 |

### CreatePointGrid

对应 Rhino 命令：`PointGrid`

| 项目 | 说明 |
|------|------|
| 功能 | 在指定平面创建矩形点阵 |
| 输入 | `Plane plane` — 所在平面，`int xCount` — X 方向数量，`int yCount` — Y 方向数量，`Interval xDomain` — X 方向范围，`Interval yDomain` — Y 方向范围 |
| 输出 | `PointCloud` — 点云（包含 xCount × yCount 个点） |
| 报错 | xCount/yCount < 1 时返回 null |

### CreatePointCloud

对应 Rhino 命令：`PointCloud`

| 项目 | 说明 |
|------|------|
| 功能 | 将多个点组合为点云对象 |
| 输入 | `IEnumerable<Point3d> points` — 点集合 |
| 输出 | `PointCloud` — 点云对象 |
| 报错 | 空集合时返回 null |

### CreatePointCloudFromMesh

对应 Rhino 命令：`PointCloud`（选择网格时）

| 项目 | 说明 |
|------|------|
| 功能 | 从网格顶点创建点云 |
| 输入 | `Mesh mesh` — 源网格 |
| 输出 | `PointCloud` — 点云对象 |
| 报错 | mesh 无效时返回 null |

### AddPointsToCloud

对应 Rhino 命令：`PointCloud` > Add

| 项目 | 说明 |
|------|------|
| 功能 | 向已有点云添加点 |
| 输入 | `PointCloud cloud` — 目标点云，`IEnumerable<Point3d> points` — 待添加点 |
| 输出 | `PointCloud` — **新点云（原点云不变，返回副本）** |
| 约束 | **必须接收返回值**：`cloud = AddPointsToCloud(cloud, pts);`，否则原 cloud 不变 |
| 报错 | cloud 或 points 为 null 时返回原 cloud |

### RemovePointsFromCloud

对应 Rhino 命令：`PointCloud` > Remove

| 项目 | 说明 |
|------|------|
| 功能 | 从点云中移除指定索引的点 |
| 输入 | `PointCloud cloud` — 目标点云，`IEnumerable<int> indices` — 待移除点索引 |
| 输出 | `PointCloud` — **新点云（原点云不变，返回副本）** |
| 约束 | **必须接收返回值**：`cloud = RemovePointsFromCloud(cloud, idx);`，否则原 cloud 不变 |
| 报错 | 索引越界时跳过该索引 |

### ReducePointCloud

对应 Rhino 命令：`ReducePointCloud`

| 项目 | 说明 |
|------|------|
| 功能 | 从点云中随机删除指定数量的点（抽稀） |
| 输入 | `PointCloud cloud` — 源点云，`int removeCount` — 要删除的点数 |
| 输出 | `PointCloud` — 抽稀后的点云 |
| 报错 | removeCount < 0 或 ≥ 点数时返回原 cloud 副本 |

## 备注

以下 Rhino 点命令归入其他功能区：

| Rhino 命令 | 归属 | 原因 |
|------------|------|------|
| `ClosestPt` | Analyze | 求最近点，属分析计算 |
| `CrvStart` / `CrvEnd` | Curve | 从曲线提取端点 |
| `Divide` | Curve | 沿曲线等分生成点 |
| `ExtractPt` | Edit | 提取控制点/顶点 |
| `MarkFoci` | Analyze | 标注圆锥曲线焦点 |
| `DrapePt` | Surface | 在曲面上投影生成点阵 |
