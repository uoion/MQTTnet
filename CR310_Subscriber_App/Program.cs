using CR310_Subscriber_App;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CR310_Subscriber_App
{
    sealed class Program
    {
        // --- CONFIGURATION FOR CSV OUTPUT ---
        const string outputFolder = "Datalog";
        const string outputFile = "datalog.csv";
        // ------------------------------------

        // --- STATE MANAGEMENT ---
        private static int _recordCounter;
        // ------------------------

        static async Task Main(string[] args)
        {
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
                            // This is a data record, so we increment our counter
                            _recordCounter++;
                            HandleDataPayload(record, jsonOptions, _recordCounter);
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
        }

        // -------------------------------------------------------------------
        // We've moved the parsing logic into its own methods
        // -------------------------------------------------------------------

        static void HandleDataPayload(DataloggerRecord record, JsonSerializerOptions jsonOptions, int recordNumber)
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
                    Console.WriteLine($"  > RecordNo: {recordNumber}"); // Use the client-side counter
                    Console.WriteLine($"  > Timestamp: {dataPoint.Time.ToLocalTime()}");

                    if (fields != null)
                    {
                        for (int i = 0; i < fields.Count && i < dataPoint.Vals?.Count; i++)
                        {
                            Console.WriteLine($"  > {fields[i].Name}: {dataPoint.Vals[i]}");
                        }
                    }

                    // --- NEW: Call the method to save the data ---
                    AppendToCsv(parsedPayload, recordNumber);
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

        static void AppendToCsv(TableDataPayload payload, int recordNumber)
        {
            try
            {
                // 1. Get the required data points
                var fields = payload.Head?.Fields;
                var dataPoint = payload.Data?.FirstOrDefault();
                if (fields == null || dataPoint == null || dataPoint.Vals == null)
                {
                    return; // Not enough data to write a record
                }

                // 2. Prepare file and folder paths
                var filePath = Path.Combine(outputFolder, outputFile);
                Directory.CreateDirectory(outputFolder); // This does nothing if the folder already exists

                // 3. Check if the file is new to write the header
                if (!File.Exists(filePath))
                {
                    var header = new StringBuilder("record_no,timestamp,");
                    header.Append(string.Join(",", fields.Select(f => f.Name)));
                    File.WriteAllText(filePath, header.ToString() + System.Environment.NewLine);
                }

                // 4. Format the data row
                var dataRow = new StringBuilder($"{recordNumber},{dataPoint.Time.ToLocalTime():yyyy-MM-dd HH:mm:ss},");
                dataRow.Append(string.Join(",", dataPoint.Vals));

                // 5. Append the data to the file
                File.AppendAllText(filePath, dataRow.ToString() + System.Environment.NewLine);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"+ Data successfully written to '{filePath}'");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"+ CSV Write Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}