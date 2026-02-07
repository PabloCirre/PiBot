import requests
import time
import threading

BASE_URL = "http://localhost:8080/api/status"
TOTAL_REQUESTS = 50
CONCURRENT_THREADS = 5

def hammer():
    for _ in range(TOTAL_REQUESTS // CONCURRENT_THREADS):
        try:
            start = time.time()
            res = requests.get(BASE_URL)
            end = time.time()
            if res.status_code == 200:
                # We expect < 20ms because it's cached in C#
                print(f"[{res.status_code}] Latency: {(end-start)*1000:.2f}ms")
            else:
                print(f"[ERROR] {res.status_code}")
        except Exception as e:
            print(f"[FAIL] {e}")

if __name__ == "__main__":
    print(f"STRESS TEST: {TOTAL_REQUESTS} requests, {CONCURRENT_THREADS} concurrent threads")
    print("Testing cache stability (responses should be very fast)...")
    
    threads = []
    start_all = time.time()
    for _ in range(CONCURRENT_THREADS):
        t = threading.Thread(target=hammer)
        t.start()
        threads.append(t)
    
    for t in threads:
        t.join()
    
    end_all = time.time()
    print(f"\nCompleted in {end_all - start_all:.2f}s")
    print("If latency was consistent and low, the background poller is working correctly.")
