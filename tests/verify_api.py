
import requests
import json

try:
    # Try localhost:8080 or wait for the port. 
    # Since I don't know the port (it's dynamic), I'll check the log.
    with open(r"c:\Users\MASTER\Desktop\PhoenixBOT\pibot_system.log", "r") as f:
        content = f.read()
        # Find "Web Interface listening on port XXX"
        import re
        match = re.search(r"listening on port (\d+)", content)
        if match:
            port = match.group(1)
            url = f"http://localhost:{port}/api/status"
            print(f"Checking API at {url}")
            r = requests.get(url)
            print(json.dumps(r.json(), indent=2))
        else:
            print("Port not found in logs")
except Exception as e:
    print(f"Error: {e}")
