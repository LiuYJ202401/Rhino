# Assets 目录

存储运行时需要的二进制资源文件（图片、网格、纹理等）。与 [Data](../Data/README.md) 目录对称：

| 目录 | 内容 | 格式 | 用途 |
|------|------|------|------|
| `Data/` | 软编码参数、默认值 | JSON | 配置数据，程序运行时读取 |
| `Assets/` | 二进制资源 | PNG/OBJ/CSV 等 | 测试和运行时所需的媒体文件 |

## 目录结构

只有项目层（Project）会使用资源，因此直接按测试链组织：

```
Assets/
├── Test/
│   └── test_heightfield.png   ← TestChain2 步骤17 Heightfield 测试用图
└── README.md                   ← 本说明
```

## 部署规则

csproj 通过通配符递归复制到插件输出目录：

```xml
<Content Include="Assets\**\*.*" CopyToOutputDirectory="PreserveNewest" />
```

- **不限文件类型**：PNG/JPG/OBJ/CSV 等均可
- **不限目录层级**：可在 Assets 下创建子目录组织资源
- **保留目录结构**：`Assets\Test\a.png` 复制到 `输出目录\Assets\Test\a.png`

## 代码查找方式

```csharp
string assemblyDir = Path.GetDirectoryName(
    System.Reflection.Assembly.GetExecutingAssembly().Location);
string assetPath = Path.Combine(assemblyDir, "Assets", "Test", "文件名");
```

## 添加新资源的步骤

1. 把资源文件放入 `Assets/Test/` 目录（可创建子目录）
2. 直接在代码中用 `Path.Combine(assemblyDir, "Assets", "Test", "相对路径")` 查找
3. **无需修改 csproj**——通配符会自动包含
