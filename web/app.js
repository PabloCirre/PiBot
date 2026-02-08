let bots = [];
let logBuffer = new Set();
let isConnected = true;

// --- Modal Controls ---
function showCreationModal() {
    const modal = document.getElementById('creation-modal');
    const content = document.getElementById('modal-content');
    if (!modal) return;
    modal.classList.remove('pointer-events-none', 'opacity-0');
    content.classList.remove('scale-95');
    content.classList.add('scale-100');
}

function hideCreationModal() {
    const modal = document.getElementById('creation-modal');
    const content = document.getElementById('modal-content');
    if (!modal) return;
    modal.classList.add('opacity-0', 'pointer-events-none');
    content.classList.remove('scale-100');
    content.classList.add('scale-95');
}

// Close on ESC
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') hideCreationModal();
});

async function botAction(botName, action) {
    const actionLabel = action === 'purge' ? 'DELETING' : action.toUpperCase();
    log(`[ACT] Sending ${actionLabel} command to core for: ${botName}...`);
    try {
        const response = await fetch(`/api/${action}?name=${botName}`);
        if (response.ok) log(`[SYS] Command ${actionLabel} acknowledged.`);
    } catch (e) {
        log(`❌ [ERR] Connection failed during ${actionLabel} operation.`);
    }
}

function connectBot(ip) {
    if (!ip) {
        log("⚠️ [WRN] Neural link address (IP) unallocated for this instance.");
        return;
    }
    log(`[LNK] Synchronizing visual stream with ${ip}:6080...`);
    window.open(`http://${ip}:6080/vnc.html?autoconnect=true`, '_blank');
}

async function launchBot() {
    hideCreationModal();
    log("[BIR] Requesting standard 4GB/30GB Rapid Birth...");
    try {
        const response = await fetch('/api/launch?ram=4096M&disk=30G', { method: 'POST' });
        if (response.ok) log("[BIR] Rapid morphogenesis propagation initiated.");
    } catch (e) {
        log("❌ [ERR] Neural Link Failure during birth request.");
    }
}

async function launchCustom() {
    let ram = document.getElementById('ram-slider').value;
    let disk = document.getElementById('disk-slider').value;

    // Ensure units are present
    if (!ram.toString().includes('M')) ram = ram + 'M';
    if (!disk.toString().includes('G')) disk = disk + 'G';

    hideCreationModal();
    log(`[BIR] Requesting Specialized Birth: ${ram} RAM / ${disk} DISK...`);
    try {
        const response = await fetch('/api/launch', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ram, disk })
        });
        if (response.ok) log("[BIR] Specialized DNA birth sequence operational.");
    } catch (e) {
        log("❌ [ERR] Connection failure on specialized birth sequence.");
    }
}

function showDnaManifest() {
    const specs = "🧬 PIBOT NEURAL DNA MANIFEST (v4.7)\n" +
        "------------------------------------\n" +
        "🖥️ OS: Ubuntu 24.04 LTS (Noble Numbat)\n" +
        "🧠 Brain: Ollama / Gemma 3 (4B Edition)\n" +
        "👁️ Vision: Moondream 2 (Native Capable)\n" +
        "🤖 Agent: OpenClaw 1.0 (Local Neural Node)\n" +
        "🎙️ STT/TTS: Faster-Whisper & Piper Neural\n" +
        "🌐 Web: Chrome Stable / noVNC Proxy\n" +
        "⚡ Accelerator: Local CPU Parallel Engine\n" +
        "📦 Storage: 30GB Dynamic Allocation\n\n" +
        "System is UP-TO-DATE and ready for genesis.";
    alert(specs);
}

function updateRange(type) {
    const valEl = document.getElementById(`${type}-val`);
    const sliderEl = document.getElementById(`${type}-slider`);
    if (valEl && sliderEl) valEl.innerText = sliderEl.value;
}

function log(msg) {
    const consoleLog = document.getElementById('console-log');
    const lastLogPeek = document.getElementById('last-log-peek');
    if (!consoleLog) return;

    const time = new Date().toLocaleTimeString('en-US', { hour12: false });
    const line = document.createElement('p');
    line.className = "mb-1 border-l-2 border-teal-500/20 pl-4 py-1";
    line.innerHTML = `<span class="opacity-30 mr-3 text-[8px] font-mono tracking-tighter">${time}</span> ${msg}`;
    consoleLog.appendChild(line);
    consoleLog.scrollTop = consoleLog.scrollHeight;

    if (lastLogPeek) lastLogPeek.innerText = msg;
}

function toggleConsole() {
    const container = document.getElementById('console-container');
    if (container) container.classList.toggle('collapsed');
}

function renderBots() {
    const grid = document.getElementById('bot-grid');
    const empty = document.getElementById('empty-state');

    if (!grid) return;

    if (bots.length === 0) {
        grid.innerHTML = "";
        empty.classList.remove('hidden');
        return;
    }

    empty.classList.add('hidden');
    grid.innerHTML = bots.map((bot, index) => {
        const progress = bot.progress || 0;
        const isRunning = bot.status === "Running";
        const isTransitioning = bot.status.includes("Starting") || bot.status.includes("Stopping");

        let displayName = bot.name.replace(/^pibot-/, '').split('-').map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');

        return `
            <div class="glass-panel p-6 border border-white/[0.03] hover:border-teal-500/30 transition-all duration-500 group relative overflow-hidden shadow-2xl">
                <!-- Glossy Hover Overlay -->
                <div class="absolute inset-0 bg-gradient-to-br from-teal-500/[0.05] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-700"></div>
                
                <div class="flex items-start justify-between mb-8 relative z-10">
                    <div class="flex items-center gap-4">
                        <!-- OFFICIAL ICON -->
                        <div class="w-12 h-12 rounded-2xl bg-white/5 flex items-center justify-center bot-icon-glow p-2 transition-transform duration-500 group-hover:scale-110">
                             <img src="assets/pibot_icon.png" class="w-full h-full object-contain ${isRunning ? '' : 'grayscale opacity-40'}">
                        </div>
                        <div>
                            <h3 class="font-black text-xs tracking-tight text-white/90 leading-none mb-1.5 uppercase italic">${displayName}</h3>
                            <div class="flex items-center gap-1.5">
                                <span class="w-1.5 h-1.5 rounded-full ${isRunning ? 'bg-teal-400' : 'bg-yellow-400'} shadow-[0_0_5px_currentColor]"></span>
                                <p class="text-[8px] ${isRunning ? 'text-teal-400' : 'text-yellow-400'} uppercase tracking-[0.2em] font-black">${bot.status}</p>
                                ${isRunning ? `<span class="text-[8px] text-gray-500 font-bold ml-1 opacity-60">⏱️ ${bot.uptime || '0m'}</span>` : ''}
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Neural Progress Visualization -->
                <div class="mb-8 relative z-10">
                    <div class="flex justify-between text-[9px] text-gray-500 font-black uppercase tracking-[0.3em] mb-3">
                        <span class="opacity-50">Core Sync</span>
                        <span class="${isRunning ? 'text-teal-400' : 'text-yellow-400'} font-bold">${progress}%</span>
                    </div>
                    <div class="w-full bg-white/5 h-1.5 rounded-full overflow-hidden p-[1px] border border-white/5">
                        <div class="bg-gradient-to-r from-teal-500 via-blue-400 to-blue-600 h-full rounded-full transition-all duration-1000 shadow-[0_0_10px_rgba(45,212,191,0.3)]" style="width: ${progress}%"></div>
                    </div>
                </div>

                <!-- Action Matrix -->
                <div class="grid grid-cols-2 gap-3 relative z-10">
                    ${isRunning ? `
                        <button onclick="botAction('${bot.name}', 'stop')" class="action-btn btn-stop shadow-lg shadow-yellow-500/5" ${isTransitioning ? 'disabled opacity-50' : ''}>
                            <span class="text-[12px]">■</span> STOP
                        </button>
                    ` : `
                        <button onclick="botAction('${bot.name}', 'start')" class="action-btn btn-start shadow-lg shadow-teal-500/5" ${isTransitioning ? 'disabled opacity-50' : ''}>
                            <span class="text-[12px]">▶</span> START
                        </button>
                    `}
                    
                    <button onclick="connectBot('${bot.ip}')" class="action-btn btn-connect shadow-lg shadow-blue-500/5" ${!isRunning || isTransitioning ? 'disabled opacity-50' : ''}>
                        <span class="text-[12px]">🔗</span> CONNECT
                    </button>
                    
                    <button onclick="botAction('${bot.name}', 'purge')" class="action-btn btn-kill col-span-2 mt-2 opacity-30 hover:opacity-100 group-hover:border-red-500/40">
                        <span class="text-[12px]">💀</span> TERMINATE UNIT
                    </button>
                    
                    <div class="col-span-2 pt-5 border-t border-white/5 mt-3 flex items-center justify-between opacity-40">
                         <div class="text-[9px] text-gray-400 font-bold uppercase tracking-tighter">
                            CORE IP: <span class="text-teal-400 font-mono ml-1">${bot.ip || '---'}</span>
                         </div>
                         <div class="text-[9px] text-gray-400 font-black font-mono">UID: ${index.toString().padStart(3, '0')}</div>
                    </div>
                </div>
            </div>
        `;
    }).join('');
}

async function refreshState() {
    try {
        const response = await fetch('/api/status');
        const data = await response.json();

        const ramMeter = document.getElementById('ram-meter');
        if (ramMeter) {
            const ram = data.ramFree || data.RamFree || 0;
            ramMeter.innerText = `${ram} MB FREE`;
            ramMeter.className = `text-xs font-bold tracking-tighter ${ram < 1000 ? 'text-red-400' : 'text-teal-400'}`;
        }

        bots = data.bots || data.Bots || [];
        const countEl = document.getElementById('bot-count');
        if (countEl) countEl.innerText = `${bots.length} ACTIVE NEURAL UNITS`;

        renderBots();

        if (data.logs || data.Logs) {
            const incoming = data.logs || data.Logs;
            incoming.forEach(l => {
                if (!logBuffer.has(l)) {
                    log(l);
                    logBuffer.add(l);
                    if (logBuffer.size > 200) {
                        const first = logBuffer.values().next().value;
                        logBuffer.delete(first);
                    }
                }
            });
        }
    } catch (e) {
        if (isConnected) {
            log("⚠️ [ALM] Neural Link Severed. Reconnecting...");
            isConnected = false;
        }
    }
}

// Initialization loop
refreshState();
setInterval(refreshState, 3000);

log("[INF] Neural Framework V4.1 [Global English] Operational.");
