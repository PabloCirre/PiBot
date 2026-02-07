# 🩺 PIBOT Virtualization: Robust Execution & Revision Plan

## 1. Current State: "Half-Baked" Bots 🥖

The reason for `ERR_CONNECTION_REFUSED` is simple:

- **Multipass** reports `Running` as soon as the Linux kernel boots (seconds).
- **Cloud-Init** starts installing heavy packages (XFCE Desktop, VNC Server) *after* boot. This takes **3-8 minutes**.
- The user clicks **VIEW** immediately, but the VNC server isn't installed yet.

## 2. The Execution Plan (Backend Fix)

We will modify `PiBotControlCenter` to stop lying to you. A bot is only `Running` when it is **ready to serve**.

### Phase A: Smart Launch Sequence

1. **Launch** the VM with `cloud-init`.
2. **Stream Logs**: Pipe `tail -f /var/log/cloud-init-output.log` to the PIBOT Console so you see "Installing XFCE...", "Setting up VNC...", etc.
3. **Health Check Loop**: Instead of assuming it works, the app will:
    - Attempt to `curl http://<IP>:6080` every 5 seconds.
    - Status remains **🟡 Initializing** until a `200 OK` response is received.
    - Only then switch to **🟢 Running**.

### Phase B: Verification (Manual)

To verify this manually right now without code changes:

1. Launch a bot.
2. Open a terminal and run: `multipass exec <bot-name> -- tail -f /var/log/cloud-init-output.log`
3. Wait until you see: `Starting noVNC websockify...`
4. Then connect.

## 3. Immediate Revision Actions

1. **Monitor** the manual bot `pibot-debug-manual` currently launching.
2. **Confirm** when it actually starts listening on port 6080.
3. **Implement** the "Health Check Loop" in C# to automate this wait.

---
*Status: Waiting for `pibot-debug-manual` cloud-init to finish...*
