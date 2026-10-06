# Silent Ledger

A story-driven first-person military shooter for Android and iOS phones (landscape, touch controls).

- **Design:** [docs/campaign-design.md](docs/campaign-design.md), a snapshot of the campaign design doc
- **Unity project:** [SilentLedger/](SilentLedger/), Unity 6000.6.4f1, URP, Android target
- **Current goal:** a playable Chapter 0 · First Light

## Local setup

| Tool | Path |
| --- | --- |
| Unity editor | `A:\Unity\Editors\6000.6.4f1` |
| Android SDK | `A:\app\tools\android-sdk` |
| Android NDK | `A:\app\tools\android-sdk\ndk\27.2.12479018` (r27c, required by Unity 6.6) |
| CMake | `A:\app\tools\android-sdk\cmake\3.22.1` (required for Android builds) |
| JDK | `A:\app\tools\jdk-17` |

Open `SilentLedger/` from Unity Hub. Unity rebuilds `Library/` on first open.

## Scenes

The grey-box scenes are generated from code in `SilentLedger/Assets/_Project/Editor`. Re-run the menu item after changing a builder; don't hand-edit the generated scenes or the `PlayerRig` prefab.

| Scene | Menu item | Contents |
| --- | --- | --- |
| `Ch0_FirstLight` | Silent Ledger > Build Chapter 0 First Light | Site Hollow and the 0.1 Arrival mission |
| `Ch0_TestRange` | Silent Ledger > Build Test Range | Sandbox for the player and weapons |

Binary assets (textures, models, audio) are stored with Git LFS: run `git lfs install` once after cloning.

## Unity Asset Store content (not in git)

This repo is public and the Asset Store licence doesn't allow redistributing source assets, so
`SilentLedger/Assets/_Project/ThirdParty/` is git-ignored. On a new machine:

1. Download **Guns Pack: Low Poly Guns Collection** (Fun Assets) from the Asset Store via Package Manager > My Assets.
2. Copy the `pistol3` and `assault4` folders (FBX + PNGs, with their `.meta` files) from the package into
   `SilentLedger/Assets/_Project/ThirdParty/LowPolyGuns/`.
3. Run **Silent Ledger > Art > Set Up Weapons**, then rebuild the scenes.

Without these files the builders fall back to grey-box guns.
