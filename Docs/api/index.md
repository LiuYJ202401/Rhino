# RhinoCommon API 参考

## 核心命名空间

### Rhino.Geometry - 几何类型

| 类型 | 说明 |
|------|------|
| Point3d | 三维点坐标 |
| Vector3d | 三维向量 |
| Line | 直线段 |
| Circle | 圆 |
| Arc | 圆弧 |
| Curve | 曲线基类 |
| NurbsCurve | NURBS 曲线 |
| Polyline | 多段线 |
| PolyCurve | 多重曲线 |
| Brep | 边界表示 (曲面/实体) |
| Surface | 曲面基类 |
| Plane | 构造平面 |
| Transform | 变换矩阵 (4x4) |
| Mesh | 网格 |
| Box | 包围盒 |
| BoundingBox | 轴对齐包围盒 |

### Rhino.Commands - 命令系统

| 类型 | 说明 |
|------|------|
| Command | 命令基类 - 继承此类创建自定义命令 |
| Result | 命令执行结果 (Success/Cancel/Failure/Nothing) |
| RunMode | 命令运行模式 (Interactive/Scripted) |
| CommandStyle | 命令样式属性 |

### Rhino.Input - 输入获取

| 类型 | 说明 |
|------|------|
| GetObject | 获取对象选择 |
| GetPoint | 获取点输入 |
| GetNumber | 获取数字输入 |
| GetInteger | 获取整数输入 |
| GetString | 获取字符串输入 |
| GetOption | 获取选项输入 |
| GetResult | 输入结果枚举 |
| ObjectType | 对象类型过滤器 |

### Rhino.DocObjects - 文档对象

| 类型 | 说明 |
|------|------|
| ObjRef | 对象引用 - 包含对象 ID 和组件信息 |
| ObjectAttributes | 对象属性 (图层、颜色、名称等) |
| RhinoObject | Rhino 对象基类 |
| RhinoDoc | Rhino 文档 |

### Rhino.Display - 显示管道

| 类型 | 说明 |
|------|------|
| DisplayConduit | 显示导管 - 自定义绘制 |
| DisplayPipeline | 显示管道 |
| DrawEventArgs | 绘制事件参数 |
| RhinoView | 视图 |
| DisplayMode | 显示模式 |

## 常用操作速查

### 创建几何

```csharp
// 创建点
Guid pointId = doc.Objects.AddPoint(new Point3d(0, 0, 0));

// 创建线
Guid lineId = doc.Objects.AddLine(new Point3d(0, 0, 0), new Point3d(10, 10, 10));

// 创建圆
Circle circle = new Circle(Plane.WorldXY, 5.0);
Guid circleId = doc.Objects.AddCircle(circle);

// 创建球体
Sphere sphere = new Sphere(Point3d.Origin, 10.0);
Guid sphereId = doc.Objects.AddSphere(sphere);
```

### 获取用户输入

```csharp
// 获取点
var getPoint = new GetPoint();
getPoint.SetCommandPrompt("选择点");
if (getPoint.Get() == GetResult.Point) {
    Point3d pt = getPoint.Point();
}

// 获取对象
var getObject = new GetObject();
getObject.SetCommandPrompt("选择曲线");
getObject.GeometryFilter = ObjectType.Curve;
if (getObject.Get() == GetResult.Object) {
    ObjRef objRef = getObject.Object(0);
    Curve curve = objRef.Geometry() as Curve;
}

// 获取数字
var getNumber = new GetNumber();
getNumber.SetCommandPrompt("输入半径");
getNumber.SetDefaultNumber(10.0);
if (getNumber.Get() == GetResult.Number) {
    double radius = getNumber.Number();
}
```

### 变换对象

```csharp
// 移动
Vector3d translation = new Vector3d(10, 0, 0);
Transform move = Transform.Translation(translation);
doc.Objects.Transform(objId, move);

// 旋转
Transform rotate = Transform.Rotation(Math.PI / 4, Vector3d.ZAxis, Point3d.Origin);
doc.Objects.Transform(objId, rotate);

// 缩放
Transform scale = Transform.Scale(Point3d.Origin, 2.0);
doc.Objects.Transform(objId, scale);
```

### Display Conduit 预览

```csharp
class PreviewConduit : DisplayConduit
{
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
    {
        base.CalculateBoundingBox(e);
        e.IncludeBoundingBox(m_bbox);
    }

    protected override void PostDrawObjects(DrawEventArgs e)
    {
        base.PostDrawObjects(e);
        e.Display.DrawLine(m_lineStart, m_lineEnd, Color.Red);
    }
}
```

## 相关资源

- [RhinoCommon API 文档（在线）](https://developer.rhino3d.com/api/)
- [RhinoCommon 源码（GitHub）](https://github.com/mcneel/rhinocommon)
