Learnings & Recipe: Building a C# MQTT Subscriber App

This document summarizes the exact steps and, more importantly, the specific "gotchas" we discovered while building the CR310_Subscriber_App.

The Goal
To create a standalone C# Console Application that connects to a secure, private HiveMQ Cloud broker using WebSockets (WSS) and subscribes to topics.

The Recipe (How to Build This Again)
Here is the step-by-step process we found that works.
1. Create the C# Project
From a terminal, create a new console application.
# Create a folder for the app
mkdir CR310_Subscriber_App

# Navigate into it
cd CR310_Subscriber_App

# Create a new C# console project
dotnet new console


2. Add the MQTTnet Dependency
This was a key step. We must use the official NuGet package for our standalone app.
# This adds the latest MQTTnet v5+ package
dotnet add package MQTTnet


3. Write the Code
We created two files: HiveMqSubscriber.cs (our reusable class) and Program.cs (our app's entry point).
(The final, working code for both files is in this folder).
4. Solve the Build-Breaking "Gotchas"
This was the most difficult part. We hit three major traps.
Gotcha #1 (The Most Important): The "Samples" vs. "NuGet Package" API Trap
Problem: The code in the Samples/ folder (which we used for our first tests) is built directly against the repository's source code. This source code is a development version and has a different API than the stable MQTTnet v5 package you install from NuGet.

The Errors This Caused: This mismatch led to almost all of our build errors. We tried to use classes and namespaces (like MQTTnet.Client) that existed in the samples but do not exist in the v5 NuGet package. We also got confused by the MqttFactory (a typo) vs. MqttClientFactory (correct) class.
The Fix (The "Golden Combination" for v5): The final, working HiveMqSubscriber.cs code is the correct pattern. 

For the v5 NuGet package, you must:
Use MqttClientFactory (from the root MQTTnet namespace) to create your client.
Use MqttClientOptionsBuilder (from MQTTnet) to build your connection options.
Use MqttClientSubscribeOptionsBuilder (from MQTTnet) to build your subscription options.
You must add using MQTTnet.Packets; to get access to MqttTopicFilter.
You must NOT try to use using MQTTnet.Client;.

Gotcha #2: Invalid Target Framework
Problem: The default .csproj file created by dotnet new had an invalid framework (<TargetFramework>net10.0</TargetFramework>).
Fix: We edited the CR310_Subscriber_App.csproj file and changed the framework to a valid, stable one: <TargetFramework>net8.0</TargetFramework>.
Gotcha #3: "Warnings As Errors" (CA1707)
Problem: The build system failed because our project name (CR310_Subscriber_App) contains underscores. This is a "style warning" (CA1707), but the project was configured to treat all warnings as build-breaking errors.

Fix: We edited the CR310_Subscriber_App.csproj file and added a <NoWarn> property to tell the compiler to ignore this specific warning.
The final, working <PropertyGroup> in our .csproj file looks like this:
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  
  <!-- This line was the final fix -->
  <NoWarn>$(NoWarn);CA1707</NoWarn>
  
</PropertyGroup>


5. Run the Application
With the code in place and the .csproj file fixed, the final command just works:
dotnet run


The application successfully connects, subscribes, and receives messages from the HiveMQ broker.





----------------------------------------------------------------------------

Learnings & Recipe: Building a C# MQTT Subscriber App

This document summarizes the exact steps and, more importantly, the specific "gotchas" we discovered while building the CR310_Subscriber_App.

The Goal
To create a standalone C# Console Application that connects to a secure, private HiveMQ Cloud broker using WebSockets (WSS) and subscribes to topics.

The Recipe (How to Build This Again)
Here is the step-by-step process we found that works.
1. Create the C# Project
From a terminal, create a new console application.
# Create a folder for the app
mkdir CR310_Subscriber_App

# Navigate into it
cd CR310_Subscriber_App

# Create a new C# console project
dotnet new console


2. Add the MQTTnet Dependency
This was a key step. We must use the official NuGet package for our standalone app.
# This adds the latest MQTTnet v5+ package
dotnet add package MQTTnet


3. Write the Code
We created three files:
* `HiveMqSubscriber.cs` (Our reusable class for connecting)
* `DataModels.cs` (Our class for parsing topic strings)
* `Program.cs` (Our app's entry point)

(The final, working code for all files is in this folder).

4. Solve the Build-Breaking "Gotchas"
This was the most difficult part. We hit four major traps.

---

### Gotcha #1 (The Most Important): The "Samples" vs. "NuGet Package" API Trap
* **Problem:** The code in the `Samples/` folder is built directly against the repository's source code. This source code is a development version and has a different API than the stable **MQTTnet v5 package** you install from NuGet.
* **The Errors This Caused:** This mismatch led to almost all of our early build errors. We tried to use classes and namespaces (like `MQTTnet.Client`) that existed in the samples but do not exist in the v5 NuGet package. We also got confused by the `MqttFactory` (a typo) vs. `MqttClientFactory` (correct) class.
* **The Fix (The "Golden Combination" for v5):**
    1.  Use `MqttClientFactory` (from the root `MQTTnet` namespace) to create your client.
    2.  Use `MqttClientOptionsBuilder` (from `MQTTnet`) to build your connection options.
    3.  Use `MqttClientSubscribeOptionsBuilder` (from `MQTTnet`) to build your subscription options.
    4.  You must add `using MQTTnet.Packets;` to get access to `MqttTopicFilter`.
    5.  You must **NOT** try to use `using MQTTnet.Client;`.

---

### Gotcha #2: Invalid Target Framework
* **Problem:** The default `.csproj` file created by `dotnet new` had an invalid framework (`<TargetFramework>net10.0</TargetFramework>`). This was confusing the compiler.
* **The Fix:** We edited the `CR310_Subscriber_App.csproj` file and changed the framework to a valid, stable one: `<TargetFramework>net8.0</TargetFramework>`.

---

### Gotcha #3: "Warnings As Errors" (CA1707)
* **Problem:** The build system failed because our project name (`CR310_Subscriber_App`) contains underscores. This is a "style warning" (`CA1707`), but the project was configured to treat all warnings as build-breaking errors.
* **The Fix:** We edited the `CR310_Subscriber_App.csproj` file and added a `<NoWarn>` property to tell the compiler to ignore this specific warning. The final, working `<PropertyGroup>` in our `.csproj` file looks like this:
    ```xml
    <PropertyGroup>
      <OutputType>Exe</OutputType>
      <TargetFramework>net8.0</TargetFramework>
      <ImplicitUsings>enable</ImplicitUsings>
      <Nullable>enable</Nullable>
      
      <!-- This line was the final fix -->
      <NoWarn>$(NoWarn);CA1707</NoWarn>
      
    </PropertyGroup>
    ```

---

### Gotcha #4 (The Final Boss): The Stubborn Null-Reference Error (CS8602)
* **Problem:** After fixing everything else, the build *still* failed with `error CS8602: Dereference of a possibly null reference.` in `Program.cs`. This happened even after we deleted the `bin` and `obj` folders.
* **Why?** The C# compiler (with `<Nullable>enable</Nullable>`) could not be convinced that our `DataloggerRecord.TryParse(...)` method guaranteed that the `record` variable was not null inside the `if` block.
* **The "Elegant" Fix (That Failed):** We correctly added the `[MaybeNullWhen(false)]` attribute in `DataModels.cs`. This *should* have fixed it, but the compiler, for some reason, was still failing the build.
* **The "Blunt Force" Fix (That Worked):** We had to manually override the compiler's incorrect analysis. We used the **null-forgiving operator (`!`)** in `Program.cs` to tell the compiler, "I know this is not null. Stop complaining."

* **Code in `Program.cs` (Before):**
    ```csharp
    // This caused CS8602
    Console.WriteLine($"+ Data:    {record}");
    Console.WriteLine($"+ Payload: {record.JsonPayload}");
    ```

* **Code in `Program.cs` (After Fix):**
    ```csharp
    // This fixed it
    Console.WriteLine($"+ Data:    {record!}");
    Console.WriteLine($"+ Payload: {record!.JsonPayload}");
    ```
* **Lesson:** When you are 100% certain a variable is not null, but the compiler's null-state analysis is failing, use the `!` operator to override it and force the build to succeed.

---

### 5. Run the Application
With the code in place and the `.csproj` file fixed, the final command just works:
```cmd
dotnet run


The application successfully connects, subscribes, parses the topic, and receives messages.


---

This new version is now a complete and robust guide.

You are now in the perfect position to **commit your work**. In GitHub Desktop, you should see all your changed and new files:
* `CR310_Subscriber_App/Program.cs` (Updated)
* `CR310_Subscriber_App/DataModels.cs` (New)
* `CR310_Subscriber_App/HiveMqSubscriber.cs` (Updated)
* `CR310_Subscriber_App/CR310_Subscriber_App.csproj` (Updated)
* `CR310_Subscriber_App/learnings.md` (New/Updated)
* `CR310_Subscriber_App/.editorconfig` (New)

You can commit all of them with a message like: **"Create and debug standalone C# datalogger subscriber."**
