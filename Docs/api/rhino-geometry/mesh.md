# Mesh - 网格

## 命名空间
`Rhino.Geometry`

## 摘要
表示多边形网格几何体，由顶点和面组成。

## 构造函数

| 构造函数 | 说明 |
|---------|------|
| `Mesh()` | 空网格 |
| `Mesh(int vertexCount, int faceCount)` | 指定顶点和面数 |

## 属性

| 名称 | 类型 | 说明 |
|------|------|------|
| Vertices | MeshVertexList | 顶点列表 |
| Faces | MeshFaceList | 面列表 |
| Normals | MeshVertexNormalList | 法线列表 |
| TextureCoordinates | MeshTextureCoordinateList | UV 坐标 |
| VertexColors | MeshVertexColorList | 顶点颜色 |
| IsValid | bool | 是否有效 |
| IsClosed | bool | 是否封闭 |

## 方法

| 名称 | 返回值 | 说明 |
|------|--------|------|
| `Vertices.AddVertex(x,y,z)` | int | 添加顶点 |
| `Faces.AddFace(v0,v1,v2)` | int | 添加三角面 |
| `Faces.AddFace(v0,v1,v2,v3)` | int | 添加四边形面 |
| `Normals.ComputeNormals()` | void | 计算法线 |
| `UnifyNormals()` | bool | 统一法线方向 |
| `Append(Mesh)` | void | 合并网格 |
| `DuplicateMesh()` | Mesh | 复制网格 |

## 创建网格

```csharp
// 创建简单四面体
Mesh mesh = new Mesh();
mesh.Vertices.AddVertex(0, 0, 0);
mesh.Vertices.AddVertex(1, 0, 0);
mesh.Vertices.AddVertex(0, 1, 0);
mesh.Vertices.AddVertex(0, 0, 1);
mesh.Faces.AddFace(0, 1, 2);
mesh.Faces.AddFace(0, 1, 3);
mesh.Faces.AddFace(1, 2, 3);
mesh.Faces.AddFace(2, 0, 3);
mesh.Normals.ComputeNormals();
mesh.Compact();

// 添加到文档
Guid id = doc.Objects.AddMesh(mesh);
```

## 从几何创建网格

```csharp
// 从 Brep 创建
Mesh[] meshes = Mesh.CreateFromBrep(brep);

// 从曲面创建
Mesh mesh = Mesh.CreateFromSurface(surface);

// 分辨率控制
var mp = new MeshingParameters();
mp.RelativeTolerance = 0.5;
Mesh[] meshes = Mesh.CreateFromBrep(brep, mp);
```
