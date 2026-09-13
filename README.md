# CactusPie.ContainerQuickLoot

**Send looted items straight into tagged containers — no extra dragging.**

A client plugin for **SPT 4.1.5** (Single Player Tushonka).

[English](README.md) · [Русский](readme_rus.md)

---

## Overview

Using **Ctrl+click** on a loot item moves it into a matching container marked with a `@loot` tag.

Example: tag a Documents case with `@loot` and keep it in your secure container. Keys you loot with Ctrl+click go into that case automatically.

The same happens for **loose loot** you pick up from the ground, shelves, and similar spots.

> Works **in raid only**. It does not run on your hideout stash.

**Plugin:** `CactusPie.ContainerQuickLoot`  
**Version:** 1.9.0 · **SPT:** 4.1.5 · **Type:** client plugin (BepInEx)

---

## Features

- Auto-sort Ctrl+click transfers into tagged containers
- Auto-sort loose loot pickups into tagged containers
- Optional stack merging (ammo, money, and similar)
- Numbered `@loot` tags for container priority
- Customizable tag keyword in the F12 menu

---

## Installation

1. Download `CactusPie.ContainerQuickLoot.dll`.
2. Copy it into your SPT folder:

```
BepInEx\plugins
```

3. Launch SPT. Open the **F12** menu and look for **Container quick loot setting (CQLv4)**.

---

## Usage

1. Put a container in your inventory (for example a Documents case).
2. Right-click the container and choose **Tag**.
3. Set the tag to `@loot`.
4. In raid, **Ctrl+click** a compatible item (such as a key), or pick up loose loot.
5. The item is placed in that container automatically.

Remove the `@loot` tag to stop using that container as a loot target.

---

## Priority tags

Add a number after `@loot` to control which container fills first. A **lower** number wins. No suffix is treated as `0`.

| Tag | Priority |
|---|---|
| `@loot` | 0 (highest) |
| `@loot1` | 1 |
| `@loot2` | 2 |
| `@loot10` | 10 |
| `@loot20` | 20 |

Examples:

- `@loot` is used before `@loot1`
- `@loot10` is used before `@loot20`
- If two containers share the same tag, the game picks one of them

---

## Settings (F12)

Section: **Container quick loot setting (CQLv4)**

| Setting | Default | What it does |
|---|---|---|
| Enable for Ctrl+click | On | Move items into tagged containers when you Ctrl+click them in the loot menu |
| Enable for loose loot | On | Move items you pick up from the world into tagged containers |
| Merge stacks | On | Merge stacks (money, ammo, etc.) when transferring into a tagged container |
| Merge stacks for non-loot containers | On | Merge stacks even when transferring into containers **without** a loot tag |
| Customize the keyword used to denote loot containers in tags | `@loot` | Change the tag word if you do not want `@loot` |

---

## Building from source

You need the **.NET Framework 4.7.2** SDK and a local **SPT 4.1.5** install.

1. Clone this repository.
2. Copy these assemblies into `CactusPie.ContainerQuickLoot\libs` (from your SPT client `Managed` folder, plus `spt-reflection.dll` from `BepInEx\plugins\spt`):

   - `Assembly-CSharp.dll`
   - `Comfort.dll`
   - `Comfort.Unity.dll`
   - `ItemComponent.Types.dll`
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `UnityEngine.InputLegacyModule.dll`
   - `spt-reflection.dll`

3. Launch SPT once to the main menu before copying `Assembly-CSharp.dll`, so the client assemblies are ready.
4. Build:

```bash
dotnet build CactusPie.ContainerQuickLoot/CactusPie.ContainerQuickLoot.csproj -c Release
```

The plugin DLL is written to `CactusPie.ContainerQuickLoot\bin\Release\net472\`.

---

## Credits

This plugin exists because of the original work and the people who kept it alive across SPT versions.

| Author | Role |
|---|---|
| [CactusPie](https://github.com/CactusPie) | Original mod |
| [reysonk](https://github.com/reysonk) | Support for recursive containers |
| [ArchmageTony](https://github.com/ArchmageTony) | 3.8.x port |
| [ShinCFN](https://dev.sp-tarkov.com/ShinCFN) | 3.10.x port |
| [bepis69](https://dev.sp-tarkov.com/bepis69) | 3.10.x reupload |
| [garlicbreadtcg](https://gitea.com/garlicbreadtcg) | 3.11.x port |

Thank you to CactusPie and everyone who maintained a fork.

---

## License

See the repository license file if one is included with the release you downloaded.
