import requests
import time

BASE_URL = "http://localhost:8080/api"

def audit_lifecycle():
    print("--- PIBOT LIFECYCLE AUDIT ---")
    
    # 1. Get Initial State
    print("Fetching initial state...")
    data = requests.get(f"{BASE_URL}/status").json()
    bots = data.get('bots', [])
    if not bots:
        print("No bots found for lifecycle test. Please create a bot first.")
        return

    target_bot = bots[0]['name']
    initial_status = bots[0]['status']
    print(f"Target Bot: {target_bot} (Current: {initial_status})")

    # 2. Toggle State
    action = "stop" if initial_status == "Running" else "start"
    print(f"Triggering {action} for {target_bot}...")
    requests.post(f"{BASE_URL}/{action}?name={target_bot}")

    # 3. Wait for Change (Multipass takes time)
    print("Waiting for state propagation (max 30s)...")
    success = False
    for i in range(15):
        time.sleep(2)
        data = requests.get(f"{BASE_URL}/status").json()
        current_bot = next((b for b in data['bots'] if b['name'] == target_bot), None)
        if current_bot:
            print(f"Poll {i+1}: {current_bot['status']}")
            # Status might be 'Starting', 'Running', 'Stopping', 'Stopped'
            if action == "stop" and current_bot['status'] == "Stopped":
                success = True
                break
            if action == "start" and current_bot['status'] == "Running":
                success = True
                break
    
    if success:
        print(f"\n[PASS] Lifecycle command '{action}' verified via API.")
    else:
        print(f"\n[FAIL] Lifecycle command '{action}' did not reflect in time.")

if __name__ == "__main__":
    audit_lifecycle()
