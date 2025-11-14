# Knowledge Transfer: CR310 Datalogger Subscriber Project

This document provides all the necessary context for a code assistant to understand the current state of the `CR310_Subscriber_App` project.

## 1. High-Level Goal

The objective is to build a robust, standalone C# console application that connects to a secure HiveMQ Cloud broker, subscribes to topics from a CR310 datalogger, and intelligently parses and persists the incoming data.

## 2. Current Architecture

The application is well-structured and separated into several key files:

*   **`Program.cs`**: The main application entry point. It instantiates the subscriber, wires up the `MessageReceived` event, and contains the top-level logic to delegate incoming messages to the correct handlers (`HandleDataPayload` or `HandleMetadataPayload`).
*   **`HiveMqSubscriber.cs`**: A reusable class that encapsulates all MQTT connection and subscription logic. It handles connecting to the WSS endpoint with credentials and raises an event when a message is received.
*   **`dataModels.cs`**: Contains all data-related classes.
    *   `DataloggerRecord`: Parses the MQTT topic string using Regex to identify `data` vs. `metadata` messages.
    *   `TableDataPayload` & `MetadataPayload`: C# classes that model the two different JSON structures sent by the datalogger, used for safe deserialization.
*   **`KANBAN.md`**: A markdown file used to track project tasks (To Do, In Progress, Done).
*   **`Past_Learnings_and_progress.md`**: A detailed history of the project, including abandoned paths (PowerShell) and key breakthroughs.

## 3. Immediate Task

According to the `KANBAN.md` file, the data persistence task is complete. The next steps involve packaging the application as a Windows Service and enhancing error logging.

## 4. Key Learnings & "Gotchas"

This is critical context to avoid re-diagnosing old problems:

*   **Protocol Mismatch:** The CR310 datalogger connects via **MQTTS (port 8883)**, but the C# app *must* use **WSS (port 8884)** to work behind firewalls.
*   **API Mismatch:** The MQTTnet **NuGet package API** is different from the API in the library's **source code samples**. This was the source of major build errors. The current code in `HiveMqSubscriber.cs` uses the correct v5 NuGet pattern.
*   **.csproj Fixes:** The project required two manual fixes to the `.csproj` file to build successfully:
    1.  The `TargetFramework` was set to `net8.0`.
    2.  A `NoWarn` for `CA1707` was added to allow underscores in the project name.
*   **Null-Forgiving Operator (`!`):** The compiler's null-state analysis failed to recognize that our `TryParse` method guaranteed a non-null result. We had to use the `!` operator in `Program.cs` (e.g., `record!`) to force the build to succeed.
*   **Namespace Conflicts:** When a class name conflicts with a system-level class (e.g., `Environment`), use the fully qualified name (e.g., `System.Environment`) to resolve the ambiguity.

## 5. Configuration

The application connects to a private HiveMQ broker. The credentials are currently hard-coded in `HiveMqSubscriber.cs`:

*   **Broker URL:** `wss://70a1960deacd43f1807bfe830d8f25b3.s1.eu.hivemq.cloud:8884/mqtt`
*   **Username:** `csharp_app`
*   **Topic:** `cs/v1/#`

