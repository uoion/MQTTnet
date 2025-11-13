using System;
using System.Diagnostics.CodeAnalysis; // This is the new 'using' for the fix
using System.Text.RegularExpressions;

namespace CR310_Subscriber_App
{
    /// <summary>
    /// A C# record to hold the structured data parsed from an MQTT topic.
    /// </summary>
    public record DataloggerRecord
    {
        // These properties are parsed from the topic string
        public string DataloggerModel { get; init; } = "Unknown";
        public string SerialNumber { get; init; } = "Unknown";
        public string TableName { get; init; } = "Unknown";

        // This property is the raw message payload
        public string JsonPayload { get; init; } = string.Empty;

        // This is a private "Regex" (Regular Expression) to parse the topic
        // It looks for 5 parts: "cs/v1/data/[part1]/[part2]/[part3]/cj"
        private static readonly Regex TopicRegex = new(
            @"^cs/v1/data/([^/]+)/([^/]+)/([^/]+)/cj$",
            RegexOptions.Compiled);

        /// <summary>
        /// A "factory method" that attempts to create a new DataloggerRecord
        /// by parsing the topic string.
        /// </summary>
        /// <param name="topic">The MQTT topic string</param>
        /// <param name="payload">The raw message payload</param>
        /// <param name="record">The output record if parsing is successful</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        //
        // This is the FIX:
        // We add the [MaybeNullWhen(false)] attribute.
        // This tells the compiler that if the method returns 'false', 'record' MIGHT be null.
        // But if it returns 'true', 'record' is NOT null.
        // This satisfies the null-checker in Program.cs.
        public static bool TryParse(string topic, string payload, 
            [MaybeNullWhen(false)] out DataloggerRecord? record)
        {
            var match = TopicRegex.Match(topic);

            if (!match.Success)
            {
                // The topic was not in the expected format
                record = null;
                return false;
            }

            // We have a match! Extract the parts.
            // match.Groups[0] is the full string
            // match.Groups[1] is the first captured group (...)
            record = new DataloggerRecord
            {
                DataloggerModel = match.Groups[1].Value,
                SerialNumber = match.Groups[2].Value,
                TableName = match.Groups[3].Value,
                JsonPayload = payload
            };
            return true;
        }

        // This makes it print nicely to the console
        public override string ToString()
        {
            return $"Model: {DataloggerModel}, Serial: {SerialNumber}, Table: {TableName}";
        }
    }
}