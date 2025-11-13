using CR310_Subscriber_App;
using System;
using System.Threading.Tasks;

// This is the main entry point for your new application.
Console.WriteLine("CR310 Datalogger Subscriber");
Console.WriteLine("-----------------------------");

// Use 'await using' to ensure the client is disposed of properly
// when the program exits (even if it crashes).
await using var subscriber = new HiveMqSubscriber();

// Subscribe to the MessageReceived event.
// This is where you tell the subscriber what to do when a message comes in.
subscriber.MessageReceived += (topic, payload) =>
{
    // Try to parse the topic string into our new record
    if (DataloggerRecord.TryParse(topic, payload, out var record))
    {
        // Success!
        Console.WriteLine($"\n--- NEW RECORD PARSED ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        
        // FIX 1: Add '!' to 'record'
        Console.WriteLine($"+ Data:    {record!}"); // Uses the .ToString() method
        Console.ForegroundColor = ConsoleColor.White;

        // FIX 2: Add '!' to 'record' on this line as well
        Console.WriteLine($"+ Payload: {record!.JsonPayload}");
        Console.ResetColor();
        Console.WriteLine("------------------------------\n");

        // TODO: Next step is to parse the JsonPayload
    }
    else
    {
        // The topic didn't match our expected format.
        Console.WriteLine($"\n--- Unknown Message Received ---");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"+ Topic:   {topic}");
        Console.WriteLine($"+ Payload: {payload}");
        Console.ResetColor();
        Console.WriteLine("----------------------------------\n");
    }
};

try
{
    // Connect to HiveMQ and subscribe to the topic.
    await subscriber.ConnectAndSubscribeAsync();

    // Keep the application running until the user presses Enter.
    Console.WriteLine("\nApplication is running. Press Enter to disconnect and exit.");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.ResetColor();
}