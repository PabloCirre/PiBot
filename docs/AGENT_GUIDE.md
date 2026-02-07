# 🤖 PIBOT Developer & Agent Guide 🛠️

## 1. Project Architecture 🏗️

The PhoenixBOT ecosystem is designed to be a **Hybrid Orchestration Platform**. It uses a lightweight Windows C# client (`PiBotControlCenter.exe`) to command a fleet of heavy-duty Linux Virtual Machines (`Multipass`).

### Core Components

- **The Brain (C#)**: WPF Application. Handles UI, Process Management, and User Commands.
- **The Muscle (Linux)**: Specialized Ubuntu VM instances running XFCE + noVNC.
- **The Bridge (CLI)**: Commands flow from C# -> `multipass.exe` -> Linux Shell.

## 2. Directory Structure 📂

A clean workspace is a happy workspace. Follow this structure strictly:

```
PhoenixBOT/
├── src/                  # Source Code (The Brain)
│   ├── PiBotControlCenter.cs
│   └── PiBotInstaller.cs
├── Data/                 # Configuration & Assets (The Soul)
│   ├── cloud-init.yaml   # Linux Initialization Script
│   ├── pibot_names.csv   # Naming Database
│   └── pibot_colors.csv  # UI Themes
├── docs/                 # Knowledge Base (The Wisdom)
│   ├── AGENT_GUIDE.md    # YOU ARE HERE
│   ├── ARCHITECTURE.md
│   └── GOLDEN_IMAGE.md   # How to clone bots
├── assets/               # Binary Assets (Icons, Images)
├── scripts/              # Helper Scripts (PowerShell/Bash)
└── PiBotControlCenter.exe # Compiled Application
```

## 3. The "Golden Image" Workflow 🥇

This is the **Critical Path** for production. We do NOT install OS from scratch every time.

1. **Launch Master**: Run `pibot-golden-master`.
2. **Customize**: Install EVERYTHING (Chrome, VSCode, Drivers) inside it manually.
3. **Seal**: Run `sudo cloud-init clean` inside the VM.
4. **Clone**: All future bots are instant clones of this master disk.

## 4. Troubleshooting 🔧

- **"Connection Refused"**: The VM is running but `cloud-init` is still installing the GUI (~10 mins). **Wait**.
- **"Out of Memory"**: Each bot needs 1.5GB RAM. Do not launch more than your host PC can handle.
- **"Zombie Process"**: Running `multipass list` freezes? Run `multipass restart` via Admin PowerShell.

---
*Maintainer Note: Always check `docs/GOLDEN_IMAGE_FACTORY.md` before modifying the virtualization logic.*
