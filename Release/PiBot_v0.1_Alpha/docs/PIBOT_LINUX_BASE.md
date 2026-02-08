# PiBot Linux System Base (V4.6) - AI Edge Edition

Official URL: **[https://pibot.club/](https://pibot.club/)**

This document defines the standard software and configuration stack for all PiBot units, optimized for running **OpenClaw**, **Autonomous Agents**, and **Local AI** systems.

## 🖥️ Operating System & Hardware

- **Distribution:** Ubuntu 24.04 LTS (Noble Numbat)
- **User:** `pibot` (Sudoer, Password: `pibot`)
- **Desktop:** XFCE4 (High Performance / Low Latency)
- **Hardware Profile:** 4GB RAM / 2 vCPU / 30GB Disk (AI-Ready Standard)
- **Auto-Activation:** Google Chrome starts automatically on login pointing to the official hub at **[pibot.club](https://pibot.club/)**.

## 🧠 Neural Logic (AI Stack)

PiBot units represent the next generation of "Hardware-Aware" AI. Every unit comes pre-equipped with an autonomous brain layer:

- **Local Runner:** Ollama (Service-hardened)
- **LLM:** Google Gemma 3 (4B Optimized) - Multilingual reasoning, coding, and decision making.
- **VLM:** Moondream 2 - Advanced visual understanding. Perfect for "eyes-on-browser" automation.
- **STT (Ears):** `faster-whisper` - Near real-time speech-to-text.
- **Wake Word:** `openwakeword` - Low-latency detection.
- **TTS (Voice):** `piper-tts` - High-quality offline neural voice synthesis.

## 🎮 OpenClaw Integration

PiBot is the reference virtualization layer for **[OpenClaw](https://docs.openclaw.ai/)**.

- **Auto-Installation:** OpenClaw is pre-installed and globally available via `openclaw`.
- **Isolation:** Each PiBot instance has a unique identity, IP, and hardware fingerprint.
- **Node.js Stack:** Powered by Node.js 22+ for maximum performance in agent logic.

## 📡 Remote Access & Hardware Passthrough

- **Protocol:** VNC Server + noVNC Web Proxy (Port **6080**).
- **Audio:** PulseAudio configured with network TCP support (Port **4713**).
- **Webcam/Mic:** Full access to `/dev/video*` and `/dev/snd/*`.
- **Connectivity:** Optimized DNS (8.8.8.8) and full internet uplink.

## 🌐 Pre-installed Software

- **Web Browser:** Google Chrome (Stable) - Configured for automation with `--no-first-run`.
- **Containerization:** `docker.io` pre-installed for running auxiliary agent tools.
- **Utilities:** `xdotool`, `scrot`, `curl`, `wget`, `git`, `python3-pip`, `ffmpeg`.

## ⚙️ Network Configuration

- **Ports:**
  - `6080`: Visual Control Dashboard (Web VNC)
  - `18789`: OpenClaw UI
  - `11434`: Ollama Neural API
  - `4713`: PulseAudio Network Streaming
- **Firewall:** UFW enabled with essential control ports pre-opened.

## 🔌 Hardware Connection (Host to PiBot)

### 🎙️ Audio & Microphone

PiBot uses **PulseAudio over TCP**.

1. The guest is listening on port **4713**.
2. To send/receive audio, you can use a PulseAudio client on your host and set `PULSE_SERVER=<pibot-ip>:4713`.

### 📷 Webcam

1. **Permissions:** All `/dev/video*` devices are unlocked.
2. **Setup:** Use **USBIP** to bridge your Windows webcam with the PiBot IP. Once bridged, the AI brain can see through your physical camera.

---
*PiBot: Your private, virtualized hardware for the AI era.*
