# 基于`python`的`Rhino8`参数化编程学习

### 项目介绍

本项目基于`Rhino8`自带的`python`接口进行参数化设计的学习，从而绕开繁琐的`grosshopper`的电池连接。

### 环境配置

##### 项目路径

修改`.vscode\settings.json`中所有的路径，将用户名改为自己电脑的用户名。

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

