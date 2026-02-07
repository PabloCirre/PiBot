#!/bin/bash

# ==============================================================================
# PIBOT Pro Beta Environment Installer
# Description: Installs XFCE, Chrome, and Hardware Support (Audio/Cam/Mic)
# Rebranded from MoltBolt to PIBOT
# ==============================================================================

set -e

# --- Configuration ---
PI_USER="pibot"
PI_PASS="pibot123"
VNC_PASS="pibot"
RESOLUTION="1280x720"
VNC_PORT=5901
NOVNC_PORT=6080

TARGET_LANG=${1:-"en"}
echo "🚀 Starting PIBOT Pro Beta Installation [$TARGET_LANG]..."

# 1. Base System & Prerequisites
export DEBIAN_FRONTEND=noninteractive
apt-get update
apt-get upgrade -y
apt-get install -y sudo curl wget git xfce4 xfce4-goodies tightvncserver \
    novnc websockify python3-pip xdotool scrot ufw dbus-x11 sed

# 2. PRO Hardware Support (Audio, Mic, Camera)
echo "🎙️  Installing PIBOT Hardware Support..."
apt-get install -y pulseaudio pulseaudio-utils alsa-utils v4l-utils \
    gstreamer1.0-plugins-base gstreamer1.0-plugins-good \
    gstreamer1.0-plugins-bad gstreamer1.0-libav \
    v4l2loopback-dkms ffmpeg

# Configure PulseAudio for network access
sed -i 's/;load-module module-native-protocol-tcp/load-module module-native-protocol-tcp auth-anonymous=1/' /etc/pulse/default.pa || true

# 3. Localization
echo "🌎 Configuring language (English Enforcement)..."
apt-get install -y language-pack-en
localectl set-locale LANG=en_US.UTF-8
localectl set-x11-keymap us

# 4. User Management
if ! id "$PI_USER" &>/dev/null; then
    useradd -m -s /bin/bash "$PI_USER"
    echo "$PI_USER:$PI_PASS" | chpasswd
    usermod -aG sudo,video,audio "$PI_USER"
fi

# 5. Browser (Chrome)
ARCH=$(dpkg --print-architecture)
if [ "$ARCH" = "amd64" ]; then
    wget -q https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
    apt-get install -y ./google-chrome-stable_current_amd64.deb
    rm google-chrome-stable_current_amd64.deb
else
    apt-get install -y chromium-browser
    ln -sf /usr/bin/chromium-browser /usr/bin/google-chrome
fi

# 6. Remote Config (VNC)
USER_HOME=$(eval echo ~$PI_USER)
mkdir -p "$USER_HOME/.vnc"
echo "$VNC_PASS" | vncpasswd -f > "$USER_HOME/.vnc/passwd"
chown -R "$PI_USER:$PI_USER" "$USER_HOME/.vnc"
chmod 600 "$USER_HOME/.vnc/passwd"

cat <<EOF > "$USER_HOME/.vnc/xstartup"
#!/bin/bash
xrdb \$HOME/.Xresources
startxfce4 &
EOF
chmod +x "$USER_HOME/.vnc/xstartup"

# 7. noVNC Branding (PIBOT)
ln -sf /usr/share/novnc/vnc.html /usr/share/novnc/index.html
sed -i 's/<title>noVNC<\/title>/<title>PIBOT - Advanced Environment<\/title>/' /usr/share/novnc/vnc.html
sed -i 's/noVNC/PIBOT Pro/g' /usr/share/novnc/vnc.html || true

# 8. Services
cat <<EOF > /etc/systemd/system/pibot-vnc.service
[Unit]
Description=PIBOT VNC Server
After=network.target

[Service]
Type=forking
User=$PI_USER
Group=$PI_USER
WorkingDirectory=$USER_HOME
ExecStartPre=-/usr/bin/vncserver -kill :1 > /dev/null 2>&1
ExecStart=/usr/bin/vncserver :1 -geometry $RESOLUTION -depth 24
ExecStop=/usr/bin/vncserver -kill :1

[Install]
WantedBy=multi-user.target
EOF

cat <<EOF > /etc/systemd/system/pibot-novnc.service
[Unit]
Description=PIBOT noVNC Proxy
After=pibot-vnc.service

[Service]
Type=simple
User=$PI_USER
ExecStart=/usr/bin/websockify --web /usr/share/novnc/ $NOVNC_PORT localhost:$VNC_PORT
Restart=always

[Install]
WantedBy=multi-user.target
EOF

systemctl daemon-reload
systemctl enable pibot-vnc.service pibot-novnc.service

# 9. Firewall
ufw allow 22/tcp
ufw allow $NOVNC_PORT/tcp
ufw --force enable

echo "✅ PIBOT Pro Beta Installation Complete!"
echo "Access: http://<vm-ip>:$NOVNC_PORT"
echo "User: $PI_USER / $PI_PASS"
