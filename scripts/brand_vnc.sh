#!/bin/bash
# Branding script for noVNC
set -e
echo "Branding noVNC with PiBot Logos..."

# 1. Install dependencies if missing
if ! command -v convert &> /dev/null; then
    sudo apt-get update && sudo apt-get install -y imagemagick
fi

# 2. Go to icons directory
ICON_DIR="/usr/share/novnc/app/images/icons"
IMG_SOURCE="/home/ubuntu/pibot_logo.png"

if [ ! -f "$IMG_SOURCE" ]; then
    echo "Error: Source image /home/ubuntu/pibot_logo.png not found"
    exit 1
fi

cd "$ICON_DIR"

# 3. Bruteforce overwrite common icons with resized PNGs
for size in 16 24 32 48 60 64 72 76 96 120 144 152 192; do
    if [ -f "novnc-$size.png" ] || [ -f "novnc-${size}x${size}.png" ]; then
        sudo convert "$IMG_SOURCE" -resize "${size}x${size}" "novnc-$size.png" 2>/dev/null || true
        sudo convert "$IMG_SOURCE" -resize "${size}x${size}" "novnc-${size}x${size}.png" 2>/dev/null || true
    fi
done

# 4. Create the SVG version (embedded base64) for the main logo
B64=$(base64 -w 0 "$IMG_SOURCE")
SVG_CONTENT="<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"256\" height=\"256\"><image href=\"data:image/png;base64,$B64\" height=\"256\" width=\"256\"/></svg>"

echo "$SVG_CONTENT" | sudo tee novnc-icon.svg > /dev/null
echo "$SVG_CONTENT" | sudo tee novnc-icon-sm.svg > /dev/null

# 5. Backup check for other logo locations
sudo cp novnc-icon.svg ../../logo.svg 2>/dev/null || true
sudo cp novnc-icon.svg ../../novnc-logo.svg 2>/dev/null || true

echo "Branding complete. Please refresh noVNC tab."
