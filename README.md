# OtogiMod-Android

本版本针对 [LemonLoader v0.7.4-android.3](https://github.com/LemonLoaderX/LemonLoader/releases/tag/v0.7.4-android.3) 构建，Mod 目标为 `net10.0`，需要 .NET 10 SDK；加载器运行时为 .NET 11，历史 `net6` 目录仍作为引用和安装路径。Loader 编译 DLL 来自正式发行包，不随 Mod ZIP 部署。使用 Patcher 安装该加载器时需 2.1.0 或更新版；从原始游戏 APK 准备新 payload，保留包名、签名及用户数据，勿混用旧运行时。游戏 Interop 保持各项目现有导出，运行验证需使用对应游戏版本。

Otogi Frontier 的 LemonLoader Android 模组。

## 功能

- Spine 马赛克效果优化
- Otogi 翻译数据与字体资源替换
- 60 FPS 锁定

## 环境

- Android ARM64
- Unity IL2CPP
- LemonLoader 或兼容的 MelonLoader Android 环境

## 构建

标准方案为 `OtogiMod-Android.slnx`。共享 Utility 和 ModEngineering
通过固定 Git 子模块构建；本地共享源码联调可复制
`SharedDependencies.local.props.example` 为 `SharedDependencies.local.props`。

```sh
git submodule update --init --recursive
python shared/ModEngineering/scripts/project.py check
python shared/ModEngineering/scripts/project.py test
python shared/ModEngineering/scripts/project.py package
```

输出位于 `artifacts/release/v<version>/`。安装 ZIP 时不要覆盖加载器或游戏 Interop 文件。

解压 `OtogiMod-Android.zip` 到加载器 base 目录，保留 `Mods` 路径。普通版和 Dev 版
使用相同的程序集名和配置文件，只安装其中一个版本。

## 配置

配置文件为 `UserData/OtogiMod.cfg`，首次启动时生成。翻译字体和 CDN 地址位于
`OtogiMod.Translation`，仅使用 `CDN`，不读取 `CDN_HK`。

字体 URL 在 `HTTPManager.SendRequest(HTTPRequest)` 前改写，不补丁生成的
`HTTPRequest` 构造函数：其包装代码会分配新对象，复制执行可能使游戏原请求未初始化。
