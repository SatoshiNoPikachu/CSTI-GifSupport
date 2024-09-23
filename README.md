# 动态图片支持（GIF Support）- CSTI MOD



## 简介

动态图片支持（GIF Support）是一个功能类Mod，为Mod作者提供动态图片的加载与播放支持。



当前版本：1.2.1

By.サトシの皮卡丘



## 安装说明

需前置Mod：ModLoader v2.0.1+

请将Mod压缩包解压于 BepInEx\plugins 文件夹下。



## 使用说明

Tips：该内容供Mod作者阅读。

Tips：以下内容除 “本Mod” 的表述是指 “动态图片支持” Mod外，其他 “Mod” 的表述皆指您所制作的 Mod。

**Tips：玩家需安装本Mod，才能使您的动态图片在游戏中加载与播放！**



### 安装 ModEditor-JsonData

将**本Mod**目录中的 “CSTI-JsonData” 文件夹复制到 ModEditor 的根目录中，即可使 ModEditor 支持动态图片定义。



### 添加动态图片资源

1. 在 Mod 的 “Resource” 文件夹中创建名为 “GIF” 的文件夹。
2. 将 “GIF” 格式文件放入 “GIF” 文件夹中。

Tips：动态图片的文件名是唯一标识，且与任何Mod共享，请注意防止命名冲突！



### 创建动态卡牌

1. 在 Mod 的 “ScriptableObject” 文件夹中创建名为 “GIF-Card” 的文件夹。
2. 在 ModEditor 中导入 Mod 项目。
3. 在左侧文件列表中找到并右击 “ScriptableObject/GIF-Card” 文件夹，在弹出菜单中选择“新建文件”。
4. 在弹出窗口中输入创建的动态卡牌名称（无实际作用），选择模板，并点击 “OK” 按钮即可完成创建。

Tips：动态卡牌名称虽然无实际作用，但仍然是唯一标识，且与任何Mod共享，请注意防止命名冲突！



### 绑定动态卡牌

1. 在 ModEditor 中打开创建好的动态卡牌。
2. 点击“隐藏未激活属性“按钮。
3. 右击 “Card” 字段，在弹出菜单中选择“添加引用”。
4. 选择要绑定的 CardData，点击 “OK” 按钮即可完成绑定。
5. 根据需求，填写 “CardGif”、“CardBackgroundGif” 等字段绑定动态图片。

Tips：每个 CardData 仅支持绑定一个动态卡牌，额外的绑定不会生效！



## 更新日志

### Version  1.2.1

修复绑定GIF的卡牌无法显示默认图像的问题。



### Version 1.2.0

1. GifPlaySet 增加了 Description 字段，用于附加描述文本。
2. CardDataGif 原 “耐久条件GIF” 字段更名为 “条件GIF”，并新增了状态条件字段。



### Version 1.1.0

新增功能：卡牌根据耐久条件切换动态图片。



### Version 1.0.1

1. 修改了 GifPlayer 的状态逻辑，现在默认状态为End，并且增加了 Reset() 方法，用于重置播放器。
2. GifPlaySet 增加了 Clear() 方法，用于清除应用在播放器上的设置。
3. 现在设置卡牌的动态图片时，若未获取到对应播放设置时，会清除当前播放设置。



## 使用的开源项目

[3DI70R-Unity-GifDecoder](https://github.com/3DI70R/Unity-GifDecoder?tab=MIT-1-ov-file) - v1.0.3