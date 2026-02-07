# 🤖 PiBot Pro - Neural Core [V0.1 Alpha]

[![License: MIT](https://img.shields.io/badge/License-MIT-teal.svg)](https://opensource.org/licenses/MIT)
[![Version](https://img.shields.io/badge/Version-0.1--Alpha-blue.svg)]()
[![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-lightgrey.svg)]()

> **The ultimate management ecosystem for autonomous agents, cloudbots, and virtualized neural units.**

PiBot Pro is a high-performance orchestration layer designed to deploy, manage, and scale autonomous agents (like **CloudBots**, **Moltbolt**, and **OpenClaw**) within secure, isolated virtual environments.

By leveraging **Multipass** virtualization, PiBot ensures that your agents run in a consistent, sandboxed Linux environment with official toolsets (Google Chrome, noVNC, and systemd persistence) ready out of the box.

---

## 🚀 Key Features

- **Genesis Orchestration:** Deploy standard or specialized units with custom RAM and Disk allocation in seconds.
- **Secure Virtualization:** Each agent lives in its own isolated Ubuntu instance. No more dependency hell or security leaks.
- **Neural Link (noVNC):** Access your agents' desktop visually through any web browser without installing additional clients.
- **Persistent Core:** Auto-restart services ensure your agents' visual streams and tools stay online through unit restarts.
- **Official Tooling:** Every unit comes pre-shipped with **Google Chrome**, `wget`, `curl`, and a native XFCE desktop environment.
- **Telemetry Console:** Real-time monitoring of all neural births and system operations.

---

## 🛠️ Why use Virtualized Agents?

Using PiBot with virtualization is superior to native execution for several reasons:

1. **Security & Isolation:** Agents are sandboxed. If an agent (like a **CloudBot**) executes untrusted code, it stays within the VM, protecting your host OS.
2. **Resource Throttling:** Prevent an agent from consuming 100% of your host's RAM. Define the "Metabolic DNA" (Resources) at birth.
3. **Pure Environment:** No need to install Linux tools on Windows. PiBot handles the translation layer between the Windows control center and the Linux agents.
4. **Instant Scalability:** Need 5 agents? Launch 5 Genesis units. Need to wipe one? Use the **Terminate (💀)** command.

---

## 📥 Installation & Download

### 💿 Quick Start

1. **Download the latest release:** [PiBot_v0.1_Release.zip](https://github.com/YourRepo/PiBot/releases/download/v0.1/PiBot_v0.1_Alpha.zip) (Mirror)
2. **Prerequisite:** Install [Multipass](https://multipass.run/) (Required for the virtualization engine).
3. **Run:** Open `PiBotControlCenter.exe`.
4. **Launch:** Click **+ GENERATE PIBOT** and watch your first agent come to life.

---

## 📂 Project Structure

- `PiBotControlCenter.exe`: The main C# WPF application (Windows-side).
- `Data/cloud-init.yaml`: The genetic blueprint for all Linux agents.
- `web/`: The frontend interface (HTML5/Tailwind/JS).
- `docs/`: Technical specifications for the Linux Base and development guides.

---

## 📜 License & Acknowledgments

Distributed under the **MIT License**. Created by the PiBot Team for the community.
Special thanks to the developers of **Moltbolt**, **OpenClaw**, and the **Multipass** team.

---

*“Giving life to silicon, one unit at a time.”* 🦾🤖🌐
