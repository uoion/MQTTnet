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
