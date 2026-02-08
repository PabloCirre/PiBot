# MASTER PROMPT: PIBOT Pro Dashboard Creator 🤖🚀

## Objective

Build a high-end, professional Desktop Orchestrator for **PIBOT** autonomous agents using C# and WPF. The application must serve as a central mission control for managing, monitoring, and deploying specialized Linux-based PIBOT environments.

---

## 🎨 Visual Identity & UX

- **Theme**: Solid, high-contrast professional UI (No transparency/glassmorphism).
- **Branding**: Integrate the **BMO** (vector-based) aesthetic: teal accents (`#B5E2D5`), dark charcoal backgrounds (`#1E2127`), and vibrant action colors.
- **Micro-interactions**: Use smooth transitions for progress bars and state changes.
- **Clarity**: Minimize visual noise. Filter out redundant system logs.

---

## 🛠️ Technical Stack

- **Language**: C# (Compatible with .NET 4.0/4.5 - C# 5 Syntax).
- **Framework**: WPF (Windows Presentation Foundation).
- **Dependencies**: Native `Process` calls to `multipass.exe`. No external UI libraries unless portable.
- **Portability**: Must compile using `csc.exe` via command line.

---

## 📋 Ordered Functional Requirements

### 1. Instance Discovery & Lifecycle

- **Scan**: Automatically list all Multipass instances with the prefix `pibot-*`.
- **Status Mapping**: Real-time monitoring of states: `Running`, `Stopped`, `Starting`, `Stopping`.
- **Universal Actions**: Individual buttons for **START**, **STOP**, and **VIEW** (Remote Desktop via noVNC).
- **Global Orchestration**: Single-click "START ALL" and "STOP ALL" commands.

### 2. Deployment Engine

- **Summoning**: "Launch" button that executes `multipass launch` with optimized PIBOT specs (2 CPUs, 2GB RAM).
- **Naming**: Automatic incremental naming (e.g., `pibot-01`, `pibot-02`).
- **Progress Tracking**:
  - **Global Bar**: Top-level indicator for the entire deployment process.
  - **Local Bars**: Individual cards must show a progress indicator during transitions.

### 3. Hardware Mission Control (EXPANDED) 🛰️

- **Passthrough Detection**: Monitor if Audio/Mic/Camera are correctly bridged to the VM.
- **Network Status**: Display the instance IP and connection health.
- **Resource Monitor**: (New) Show CPU and RAM usage per PIBOT instance.

### 4. Integrated Debug Console

- **Live Stream**: Real-time output of Multipass operations.
- **Intelligent Filtering**: Suppress "retrieving image" or "checking updates" noise.
- **Contextual Logging**: Colors for Errors (Red), Warnings (Yellow), and Success (Lime).

---

## 🚀 Expanded Advanced Features (Phase 2)

1. **Snapshot Manager**: Quick "Save State" and "Restore" for PIBOT nodes.
2. **Auto-Tunnels**: Automatically handle port forwarding for specialized PIBOT services.
3. **Credential Keeper**: Display current VNC/SSH passwords for each summoned instance.
4. **Bulk Updates**: One-click to update the PIBOT environment script across all running instances.

---

## ⚠️ Critical Constraints

1. **Thread Safety**: Always use `Dispatcher.Invoke` for UI updates from background threads.
2. **Clean Exit**: Ensure background processes are terminated if the app closes.
3. **BMO Vector Drawing**: Use WPF `Shapes` and `Path` for the BMO face to ensure resolution independence.

---
*Instruction for the Agent: Handle the codebase with a "Senior Systems Architect" mindset. Prioritize reliability and ultra-fast UI response.*
