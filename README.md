# Multi-Agent Warehouse Simulation

Multi-agent warehouse simulation developed using **Python** and **Unity**.

The project simulates multiple Automated Guided Vehicles (AGVs) operating in a warehouse environment. The multi-agent system is implemented in Python, while Unity is used to visualize the simulation.

## Project Structure

```text
MultiAgentChallenge/
│
├── Unity/
│   └── Unity project and visualization
│
├── Python/
│   └── Multi-agent simulation and communication server
│
├── README.md
└── .gitignore
```
> The exact location and names of the Python files may vary depending on the repository version.

## Overview

This project implements a **Multi-Agent System (MAS)** for a virtual warehouse environment inspired by the **RoboArena 4.0 / e80 Group challenge**.

The system simulates multiple **Automated Guided Vehicles (AGVs)** that independently operate inside a warehouse. The AGVs are responsible for selecting and executing missions, navigating through the warehouse, transporting pallets, managing their battery level, and communicating their current state.

The simulation logic is implemented in **Python**, while **Unity** is used as a 3D visualization of the simulation.

Communication between both environments is implemented through **TCP/IP using JSON messages**.

### Main Architecture

```text
+---------------------------+
|       Python MAS          |
|                           |
|  AGVs                     |
|  Missions                 |
|  Pathfinding              |
|  Battery management       |
|  Warehouse environment    |
+-------------+-------------+
              |
              | JSON over TCP/IP
              |
              v
+---------------------------+
|          Unity            |
|                           |
|  3D Warehouse             |
|  AGV visualization        |
|  Pallets                  |
|  Racks / Docks            |
|  Production lines         |
|  Persons                  |
|  AGV information          |
+---------------------------+
```

---

# Features

- Multi-agent warehouse simulation.
- Multiple AGVs operating simultaneously.
- Autonomous AGV behavior.
- AGV state management.
- AGV battery management.
- Mission assignment and execution.
- Pallet transportation.
- Warehouse navigation and pathfinding.
- Representation of racks, production lines, truck docks, and charging stations.
- One-way and restricted movement areas.
- TCP/IP communication between Python and Unity.
- JSON-based simulation frames.
- 3D visualization of AGV movement in Unity.
- Visualization of relevant AGV information:
  - AGV ID
  - Current status
  - Battery level
  - Current mission
  - Transported pallet
  - Position

---

# Requirements

## Python

The Python simulation requires:

- Python 3.x
- pip
- AgentPy
- NumPy
- SciPy
- Matplotlib

A virtual environment is recommended.

## Unity

The visualization requires:

- Unity Hub
- A compatible Unity Editor version
- A computer capable of running the 3D warehouse scene

---

# Installation

## 1. Clone the Repository

Clone the repository using Git:

```bash
git clone https://github.com/valeriarossan/MultiAgentChallenge.git
```

Then enter the project directory:

```bash
cd MultiAgentChallenge
```

---

# Python Installation

## 2. Create a Virtual Environment

From the Python project directory:

### macOS / Linux

```bash
python3 -m venv .venv
source .venv/bin/activate
```

### Windows

```bash
python -m venv .venv
.venv\Scripts\activate
```

---

## 3. Install Dependencies

Install the required Python packages:

```bash
pip install agentpy numpy scipy matplotlib
```

If the repository contains a `requirements.txt` file, use:

```bash
pip install -r requirements.txt
```

---

# Unity Installation

## 4. Open the Unity Project

1. Open **Unity Hub**.
2. Select **Add project from disk**.
3. Select the Unity project folder from the repository.
4. Open the project using the compatible Unity Editor version.
5. Open the main warehouse scene titled "MAS-Environment".

Make sure the required scripts and scene objects are present in the Unity project.

---

# Execution

The Python simulation and Unity visualization must be running at the same time.

The recommended execution order is:

```text
1. Start Python server
        ↓
2. Start Unity
        ↓
3. Unity connects to Python
        ↓
4. Python sends simulation data
        ↓
5. Unity receives JSON
        ↓
6. Simulation is visualized
```

---

## 5. Start the Python Server

Activate the Python virtual environment:

```bash
source .venv/bin/activate
```

Then run the TCP server:

```bash
python server.py
```

The server listens for Unity connections through the configured TCP port.

The current Unity client is configured to use:

```text
IP:   127.0.0.1
Port: 1101
```

Therefore, when both applications are running on the same computer, Unity connects to:

```text
127.0.0.1:1101
```

> If Python and Unity are running on different computers, change `serverIP` in `UnityTCPClient.cs` to the IP address of the computer running the Python server.

---

## 6. Run the Python Simulation

Run the main simulation script according to the current project configuration.

For example:

```bash
python agvs_bueno.py
```

The simulation generates the state of the warehouse and its AGVs.

The information sent to Unity is represented as JSON.

An AGV frame contains information similar to:

```json
{
    "id": 0,
    "pos": [3, 1],
    "state": "fetching",
    "pallet_id": null,
    "orientation": 0,
    "battery": 100,
    "mission": 0,
    "missions_completed": 0,
    "distance_traveled": 0
}
```

---

# Unity Execution

## 7. Start the Unity Visualization

Open the Unity project and press:

**Play ▶**

The `UnityTCPClient` connects to the Python server.

When a JSON payload is received:

```text
Python Simulation
       ↓
TCP connection
       ↓
UnityTCPClient
       ↓
SimulationManager
       ↓
FrameData
       ↓
AGVManager / PalletManager / other managers
       ↓
Unity Visualization
```

The Unity scene then updates according to the simulation data.

---

# TCP/IP Communication

Communication between Python and Unity uses **TCP/IP**.

The simulation data is serialized as JSON and transmitted from Python to Unity.

The general message flow is:

```text
Python Simulation
       |
       | JSON
       v
UnityTCPClient
       |
       v
SimulationManager
       |
       +----> AGVManager
       |
       +----> PalletManager
       |
       +----> PersonManager
       |
       +----> Other managers
```

The `UnityTCPClient` receives the complete JSON payload and forwards it to the `SimulationManager`.

The `SimulationManager` deserializes the JSON into `FrameData` objects.

---

# AGV Visualization

The Unity visualization represents the state of each AGV according to the Python simulation.

For each AGV, the Unity console can display information such as:

```text
========== AGV 0 ==========
Status: fetching
Battery: 100%
Mission ID: 0
Transported pallet: None
Position: [3, 1]
================================
```

This allows the user to verify that the visualization corresponds to the state of the multi-agent simulation.

The displayed information includes:

- **AGV ID**
- **Current status**
- **Battery level**
- **Current mission**
- **Transported pallet**
- **Position**

---

# Warehouse Environment

The simulated warehouse is represented as a grid with:

```text
Width:  24 cells
Height: 18 cells
```

The environment includes:

- Production lines
- Storage racks
- Truck docks
- Charging stations
- Doors
- AGV navigation paths
- Obstacles
- Restricted movement areas
- Pallets
- AGVs
- Persons

The Python simulation is responsible for determining valid movement paths, while Unity translates the grid coordinates into 3D world coordinates.

---

# AGV Behavior

Each AGV maintains its own state and information, including:

- Position
- State
- Current mission
- Battery level
- Transported pallet
- Completed missions
- Distance traveled
- Orientation

AGVs are responsible for their own decisions rather than being directly controlled by a central mission selector.

The battery management includes a charging strategy in which AGVs may travel to a charging station when their battery level becomes low.

AGVs should also avoid accepting missions that would cause their battery level to fall below the minimum operational threshold.

---

# Pathfinding

The warehouse navigation system uses a movement graph generated from the warehouse layout.

The graph considers:

- Walkable cells
- Obstacles
- Charging stations
- Restricted areas
- One-way movement cells
- Warehouse boundaries

Pathfinding is used by AGVs to determine routes between relevant warehouse locations.

---

# JSON Data Format

Simulation frames are represented using JSON.

A frame contains the state of the warehouse and its agents.

An example AGV object is:

```json
{
    "id": 0,
    "pos": [3, 1],
    "state": "fetching",
    "pallet_id": null,
    "orientation": 0,
    "battery": 100,
    "mission": 0,
    "missions_completed": 0,
    "distance_traveled": 0
}
```

### AGV fields

| Field | Description |
|---|---|
| `id` | Unique AGV identifier |
| `pos` | Current grid position |
| `state` | Current AGV state |
| `pallet_id` | ID of the transported pallet, or `null` |
| `orientation` | Current AGV orientation |
| `battery` | Current battery level |
| `mission` | Current mission ID, or `null` |
| `missions_completed` | Number of completed missions |
| `distance_traveled` | Total distance traveled |

---

# Troubleshooting

## Unity Cannot Connect to Python

Verify that:

1. The Python server is running.
2. The IP address is correct.
3. The port is correct.
4. Unity's `UnityTCPClient` uses the same port as the Python server.

Current local configuration:

```csharp
public string serverIP = "127.0.0.1";
public int serverPort = 1101;
```

---

## Port Is Already in Use

If the Python server cannot start because the port is occupied, identify the process using the port.

### macOS / Linux

```bash
lsof -i :1101
```

Then terminate the process:

```bash
kill <PID>
```

If necessary:

```bash
kill -9 <PID>
```

---

## Unity Reports a JSON Parsing Error

Verify that the JSON fields match the corresponding C# data classes.

Some JSON values can be `null`.

For example:

```json
"mission": null
```

must be represented using a nullable C# type:

```csharp
public int? mission;
```

The same applies to:

```csharp
public int? pallet_id;
```

---

# Technologies

## Simulation

- **Python**
- **AgentPy**
- **NumPy**
- **SciPy**
- **Matplotlib**

## Visualization

- **Unity**
- **C#**
- **Newtonsoft.Json**

## Communication

- **TCP/IP**
- **JSON**

## Version Control

- **Git**
- **GitHub**

---

# Authors
- Camila Adriana Portanda Polo
- Valeria Abril Rosado Sánchez
- Anna Castro Medina
- Mariana Guerrero Pérez
- Fernando José Anckerman Ramírez
- Ian Eduardo Thomas Morales

Developed as part of the **TC2008B Multi-Agent Systems / Integrative Project**.

The project integrates:

- Multi-agent simulation
- Autonomous AGV behavior
- Pathfinding
- Warehouse logistics
- Network communication
- JSON data exchange
- 3D visualization
