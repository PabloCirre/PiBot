
import subprocess
import sys
import json
import os
import time

MULTIPASS = r"C:\Program Files\Multipass\bin\multipass.exe"

class PiBotCLI:
    def __init__(self):
        self.check_env()

    def check_env(self):
        if not os.path.exists(MULTIPASS):
            print(f"[ERROR] Multipass not found at {MULTIPASS}")
            sys.exit(1)

    def run_mp(self, args):
        cmd = [MULTIPASS] + args
        result = subprocess.run(cmd, capture_output=True, text=True)
        return result

    def list_bots(self):
        res = self.run_mp(["list", "--format", "json"])
        if res.returncode != 0:
            print("Error listing bots")
            return
        
        data = json.loads(res.stdout)
        print(f"{'NAME':<40} | {'STATE':<10} | {'IP':<15} | {'VNC':<5}")
        print("-" * 80)
        
        for inst in data.get("list", []):
            if "pibot" in inst["name"]:
                name = inst["name"]
                state = inst["state"]
                ip = inst["ipv4"][0] if inst["ipv4"] else "N/A"
                
                # Check health
                vnc_ok = "WAIT"
                if state == "Running" and ip != "N/A":
                    # Quick check if port 6080 is open via exec
                    check = self.run_mp(["exec", name, "--", "nc", "-z", "localhost", "6080"])
                    vnc_ok = "READY" if check.returncode == 0 else "BOOT"
                
                print(f"{name:<40} | {state:<10} | {ip:<15} | {vnc_ok}")

    def exec_in(self, name, command):
        print(f"--- EXECUTING IN {name} ---")
        # To support pipes |, we must wrap the command in 'bash -c'
        cmd = [MULTIPASS, "exec", name, "--", "bash", "-c", command]
        res = subprocess.run(cmd, capture_output=True, text=True)
        print(res.stdout)
        if res.stderr:
            print(f"ERR: {res.stderr}")

    def purge(self, name):
        print(f"TERMINATING {name}...")
        self.run_mp(["delete", "--purge", name])
        print("Done.")

    def audit(self):
        print("SYSTEM AUDIT INITIATED")
        print("1. Checking Multipass Nodes...")
        self.list_bots()
        print("\n2. Checking Host Resources...")
        # (Could add more host checks here)

if __name__ == "__main__":
    cli = PiBotCLI()
    if len(sys.argv) < 2:
        print("Usage: python pibot_tool.py [list|exec|purge|audit] [args...]")
        sys.exit(1)

    cmd = sys.argv[1]
    if cmd == "list":
        cli.list_bots()
    elif cmd == "audit":
        cli.audit()
    elif cmd == "purge":
        if len(sys.argv) > 2: cli.purge(sys.argv[2])
    elif cmd == "exec":
        if len(sys.argv) > 3: cli.exec_in(sys.argv[2], sys.argv[3])
    else:
        print("Unknown command")
