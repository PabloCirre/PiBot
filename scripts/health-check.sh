#!/bin/bash

# ==============================================================================
# MoltBolt Environment Health Check
# Checks if VNC, noVNC, and dependencies are running correctly.
# ==============================================================================

echo "🔍 Running MoltBolt Environment Health Check..."

check_service() {
    if systemctl is-active --quiet "$1"; then
        echo "✅ Service $1 is running."
    else
        echo "❌ Service $1 is NOT running."
    fi
}

# 1. Check Systemd Services
check_service moltbolt-vnc.service
check_service moltbolt-novnc.service

# 2. Check Ports
echo "📡 Checking network ports..."
if ss -tuln | grep -q ":6080"; then
    echo "✅ Port 6080 (noVNC) is open."
else
    echo "❌ Port 6080 (noVNC) is closed."
fi

if ss -tuln | grep -q ":5901"; then
    echo "✅ Port 5901 (VNC) is open."
else
    echo "❌ Port 5901 (VNC) is closed."
fi

# 3. Check X11 Display
echo "🖥️ Checking X11 Display :1..."
if xdpyinfo -display :1 >/dev/null 2>&1; then
    echo "✅ X11 Display :1 is available."
else
    echo "❌ X11 Display :1 is NOT available."
fi

# 4. Check Browser
echo "🌐 Checking Google Chrome/Chromium installation..."
if command -v google-chrome >/dev/null 2>&1; then
    echo "✅ Google Chrome is installed."
else
    echo "❌ Google Chrome is NOT found."
fi

echo "-------------------------------------------------------"
echo "Health Check Finished."
