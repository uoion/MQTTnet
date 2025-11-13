using CR310_Subscriber_App;
using System;
using System.Text.Json; // We need this for the JSON parser
using System.Threading.Tasks;

// This is the main entry point for your new application.
Console.WriteLine("CR310 Datalogger Subscriber");
Console.WriteLine("-----------------------------");

await using var subscriber = new HiveMqSubscriber();

// This is where we configure the JSON parser.
// The datalogger uses snake_case (like "table_name") but our C#
// classes use PascalCase (like "TableName"). This option bridges the gap.
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
};


subscriber.MessageReceived += (topic, payload) =>
{
    // Try to parse the TOPIC string
    if (DataloggerRecord.TryParse(topic, payload, out var record))
    {
        // Success! We parsed the topic.
        Console.WriteLine($"\n--- NEW RECORD PARSED ---");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"+ Topic Data: {record!}");
        Console.ResetColor();

        // Now, try to parse the JSON PAYLOAD
        try
        {
            var parsedPayload = JsonSerializer.Deserialize<TableDataPayload>(record!.JsonPayload, jsonOptions);

            // If we get here, the JSON was parsed successfully!
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
                    // Loop through the values and print them with their field names
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
            Console.WriteLine($"+ JSON Parse Error: {ex.Message}");
            Console.ResetColor();
            
            // This '!' fix is still required
            Console.WriteLine($"+ Raw Payload: {record!.JsonPayload}");
        }
        
        Console.WriteLine("------------------------------\n");
    }
    else
    {
        // The topic didn't match our data format.
        Console.WriteLine($"\n--- Unknown Message Received ---");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"+ Topic:   {topic}");
        //Console.WriteLine($"+ Payload: {payload}"); // We can hide this to reduce spam
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