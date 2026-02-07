# 📦 PiBot Pro - Release Guide & Organization

This document explains how the PiBot project is organized for production and how to perform a new release on GitHub/GitLab.

## 📂 Release Structure

When a user downloads the PiBot Release, they will encounter the following professional structure:

```text
PiBot_Release/
├── PiBotControlCenter.exe   # Main dashboard (Windows executable)
├── README.md               # User guide & quick start
├── LICENSE                 # Legal & attribution (MIT)
├── Data/
│   └── cloud-init.yaml     # The "DNA" blueprint for Linux agents
├── web/                    # Visual interface assets (Tailwind/JS)
│   ├── index.html
│   └── app.js
├── assets/                 # Brand assets (Icons)
│   ├── pibot_icon.png
│   └── pibot_icon.ico
└── docs/
    └── PIBOT_LINUX_BASE.md # Technical spec of the internal system
```

## 🚀 How to Publish a New Version

To maintain a high-quality public repository, follow these steps to release the binaries:

1. **Consolidate:** Ensure the `Release/` folder has the latest version of all files (handled by the release script).
2. **Zip it:** Compress the **contents** of the `Release/` folder into a file named `PiBot_vX.X_Alpha.zip`.
3. **GitHub Release:**
    * Go to your repository on GitHub.
    * Click on **"Releases"** -> **"Draft a new release"**.
    * Select the Tag you created (e.g., `v0.1`).
    * Set the Title: `PiBot Pro Beta v0.1 Alpha`.
    * **Upload the .zip file** you created in step 2.
    * Publish!

## 🛡️ Best Practices

- **Never upload local logs** or `tmp` files to the release.
* **Verification:** Always run the `PiBotControlCenter.exe` from the `Release/` folder once before zipping to ensure all relative paths (like `/web`) are working correctly.
* **Attribution:** The `LICENSE` file MUST be included in every ZIP to ensure your authorship is respected.

---
*“Organized code is the foundation of powerful AI.”* 🦾🤖🌐
