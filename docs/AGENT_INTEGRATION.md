# PiBot Agent Integration Guide

This document defines how the PiBot autonomous agent interacts with the graphical Linux environment.

## 1. Display Access

The agent operates within the X11 session. To access the display, the agent must set the `DISPLAY` environment variable.

```bash
export DISPLAY=:1
```

## 2. Visual Observation (Screenshots)

The agent captures the current state of the screen using `scrot`.

- **Command**: `scrot /tmp/pibot_screen.png`
- **Output**: A PNG file representing the current 1440x900 desktop.
- **Optimization**: Use `-q 75` to reduce file size for faster processing.

## 3. Input Execution (Mouse & Keyboard)

The agent uses `xdotool` to simulate user interactions.

### Mouse Actions

- **Move**: `xdotool mousemove <x> <y>`
- **Click**: `xdotool click 1` (Left), `xdotool click 3` (Right)
- **Drag**: `xdotool mousedown 1`; `xdotool mousemove <x> <y>`; `xdotool mouseup 1`

### Keyboard Actions

- **Type Text**: `xdotool type "Hello PiBot"`
- **Key Combo**: `xdotool key control+t` (New tab in Chrome)
- **Enter**: `xdotool key Return`

## 4. Browser Automation (Chrome)

Chrome is configured to run with a persistent profile.

- **Profile Path**: `/home/pibot/.config/google-chrome`
- **Launch Command**:

  ```bash
  google-chrome --no-first-run --disable-notifications --start-maximized &
  ```

## 5. Agent Lifecycle

The agent should be managed as a systemd service (optional but recommended).

- **Startup**: `systemctl start pibot-agent`
- **Logs**: `journalctl -u pibot-agent -f`
