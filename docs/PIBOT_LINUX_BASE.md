# PiBot Linux System Base (V4.2) - OpenClaw Edition

This document defines the standard software and configuration stack for all PiBot units, optimized for running **OpenClaw** agents.

## 🖥️ Operating System

- **Distribution:** Ubuntu 24.04 LTS (Noble Numbat)
- **User:** `pibot` (Sudoer, Password: `pibot`)
- **Desktop:** XFCE4 (High Performance / Low Latency)

## 🎮 OpenClaw Integration

PiBot is the reference hardware virtualization layer for **[OpenClaw](https://docs.openclaw.ai/)** instances.

- **Isolation:** Each PiBot instance runs a dedicated, sandboxed OpenClaw environment.
- **Independence:** Every agent has its own network stack and unique IP address, ensuring clean sessions for cloud-based automation.
- **Prerequisites:** All necessary libraries (`libsdl2`, `libvulkan1`, etc.) are pre-configured in the genesis DNA.

## 📡 Remote Access & Display

- **Protocol:** VNC Server + noVNC Web Proxy (Port **6080**).
- **Persistence:** Systemd services ensure the visual stream remains active after unit reboots.

## 🌐 Pre-installed Software

- **Web Browser:** Google Chrome (Stable) - Default for web-based automation.
- **OpenClaw Runtime:** Pre-installed and connected to the internal internet uplink.
- **Utilities:** `xdotool`, `scrot`, `curl`, `wget`.

## ⚙️ Network Configuration

- **DNS:** 8.8.8.8 & 1.1.1.1 for ultra-stable connectivity.
- **OpenClaw Ready:** Ports and firewall rules are pre-opened for external agent communication.
