# 基于`C#`的`Rhino8`参数化编程学习

### 项目介绍

本项目基于`C#`进行`Rhino8`参数化设计的学习，开发插件，从而绕开繁琐的`grosshopper`的电池连接。

### 环境配置

##### 项目环境

本项目基于`Visual Studio Community 2022`进行开发，通过[github的链接](https://github.com/mcneel/RhinoVisualStudioExtensions/releases)或根目录里的`.vsix`文件运行对`VS 2022`的修改，增加`Rhino 8 Plug-In(C#)`文件的模板。

新建该模板类型的项目，在**解决方案资源管理器**中右键项目名称，选择**添加->项目引用**，添加`Rhino 8`安装目录**.../Rhino 8/System/**内的文件`RhinoCommon.dll`,`Rhino.YI.dll`,`Eto.dll`。

修改**./Properties/launchSettings.json**的内容，将"executablePath"的内容修改为自己电脑上`Rhino.exe`的路径。

运行编译程序，等待`Rhino 8`自动打开载入插件，注意插件的名称不能使用已使用的ID（如"Rhino"等）

##### `Rhino 8`设置

在`Rhino 8`的命令行中输入程序中`EnglishName`对应的字符串，即可调用插件命令。

### 学习内容

##### 项目结构
本项目的文件结构可见[文件结构](./Orders.md)