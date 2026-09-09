# TODO 项目记忆

## 项目概况
TodoList：Avalonia 11.3.6 + MVVM（CommunityToolkit.Mvvm）+ .NET 10 桌面 TODO 应用，JSON 持久化。
- 结构：TodoList.slnx + src/TodoList/{Models,Services,ViewModels,Views,Assets}
- 排序规则：未完成 → 已完成 → 不需要完成，组内创建时间倒序
- 数据：exe 同目录 todos.json
- 应用图标：src/TodoList/Assets/icon.ico（多尺寸）+ icon.png，蓝白渐变对勾清单风格

## 构建命令（重要！）
**用户自己的 PowerShell/CMD**：正常，直接 `dotnet build` / `dotnet run --project src/TodoList`

**WorkBuddy bash（缺 APPDATA 等环境变量，必须带前缀）**：
```bash
cd "C:/Users/Administrator/Desktop/TODO" && MSYS2_ARG_CONV_EXCL='*' MSYS_NO_PATHCONV=1 env APPDATA='C:\Users\Administrator\AppData\Roaming' LOCALAPPDATA='C:\Users\Administrator\AppData\Local' 'ProgramFiles=C:\Program Files' 'ProgramFiles(x86)=C:\Program Files (x86)' 'ProgramW6432=C:\Program Files' dotnet build -c Debug
```
（把 dotnet build 换成任意 dotnet 命令同理；不带动2000前缀会报 `Value cannot be null. (Parameter 'path1')`）

## 运行
- exe：src/TodoList/bin/Debug/net10.0/TodoList.exe
- 测试数据修改后注意 bin 下的 todos.json 会跟随 exe（调试模式数据在 bin 目录）
