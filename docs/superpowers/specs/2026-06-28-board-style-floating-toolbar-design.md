# 白板风格浮动工具栏主题设计

## 背景

当前 ICC 的浮动工具栏（FloatingToolbar）采用较小的图标+文字按钮样式，而白板模式下的工具栏（BoardToolbar）使用更大的圆角矩形按钮、蓝色选中态和分组分隔线，视觉上更醒目。用户希望浮动工具栏也能切换成白板工具栏的视觉风格。

## 目标

- 为所有浮动工具栏增加一个可切换的“白板风格”主题
- 默认保持现有样式，不影响老用户
- 只改视觉样式，不改按钮逻辑、注册、隐藏规则等行为
- 用户可在设置中开关

## 非目标

- 不替换为真正的 `BoardToolbarButton` 控件
- 不改浮动工具栏的拖拽、折叠、翻转、位置记忆等行为
- 不改 `IToolbarItem` 接口或工具栏配置文件格式
- 第一期不做按工具栏单独设置风格

## 设计方案

### 1. 新增设置项

- 在 `Properties/Settings.cs` 的合适 Settings 类中新增布尔属性：
  - 推荐放在 `Ink_Canvas.Properties.PowerPointSettings` 或新建 `AppearanceSettings` 下
  - 属性名：`UseBoardStyleFloatingToolbar`
  - 默认值：`false`

- 在 `Windows/SettingsViews/Pages/ToolbarAppearancePage.xaml` 增加一个 `ToggleSwitch`：
  - 标题：`使用白板风格工具栏`
  - 绑定到上述设置属性

- 设置变更通过现有的 `SettingsActionHub` 或事件机制通知 UI 刷新。

### 2. 视觉样式实现

采用 **A 方案：纯样式切换**。

- 扩展 `Controls/Toolbar/FloatingToolbar/Items/ToolbarImageButton.cs` 或对应的 XAML 样式，支持两套 ControlTemplate：
  - **默认模板**：现有的小按钮 + 图标下方文字
  - **白板风格模板**：
    - 更大的圆角矩形按钮区域（例如 52×52 或 56×56）
    - 图标与文字垂直居中
    - 选中态使用蓝色背景（参考白板工具栏）
    - 支持分组分隔线（在按钮组之间插入 `Separator` 或带分隔视觉的容器）

- 通过附加属性 `ToolbarRegistry.UseBoardStyleProperty` 在构建视图时决定使用哪套模板。
  - 该附加属性可沿用现有 `UseRedStyleProperty` 的模式添加。

- 把尺寸、圆角、间距、颜色等资源抽到 XAML 资源字典，便于后续用户自定义或支持暗色模式。

### 3. 构建与刷新

- 在 `ToolbarRegistry.BuildView` 中读取全局设置，根据 `UseBoardStyleFloatingToolbar` 的值设置 `UseBoardStyleProperty`。
- 所有已创建的工具栏在设置变更时收到通知，重新生成视图或动态切换按钮模板。
- 新增、删除、重新排序按钮时自动应用当前主题。

### 4. 不变的东西

- `IToolbarItem` 接口与所有实现类
- 工具栏配置文件（JSON）格式
- 按钮点击事件、命令绑定、禁用/启用逻辑
- 浮动工具栏的位置、拖拽、自动翻转、隐藏规则
- 红色/高对比等特殊样式逻辑（在新增白板风格附加属性的同时保留）

## 影响范围

- `Controls/Toolbar/FloatingToolbar/Items/ToolbarImageButton.cs` 及对应样式
- `Controls/Toolbar/FloatingToolbar/ToolbarRegistry.cs`
- `Controls/Toolbar/ToolbarHost.cs` 或视图刷新入口
- `Properties/Settings.cs`
- `Windows/SettingsViews/Pages/ToolbarAppearancePage.xaml` / `.xaml.cs`
- 可能需要新增 XAML 资源字典文件

## 验收标准

- [ ] 默认情况下浮动工具栏保持原样
- [ ] 在设置中打开“使用白板风格工具栏”后，所有浮动工具栏立即切换为大按钮、圆角矩形、蓝色选中态风格
- [ ] 切换设置时，已显示的浮动工具栏无需重启即可刷新
- [ ] 按钮点击、禁用、分组显示正常
- [ ] 不影响 PPT 联动、隐藏规则、拖拽等行为

## 风险与回退

- 风险：XAML 模板切换可能导致按钮布局或文字换行异常
  - 缓解：先在小范围（主浮动栏）验证，再应用到所有浮动栏
- 风险：设置刷新机制不完善导致切换后部分工具栏未更新
  - 缓解：复用现有 `SettingsActionHub` 广播刷新
- 回退：如出现问题，将 `UseBoardStyleFloatingToolbar` 默认设为 `false` 即可恢复原有行为
