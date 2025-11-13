using CR310_Subscriber_App;
using System;
using System.Text.Json;
using System.Threading.Tasks;

Console.WriteLine("CR310 Datalogger Subscriber");
Console.WriteLine("-----------------------------");

await using var subscriber = new HiveMqSubscriber();

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};

subscriber.MessageReceived += (topic, payload) =>
{
    // Try to parse the TOPIC string
    if (DataloggerRecord.TryParse(topic, payload, out var record))
     {
        Console.WriteLine($"\n--- NEW RECORD PARSED ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"+ Topic Data: {record!}");
        Console.ResetColor();

        // Check what kind of record we parsed and handle it.
        switch (record!.Type)
        {
            case RecordType.Data:
                HandleDataPayload(record, jsonOptions);
                break;

            case RecordType.Metadata:
                HandleMetadataPayload(record, jsonOptions);
                break;
        }
        
        Console.WriteLine("------------------------------\n");
    }
    else
    {
        // THIS IS THE UPDATE (as you requested)
        // We will now print the payload for all "Unknown" messages
        // This helps us debug 'statusInfo', 'watchdogEvent', etc.
        Console.WriteLine($"\n--- Unknown Message Received ---");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"+ Topic:   {topic}");
        Console.WriteLine($"+ Payload: {payload}"); // This line is now active
        Console.ResetColor();
        Console.WriteLine("----------------------------------\n");
    }
};

try
{
    await subscriber.ConnectAndSubscribeAsync();
    Console.WriteLine("\nApplication is running. Press Enter to disconnect and exit.");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.ResetColor();
}

// -------------------------------------------------------------------
// We've moved the parsing logic into its own methods
// -------------------------------------------------------------------

static void HandleDataPayload(DataloggerRecord record, JsonSerializerOptions jsonOptions)
{
    try
    {
        var parsedPayload = JsonSerializer.Deserialize<TableDataPayload>(record.JsonPayload, jsonOptions);

        if (parsedPayload?.Data != null && parsedPayload.Data.Count > 0)
        {
            var dataPoint = parsedPayload.Data[0];
            var fields = parsedPayload.Head?.Fields;
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("+ JSON Data Parsed Successfully:");
            Console.ResetColor();
            Console.WriteLine($"  > Timestamp: {dataPoint.Time.ToLocalTime()}");

            if (fields != null)
            {
                for (int i = 0; i < fields.Count && i < dataPoint.Vals?.Count; i++)
                {
                    Console.WriteLine($"  > {fields[i].Name}: {dataPoint.Vals[i]}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"+ JSON Parse Error (Data): {ex.Message}");
        Console.ResetColor();
        Console.WriteLine($"+ Raw Payload: {record.JsonPayload}");
    }
}

static void HandleMetadataPayload(DataloggerRecord record, JsonSerializerOptions jsonOptions)
{
    try
    {
        var parsedPayload = JsonSerializer.Deserialize<MetadataPayload>(record.JsonPayload, jsonOptions);

        if (parsedPayload?.Fields?.Definitions != null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("+ JSON Metadata Parsed Successfully:");
            Console.ResetColor();
            
            // Print out the field definitions we found
            foreach (var def in parsedPayload.Fields.Definitions)
            {
                Console.WriteLine($"  > Field: {def.Key} (Type: {def.Value[0]})");
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"+ JSON Parse Error (Metadata): {ex.Message}");
        Console.ResetColor();
        Console.WriteLine($"+ Raw Payload: {record.JsonPayload}");
    }
}