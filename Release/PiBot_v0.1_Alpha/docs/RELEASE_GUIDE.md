
# 📦 PiBot Release & Packaging Guide (v0.1 Alpha)

This document outlines the official process for packaging and distributing **PiBot Pro**. Following these steps ensures a consistent "out-of-the-box" experience for the user.

## 🏗️ The Release Architecture

The final distribution package is a portable directory (or a `.zip` archive) containing the pre-compiled binary heart of PiBot and its neural DNA.

### 📁 Directory Structure

```text
PiBot_v0.1_Alpha/
├── PiBotControlCenter.exe      # Main GUI & Orchestrator
├── PiBotTray.exe               # Background Monitoring & Quick Actions
├── PiBotInstaller.exe          # Setup Utility for C:\Program Files
├── README.md                   # Version info & Official URL (pibot.club)
├── LICENSE                     # MIT License
├── /assets/                    # UI Images, Fonts, and Branding
├── /Data/                      # Neural DNA (cloud-init.yaml)
├── /web/                       # Local Control Dashboard (HTML/JS)
└── /docs/                      # Technical Manifests
```

## 🚀 The Build Solution: `generate_release.ps1`

We use a unified PowerShell script to automate the compilation and packaging.

### What the script does

1. **Validates Environment**: Checks if `.NET` and `Multipass` are present.
2. **Compiles Core**: Uses `csc.exe` to build the native Windows binaries.
3. **Packages Assets**: Mirrors the directory structure above.
4. **Creates Distribution**: Generates a finalized `.zip` archive ready for upload to **[pibot.club](https://pibot.club/)**.

## 📥 Deployment to User

Users should be instructed to:

1. Verify **Multipass** is installed on their host machine.
2. Extract the ZIP.
3. Run `PiBotControlCenter.exe`.
4. Initiate the **Genesis DNA** to deploy their first neural node.

---
*PiBot: Simplifying local AI orchestration.*
