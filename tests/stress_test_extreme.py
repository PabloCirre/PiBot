
import subprocess
import time
import sys
import json

MULTIPASS = r"C:\Program Files\Multipass\bin\multipass.exe"

def run_cmd(args):
    return subprocess.run([MULTIPASS] + args, capture_output=True, text=True)

def log(msg):
    print(f"[STRESS-TEST] {msg}")

def test_cycle(bot_name):
    log(f"Starting Stress Cycle for {bot_name}")
    
    # 1. Launch
    log("Step 1: Launching...")
    run_cmd(["launch", "lts", "--name", bot_name, "--cpus", "1", "--memory", "512M"])
    
    # 2. Status Check
    log("Step 2: Verifying Running status...")
    res = run_cmd(["list", "--format", "json"])
    data = json.loads(res.stdout)
    instance = next((i for i in data['list'] if i['name'] == bot_name), None)
    if instance and instance['state'] == "Running":
        log("SUCCESS: Bot is Running")
    else:
        log("FAILED: Bot is not Running")
        return False

    # 3. Stop
    log("Step 3: Stopping...")
    run_cmd(["stop", bot_name])
    res = run_cmd(["list", "--format", "json"])
    data = json.loads(res.stdout)
    instance = next((i for i in data['list'] if i['name'] == bot_name), None)
    if instance and instance['state'] == "Stopped":
        log("SUCCESS: Bot is Stopped")
    else:
        log("FAILED: Bot is not Stopped")

    # 4. Start
    log("Step 4: Starting again...")
    run_cmd(["start", bot_name])
    res = run_cmd(["list", "--format", "json"])
    data = json.loads(res.stdout)
    instance = next((i for i in data['list'] if i['name'] == bot_name), None)
    if instance and instance['state'] == "Running":
         log("SUCCESS: Bot is Running again")
    else:
         log("FAILED: Bot failed to restart")

    # 5. Purge
    log("Step 5: Purging (Assassinating) [SKULL]")
    run_cmd(["delete", "--purge", bot_name])
    res = run_cmd(["list", "--format", "json"])
    if bot_name not in res.stdout:
        log("SUCCESS: Bot Purged")
    else:
        log("FAILED: Bot still exists")
    
    return True

if __name__ == "__main__":
    test_name = "pibot-stress-unit"
    log("--- EXTREME STRESS TEST INITIATED ---")
    start_time = time.time()
    
    # Clean up any leftover
    run_cmd(["delete", "--purge", test_name])
    
    success = test_cycle(test_name)
    
    total_time = time.time() - start_time
    log(f"--- TEST FINISHED in {total_time:.2f}s ---")
    if success:
        log("RESULT: SYSTEM STABLE")
    else:
        log("RESULT: SYSTEM UNSTABLE")
