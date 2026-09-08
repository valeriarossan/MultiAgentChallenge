'''
import json
model = WarehouseModel(n_agvs=N_AGVS, n_persons=N_PERSONS, seed=SEED)
for _ in range(N_STEPS): model.step()

with open("simulation_frames.json", "w") as f:
    json.dump(model.history, f, indent=4)
'''

import socket
import json

def send():
    json_name  = "simulation_frames.json"
    with open(json_name, "r") as file: json_data = json.load(file)
    #print(json_data)
    print(type(json_data))
    s=socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind(('127.0.0.1', 1101))
    s.listen()

    while True:
        conn, addr = s.accept()
        #conn.send(b"I'll send frame $")
        data = json.dumps(json_data).encode("utf-8")
        print(type(data))
        conn.sendall(data)
        conn.close()

    s.close()

#send()print(type(json_data))