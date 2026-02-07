# 🏭 PIBOT "Golden Image" Factory

This document explains how to prepare and finalize the perfect base image.

## 1. The Goal 🎯

Create one perfect virtual machine (`pibot-golden-master`) with all software, themes, and tweaks pre-installed.
Future bots will be cloned from this, launching in **10 seconds** instead of 10 minutes.

## 2. Preparation (Automated)

✅ Launched `pibot-golden-master` (20GB Disk, 2GB RAM).
⏳ Wait for initial setup (XFCE + VNC) to complete (~5-8 mins).

## 3. Customization (Your Mission) 🛠️

Once the bot is ready (green in Control Center), connect via **CONNECT** and do the following:

1. **Install Essential Software**:

    ```bash
    sudo apt install -y chromium-browser git htop
    ```

2. **Customize Look & Feel**:
    - Change Wallpaper.
    - Set Dark Theme.
    - Add Desktop Shortcuts.
3. **Clean Up**:
    - Remove temporary files (`sudo apt clean`).
    - Clear logs.

## 4. Finalization (Seal the Deal)

Run this command inside the bot when finished:

```bash
sudo cloud-init clean
sudo shutdown -h now
```

This prepares the disk for cloning. Do not turn it back on manually!

## 5. Cloning Phase (Future)

We will update `PiBotControlCenter` to launch new bots using this finalized disk as the template.
