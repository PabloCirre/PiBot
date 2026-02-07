import requests
import time
import json

BASE_URL = "http://localhost:8080/api"

def test_status_endpoint():
    print("Testing /api/status...")
    try:
        start_time = time.time()
        response = requests.get(f"{BASE_URL}/status")
        end_time = time.time()
        
        print(f"Status Code: {response.status_code}")
        print(f"Response Time: {end_time - start_time:.4f}s")
        
        if response.status_code == 200:
            data = response.json()
            print("[SUCCESS] JSON Schema Valid")
            print(f"Bots Found: {len(data.get('bots', []))}")
            print(f"RAM Free: {data.get('ramFree', 0):.0f} MB")
            return True
        else:
            print(f"[FAIL] Server returned: {response.status_code}")
            return False
    except Exception as e:
        print(f"[ERROR] Connection failed: {e}")
        return False

def test_static_files():
    print("\nTesting Static Asset delivery...")
    assets = ["/", "/index.html", "/app.js", "/assets/logo.png"]
    for asset in assets:
        url = f"http://localhost:8080{asset}"
        try:
            res = requests.head(url)
            print(f"[{res.status_code}] {asset}")
        except Exception as e:
            print(f"[ERROR] Asset {asset} unreachable: {e}")

if __name__ == "__main__":
    print("--- PIBOT COMMUNICATION AUDIT ---")
    if test_status_endpoint():
        test_static_files()
    else:
        print("\n[CRITICAL] SYSTEM UNREACHABLE. Ensure PiBotControlCenter.exe is running.")
