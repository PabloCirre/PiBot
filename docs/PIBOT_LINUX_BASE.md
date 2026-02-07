# PiBot Linux System Base (V4.1)

This document defines the standard software and configuration stack for all PiBot units. Every new birth follows this specification to ensure compatibility and tool availability.

## 🖥️ Operating System

- **Distribution:** Ubuntu 24.04 LTS (Noble Numbat)
- **Kernel:** Latest LTS via Multipass
- **User:** `pibot` (Sudoer, no password required)
- **Password:** `pibot`

## 🎨 Desktop Environment

- **XFCE4:** Lightweight desktop environment chosen for performance.
- **XFCE4-Goodies:** Standard utility pack.
- **Resolution:** 1440x900 (Native High Definition).

## 📡 Remote Access & Display

- **VNC Server:** TightVNC Server running on `:1` (Port 5901).
- **noVNC Proxy:** Web-based VNC client running on port **6080**.
- **Persistence:** Managed via `systemd` services (`pibot-vnc.service` and `pibot-novnc.service`) to ensure connection survives unit restarts.

## 🌐 Pre-installed Software

- **Web Browser:** Google Chrome (Stable) - Default browser for all tasks.
- **Utilities:** `wget`, `curl`, `ca-certificates`, `net-tools`, `dbus-x11`.
- **Drivers:** `libu2f-udev`, `libvulkan1`, `fonts-liberation`.

## ⚙️ Network Configuration

- **DNS Servers:**
  - 8.8.8.8 (Google)
  - 1.1.1.1 (Cloudflare)
- **Stability:** Custom `resolv.conf` management to prevent connection drops during heavy processing.

## 🔄 Update Policy

All units perform a `package_update` upon genesis to ensure the latest security patches are applied. Chrome is installed from the official Google repository.
