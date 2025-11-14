# Project: CR310 Datalogger MQTT Subscriber

**Goal:** To build a robust, standalone C# application that connects to a secure HiveMQ Cloud broker, subscribes to a datalogger's topics, and intelligently parses all incoming data.

| Status | Task | Details & Learnings |
| :--- | :--- | :--- |
| **✅ Done** | **Phase 1: Initial Setup & Learning** | **Task:** Clone the `dotnet/MQTTnet` repository and understand how to use it. <br/> **Learnings:** We discovered we can't just "run" a `.cs` file. The project must be built and run. We learned to use VSCode's terminal and `dotnet` commands. |
| **✅ Done** | **Phase 2: Running Samples** | **Task:** Successfully run a sample from the cloned repository. <br/> **Learnings:** Discovered the interactive `dotnet run` menu in the `Samples/` folder. This is the *correct* way to run samples, not by manually editing `Program.cs` as we first thought. |
| **✅ Done** | **Phase 3: Public Broker Test** | **Task:** Connect to a public, unsecured broker (`broker.hivemq.com`). <br/> **Learnings:** Ran `Client_Subscribe_Samples.Handle_Received_Application_Message`. This confirmed our basic understanding of MQTT `Subscribe` and `Publish` was correct. |
| **✅ Done** | **Phase 4: Private Broker Test (WSS)** | **Task:** Connect to our *private*, secure HiveMQ Cloud broker. <br/> **Details:** This was our first major custom code. We had to create a new sample in `Client_Connection_Samples.cs` to handle WebSocket Secure (WSS), TLS, and user credentials. <br/> **Learnings (Critical):** <br/> 1. `UriFormatException`: The WebSocket URL *must* start with `wss://`. <br/> 2. We debugged the correct port from your screenshot (`8884`). <br/> 3. `ReasonString: "unknown authentication key..."`: This error means the `username` or `password` is wrong. We fixed it and got a successful connection. |
| **✅ Done** | **Phase 5: Refactor to Standalone App** | **Task:** Create the `CR310_Subscriber_App` as a new, standalone console application. <br/> **Details:** We moved our logic from the `Samples` folder into a clean project (`dotnet new console`). We created `HiveMqSubscriber.cs` to hold connection logic and `Program.cs` to run it. |
| **✅ Done** | **Phase 6: The "Gotcha" Debugging** | **Task:** Debug the numerous, painful build errors in the new app. <br/> **Learnings (This was the hardest part):** <br/> **1. API Mismatch:** The `Samples` project (uses source code) has a different API than the `NuGet v5` package. This was the root cause of all our build errors. (`MqttFactory` vs `MqttClientFactory`, missing `MQTTnet.Client` namespace, etc.). <br/> **2. `.csproj` Errors:** The new project had an *invalid* `TargetFramework` (`net10.0`). We fixed it to `net8.0`. <br/> **3. `CA1707` Error:** The build failed due to underscores in the project name. We fixed this by adding `<NoWarn>CA1707</NoWarn>` to the `.csproj` file. <br/> **4. `CS8602` Null Error:** The compiler wouldn't trust our `TryParse`. We had to first try `[MaybeNullWhen(false)]` in `DataModels.cs`, and finally use the "blunt force" null-forgiving operator (`!`) in `Program.cs` (e.g., `record!`) to fix the build. |
| **✅ Done** | **Phase 7: Intelligent Topic Parsing** | **Task:** Parse the incoming MQTT topic string. <br/> **Details:** We received real topics (e.g., `cs/v1/data/...` and `cs/v1/metadata/...`). <br/> **Learnings:** We discovered the datalogger sends *multiple* message types. We built `DataloggerRecord.TryParse` with a `Regex` for *each* topic type and added a `RecordType` enum to tag the messages. |
| **✅ Done** | **Phase 8: Intelligent JSON Parsing** | **Task:** Parse the *two* different JSON payloads. <br/> **Details:** We created C# classes for both the `data` payload (`TableDataPayload`) and the `metadata` payload (`MetadataPayload`). We updated `Program.cs` with a `switch` statement to call the correct JSON parser (`HandleDataPayload` or `HandleMetadataPayload`) based on the `RecordType`. <br/> **Status:** This is now 100% functional. |
| **✅ Done** | **Phase 9: Data Persistence** | **Task:** Save the parsed data to a local CSV file. <br/> **Details:** Implemented a new method `AppendToCsv` in `Program.cs`. This method is called from `HandleDataPayload` and appends the timestamp and sensor values to a `.csv` file. The file is created with a header if it does not exist. This provides a simple and effective way to store the data. |
| **✅ Done** | **Phase 10: Finalization and Debugging** | **Task:** Resolved final build errors and warnings. <br/> **Details:** Fixed ambiguous reference errors (`CS0104`) by using fully qualified names (e.g., `System.Environment`). Addressed and fixed compiler warnings `CA1805`, `CA1852`, and `CS1004` to ensure a clean, production-ready build. |
| **🗓️ To Do** | **Phase 11: Package and Deploy** | **Task:** Package the application as a Windows Service and enhance error logging. |

---

## Legacy Development (Development 1)

This section contains the history of the first development attempt. It is kept for context.

# Kairo Project: CR310 MQTT Data Logger - Kanban Board

## [ 🚀 Backlog / To Do ]

* **Package App as a Windows Service:**
    * Research `dotnet` worker services to run the C# app as a background service (no console window).
    * This will allow the logger to start automatically when the PC boots.
* **Enhance Error Logging:**
    * Implement a proper logging library (e.g., Serilog) in the C# app.
    * Write all console messages (and errors) to a rolling text log file (e.g., `C:\Kairo\Logs\app.log`).
* **Implement a "Dead Man's Switch":**
    * Add logic to the C# app to send an alert (e.g., email) if no message is received from the logger for a set period (e.g., > 1 hour).
* **Data Visualization:**
    * Explore tools to read the `cr310_data.dat` (TOA5) file for real-time graphing (e.g., Excel, Grafana, or a custom web app).
* **Database Integration:**
    * Instead of (or in addition to) a `.dat` file, log data directly to a database (e.g., InfluxDB, PostgreSQL) for easier querying.
* **Refine C# TOA5 Parser:**
    * Make the JSON parsing more robust by creating C# classes (`Head`, `Environment`, `DataPoint`) instead of using `dynamic`. This is safer and prevents crashes if the JSON structure changes.

---

## [ 🏃‍♂️ In Progress ]

* **Develop Robust C# Data Logger:**
    * **Current Status:** V12 (Self-Healing) is complete.
    * **Task:** Implement the `IManagedMqttClient` for self-healing.
    * **Details:** The managed client will automatically handle network drops, connection loss, and will automatically re-subscribe to the topic on reconnect. This is the final, production-ready version of the script.

---

## [ ✅ Done / Accomplished ]

* **Initial Setup & Broker Config (HiveMQ)**
    * Created a free Serverless HiveMQ Cloud cluster.
    * Identified all critical connection parameters (Host, Port 8883, WSS Port 8884).
    * Created initial user credentials.

* **Datalogger Connection (CR310) - SUCCESS**
    * Configured CR310 with Host, Port 8883, and new `hivemq_cr310` credentials.
    * **[SOLVED]** `MQTT State: Connection retry wait`.
    * **[FIX]** Located the `TLS` tab and set `Max TLS Server Connections` from `0` to `1`.
    * **[VERIFIED]** `MQTT State` now shows **`Publishing`**.

* **PC Client Connection (MQTTBox) - SUCCESS**
    * **[SOLVED]** `mqtts / tls` on port `8883` failed (likely firewall).
    * **[FIX]** Switched to **`wss` (Secure WebSocket) protocol** on port `8884`.
    * **[FIX]** Created new, separate `mqttbox_cr310` credentials.
    * **[VERIFIED]** MQTTBox successfully connects and receives data.

* **PowerShell Scripting (Deprecated)**
    * **Attempt 1:** `MQTT` module failed (wrong module name).
    * **Attempt 2:** `PSMQTT` module failed (connection error, no `wss` support).
    * **Attempt 3:** `MQTTnet` DLL (manual load) failed (incompatible .NET version).
    * **Attempt 4:** `M2Mqtt` DLL (manual load) failed (connection error).
    * **[CONCLUSION]** PowerShell is too fragile for this task due to module, dependency, and security (DLL unblocking) issues.

* **C# Console App - SUCCESS**
    * **V1:** Initialized project (`dotnet new console`).
    * **V2:** Added `MQTTnet` package (`dotnet add package MQTTnet`).
    * **V3:** Implemented `IMqttClient` with `wss` (V9/V10/V11).
    * **V4:** Solved Windows security error by "unblocking" DLLs.
    * **V5:** Solved dependency error by using the correct `.NET Framework` (`net48`) DLLs.
    * **V6 (V12):** Migrated to `M2Mqtt.dll` for better framework compatibility.
    * **V7 (C#):** Pivoted to a full C# Console App using `dotnet add package MQTTnet`. This is the robust, correct solution.
    * **V8:** Implemented TOA5 file conversion logic.
    * **V9:** Added raw JSON (`.jsonl`) backup logic.
    * **V10 (V12):** Upgraded client to `IManagedMqttClient` for self-healing and auto-reconnect.