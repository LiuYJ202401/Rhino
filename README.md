# 基于`C#`的`Rhino8`参数化编程学习

### 项目介绍

本项目基于`C#`进行`Rhino8`参数化设计的学习，开发插件，从而绕开繁琐的`grosshopper`的电池连接。

### 环境配置

##### 项目环境

本项目基于`Visual Studio Community 2022`进行开发，通过[github的链接](https://github.com/mcneel/RhinoVisualStudioExtensions/releases)或根目录里的`.vsix`文件运行对`VS 2022`的修改，增加`Rhino 8 Plug-In(C#)`文件的模板。

新建该模板类型的项目，在**解决方案资源管理器**中右键项目名称，选择**添加->项目引用**，添加`Rhino 8`安装目录**.../Rhino 8/System/**内的文件`RhinoCommon.dll`,`Rhino.YI.dll`,`Eto.dll`。

修改

##### `Rhino8`设置

在`Rhino8`的命令行中输入`ScriptEditor`，打开`shell.py`文件，在本文件中进行编程，在`Rhino8`环境中运行即可。

##### 环境问题

- `.vscode\settings.json`中的路径在本地电脑上不存在？

未初始化，在`Rhino8`的命令行中输入`ScriptEditor`进行初始化。

- `shell.py`文件无法运行？

文件无法直接运行，要在`Rhino8`环境中打开并在`ScriptEditor`中运行。

- `QuicAct`中的函数无法调用？

`Rhino8`使用了临时的虚拟环境，无法依据相对路径调用函数，应该复制粘贴到`shell.py`文件中调用。

### 学习内容

