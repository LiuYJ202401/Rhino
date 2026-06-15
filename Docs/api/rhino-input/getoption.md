# GetOption - 获取选项

## 命名空间
`Rhino.Input.Custom`

## 摘要
获取用户从预定义选项中选择。

## 基本结构

```csharp
var getOption = new GetOption();
getOption.AddOption("Yes", "Y");
getOption.AddOption("No", "N");
getOption.AddOption("Cancel");

while (true)
{
    GetResult result = getOption.Get();
    
    if (result == GetResult.Option)
    {
        Option option = getOption.Option();
        
        switch (option.EnglishName)
        {
            case "Yes":
                // 处理 Yes
                break;
            case "No":
                // 处理 No
                break;
            case "Cancel":
                return Result.Cancel;
        }
    }
    else if (result == GetResult.Nothing)
    {
        // 默认选项
        break;
    }
    else
    {
        return Result.Cancel;
    }
}
```

## 带数值输入的选项

```csharp
var getOption = new GetOption();
getOption.AddOption("Radius");
double radius = 10.0;

while (true)
{
    getOption.SetDefaultInteger(1);
    GetResult result = getOption.Get();
    
    if (result == GetResult.Option)
    {
        // 处理选项
    }
    else if (result == GetResult.Number)
    {
        radius = getOption.Number();
        break;
    }
}
```
