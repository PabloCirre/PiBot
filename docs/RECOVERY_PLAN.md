# 🩺 PIBOT Virtualization Recovery Plan

## 1. Current Status Diagnosis

Crucial diagnostics have been run on the local Multipass environment.

- **Multipass Binary**: Detected at `C:\Program Files\Multipass\bin\multipass.exe`.
- **Instance State**:
  - ✅ `pibot-brenda-achernar-olive`: **Running** (IP: 172.31.131.221).
  - ⏳ `pibot-dorothy-himalia-sapphire`: **Stuck/Starting**.
  - 🗑️ `moltbolt-*`: Multiple legacy instances are **Stopped** but consuming disk formatting.

## 2. Identified Issues

1. **Zombie Processes**: Old "moltbolt" instances are cluttering the hypervisor backend.
2. **User Blindness**: The current UI does not show *why* a bot might fail to start (e.g., timeout, disk full, image download error).
3. **Path Issues**: The system PATH does not include Multipass, requiring hardcoded invocation which can be fragile.

## 3. The Robust Plan (Execution Steps)

### Phase A: Deep Cleanup (Immediate)

We will implement a "Purge System" to:

1. **Delete** all "Deleted" state instances to free disk space immediately.
2. **Archive** legacy "moltbolt" instances (stop and remove).
3. **Reset** the Multipass daemon if it becomes unresponsive.

### Phase B: Robust Launch Logic (Code Update)

I will refactor the `OnNewBot` method in `PiBotControlCenter` to:

1. **Pre-Flight Check**: Verify RAM/CPU availability before launch.
2. **Real-Time Logging**: Redirect `StandardError` from the Multipass process directly to the PIBOT Console. You will see *exactly* what hangs.
3. **Timeout Guards**: If a bot stays in "Starting" for >60s, auto-diagnose the issue.
4. **Version Locking**: Explicitly request `22.04` or `lts` to avoid trying to download broken daily images.

### Phase C: Self-Healing

Implement a "Doctor" button that runs:

```powershell
multipass restart
multipass purge
```

This ensures that if the virtualization engine hangs (common in Windows with Hyper-V/VirtualBox conflicts), the user can reset it with one click.

## 4. Next Steps for YOU

1. **Approve this plan**.
2. I will immediately code Phase B (Robust Launch) into the Control Center.
3. I will run a cleanup script for Phase A.
