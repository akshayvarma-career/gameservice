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
| JDK | `A:\app\tools\jdk-17` |

Open `SilentLedger/` from Unity Hub. Unity rebuilds `Library/` on first open.

Binary assets (textures, models, audio) are stored with Git LFS: run `git lfs install` once after cloning.
