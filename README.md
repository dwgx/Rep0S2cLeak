# Rep0S2cLeak

**R.E.P.O. 反编译源码快照 | R.E.P.O. Decompiled Source Snapshot**

---

## Overview

A full decompiled source archive of the Unity game **R.E.P.O.** — 850+ C# scripts covering gameplay logic, enemy AI, audio systems, networking, and more. This is a read-only code snapshot for studying how the game is built internally. It is not a runnable Unity project; engine DLLs and assets are not included.

## 概述

R.E.P.O. 游戏的完整反编译源码存档，850+ 个 C# 脚本，覆盖游戏逻辑、敌人 AI、音频系统、网络同步等。这是一份用来研究游戏内部实现的只读代码快照，不是可运行的 Unity 工程——引擎 DLL 和游戏资产未包含在内。

---

## Features / 功能

- **Enemy AI system** — Full state-machine architecture (`EnemyState*`), director logic (`EnemyDirector`), physics (`EnemyRigidbody`), and individual enemy implementations (Hunter, Shadow, Gnome, Bang, etc.)
- **Game flow** — `GameDirector` with Load/Start/Main/Outro/End/Death states, `GameManager` with Steam seasonal event detection, `LevelGenerator`, map systems
- **Audio pipeline** — `AudioManager`, `Audial/` effects chain (compression, distortion, reverb, flanging), Overtone TTS, Klattersynth speech synthesis
- **Networking** — Photon Unity Networking (PUN), Photon Voice, Photon Realtime for multiplayer and voice chat
- **Platform integration** — Steamworks (Facepunch), Discord SDK
- **UI & debug** — `ChatManager`, `DebugCommandHandler`, `DebugConsoleUI`, various `*UI` scripts
- **Valuables / pickups** — `BlenderValuable`, `JackhammerValuable`, etc.

---

## Tech Stack / 技术栈

From `Assembly-CSharp.csproj`:

| Item | Detail |
|------|--------|
| Language | C# (LangVersion 12.0, `AllowUnsafeBlocks`) |
| Target | .NET Standard 2.1 (Unity Assembly-CSharp) |
| Engine | Unity (CoreModule, Physics, Animation, AI, UI, Video, ParticleSystem) |
| Networking | Photon (PUN, Realtime, Voice, Photon3Unity3D) |
| Platform | Facepunch.Steamworks.Win64, Discord.Sdk |
| UI | TextMeshPro, InputSystem |
| Audio | Klattersynth, Audial (custom) |
| AI/Nav | Unity.AI.Navigation |
| Visual | PostProcessing, VisualScripting.Core |

> `.csproj` 中的 `HintPath` 指向本地 Steam 安装目录中的 Managed DLL，这些 DLL 不在仓库内。
> The `HintPath` entries point to local Steam install Managed DLLs — not included in the repo.

---

## Project Structure / 项目结构

```
.
├── Assembly-CSharp.csproj      # Project file (references local Unity/Photon/Steam DLLs)
├── LICENSE                     # GPLv3
├── game_decompile.zip          # Decompiled archive (~1.1 MB)
├── Properties/AssemblyInfo.cs
├── Audial/                     # Audio effects (compressor, distortion, reverb, flanger)
├── Audial.Utils/               # Audio utilities (filters, envelopes, LFO)
├── LeastSquares.Overtone/      # Overtone TTS
├── Overtone.Scripts/           # Overtone scripts
├── Assets.Overtone.Scripts/    # Overtone (legacy TTS)
├── Strobotnik.Klattersynth/    # Klattersynth speech synthesis
└── *.cs                        # 850+ top-level gameplay scripts
```
<!-- TODO: confirm contents of game_decompile.zip -->

---

## Getting Started / 快速开始

This repo is for **reading**, not building.

1. Open the repo or `Assembly-CSharp.csproj` in your IDE (Visual Studio / Rider / VS Code) and let it index.
2. Start from the entry points: `GameManager.cs`, `GameDirector.cs`, `LevelGenerator.cs`, `EnemyDirector.cs`.
3. Follow the state machines (`EnemyState*`) and subsystems (`Audial/`, `Map*`, `*UI`) from there.
4. If you want to attempt compilation, supply the Unity / Photon / Steamworks / Discord DLLs referenced in the `.csproj` and fix the `HintPath` entries yourself.

这个仓库是**看代码**用的，不是拿来跑的。

1. 用 IDE 打开仓库或 `Assembly-CSharp.csproj`，等全局索引完成。
2. 从入口类开始：`GameManager.cs`、`GameDirector.cs`、`LevelGenerator.cs`、`EnemyDirector.cs`。
3. 顺着状态机（`EnemyState*`）和子系统（`Audial/`、`Map*`、`*UI`）展开。
4. 想尝试编译的话，自己补齐 `.csproj` 里引用的 DLL 并修正 `HintPath`。

<!-- TODO: confirm whether compilation is intended/possible at all -->

---

## Status / 状态

Archive / source study snapshot. 存档 + 代码结构学习快照。不活跃维护。

---

## License / 许可证

**GNU General Public License v3.0** — see `LICENSE`.
