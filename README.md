# Rep0S2cLeak

> R.E.P.O. 游戏的 Unity/C# 反编译源码快照，850+ 个脚本文件，拿来读代码结构用的。
> Decompiled Unity/C# source snapshot of the game R.E.P.O. — 850+ script files for studying code structure.

## Overview / 概述

A source-code archive of **R.E.P.O.**, the co-op horror game. All C# scripts and project files were extracted/decompiled from the game's `Assembly-CSharp.dll`. This is not a buildable Unity project — engine DLLs and assets are not included — it's a snapshot for reading game architecture, naming patterns, module decomposition, and doing static analysis.

这是多人恐怖游戏 **R.E.P.O.** 的源码存档。所有 C# 脚本和工程文件都是从游戏的 `Assembly-CSharp.dll` 反编译/提取得来的。这不是一个能直接跑起来的 Unity 工程（引擎 DLL 和资产文件不包含在内），纯粹是一份代码结构阅读快照，用来看架构、看命名习惯、看模块拆分。

## Features / 功能

- **Enemy system / 敌人系统** — `EnemyDirector`, `EnemyParent`, `EnemyRigidbody`, 状态机 (`EnemyState*`), 各具体敌人实现 (`EnemyHunter`, `EnemyShadow`, `EnemyGnome`, `EnemyBang` ...)
- **Game flow / 游戏流程** — `GameDirector` (Load/Start/Main/Outro/End/Death 状态枚举), `GameManager` (含 Steam 季节性活动检测), `LevelGenerator`, `Map*`
- **Audio / 音频** — `AudioManager`, `Audial/` 与 `Audial.Utils/` 下的音效处理器 (压缩、失真、混响、镶边), TTS 相关命名空间
- **Networking / 网络** — Photon PUN2 多人同步与语音
- **UI & Debug / UI 与调试** — `ChatManager`, `DebugCommandHandler`, `DebugConsoleUI`, 各类 `*UI` 脚本
- **Valuables / 可拾取物** — `BlenderValuable`, `JackhammerValuable` 等物品逻辑
- **Speech synthesis / 语音合成** — Overtone TTS, Klattersynth

## Tech Stack / 技术栈

From `Assembly-CSharp.csproj`:

| Item | Detail |
|------|--------|
| Language | C# (LangVersion 12.0, AllowUnsafeBlocks) |
| Target | .NET Standard 2.1 (Unity Assembly-CSharp) |
| Engine | Unity (CoreModule, Physics, Animation, AI, UI, Video, ParticleSystem ...) |
| Networking | Photon (PUN, Realtime, Voice, Photon3Unity3D) |
| Platform | Facepunch.Steamworks.Win64, Discord.Sdk |
| TTS | Klattersynth, Overtone |
| UI | TextMeshPro, InputSystem |
| Other | AI.Navigation, Postprocessing, VisualScripting.Core |

`.csproj` 中的 `HintPath` 指向本地 Steam 安装目录下的 R.E.P.O. Managed DLL，这些 DLL 不在仓库里。

## Project Structure / 项目结构

```
.
├── Assembly-CSharp.csproj      # 工程文件 (引用 Unity/Photon/Steam DLL)
├── LICENSE                     # GPLv3
├── game_decompile.zip          # 反编译压缩包
├── Properties/AssemblyInfo.cs
├── Audial/                     # 音频效果器
├── Audial.Utils/               # 音频工具 (滤波器、包络、LFO)
├── LeastSquares.Overtone/      # Overtone TTS
├── Overtone.Scripts/           # Overtone 脚本
├── Assets.Overtone.Scripts/    # Overtone (旧 TTS)
├── Strobotnik.Klattersynth/    # Klattersynth 语音合成
└── *.cs                        # 850+ 顶层游戏脚本
```

<!-- TODO: confirm contents of game_decompile.zip -->

## Getting Started / 快速开始

This repo is for **reading**, not building.

这个仓库是拿来**读**的，不是拿来编译的。

1. Open the repo or `Assembly-CSharp.csproj` in your IDE (Visual Studio / Rider / VS Code), let it index.
2. Start from entry-point classes: `GameManager.cs`, `GameDirector.cs`, `LevelGenerator.cs`, `EnemyDirector.cs`.
3. Follow state machines (`EnemyState*`) and subsystems (`Audial/`, `Map*`, `*UI`) outward.
4. If you want to try compiling: supply the Unity/Photon/Steamworks/Discord DLLs yourself and fix the `HintPath` entries.

<!-- TODO: confirm whether compilation is intended/possible at all -->

## Status / 状态

历史存档 / 源码研究快照。不是活跃维护的可运行工程，就是存着当学习资料。

Archive / source study snapshot. Not actively maintained as a runnable project.

## License / 许可证

GPLv3 — see `LICENSE`.
