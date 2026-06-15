# 插件用户数据

## 概述

插件有两种方式在 Rhino .3dm 文件中存储信息：
1. 文档用户数据
2. 对象用户数据

## 文档用户数据

覆盖 PlugIn 的三个方法：

```csharp
public override bool ShouldCallWriteDocument()
{
    return true;
}

public override void WriteDocument(RhinoDoc doc, BinaryWriter writer)
{
    writer.Write(myDataString);
}

public override void ReadDocument(RhinoDoc doc, BinaryReader reader)
{
    myDataString = reader.ReadString();
}
```

## 对象用户数据

三种形式：
1. **User Strings** - 用户字符串
2. **UserDictionary** - 用户字典
3. **Custom UserData** - 自定义用户数据

### User Strings

```csharp
// 设置
obj.Attributes.SetUserString("MyKey", "MyValue");

// 获取
string value = obj.Attributes.GetUserString("MyKey");
```

### UserDictionary

```csharp
// 获取或创建字典
var dict = obj.Attributes.UserData.GetOrCreateDictionary();

// 存储数据
dict.Set("MyNumber", 42.0);
dict.Set("MyString", "Hello");

// 读取数据
if (dict.TryGetDouble("MyNumber", out double num))
{
    RhinoApp.WriteLine($"Number: {num}");
}
```

### Custom UserData

```csharp
[Guid("DAAA9791-01DB-4F5F-B89B-4AE46767C783")]
public class PhysicalData : UserData
{
    public double Density { get; set; }
    public double Mass { get; set; }

    public override bool ShouldWrite => true;

    public override bool Read(BinaryReader binaryReader)
    {
        Density = binaryReader.ReadDouble();
        Mass = binaryReader.ReadDouble();
        return true;
    }

    public override bool Write(BinaryWriter binaryWriter)
    {
        binaryWriter.Write(Density);
        binaryWriter.Write(Mass);
        return true;
    }
}

// 使用
var obj = doc.Objects.Find(id);
obj.Attributes.UserData.Add(new PhysicalData { Density = 7.85, Mass = 100.0 });
```
