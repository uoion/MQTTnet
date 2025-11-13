using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; // We need this for JSON parsing
using System.Collections.Generic;    // We need this for Lists

namespace CR310_Subscriber_App
{
    // -------------------------------------------------------------------
    // This is our existing class for parsing the TOPIC STRING
    // -------------------------------------------------------------------
    public record DataloggerRecord
    {
        public string DataloggerModel { get; init; } = "Unknown";
        public string SerialNumber { get; init; } = "Unknown";
        public string TableName { get; init; } = "Unknown";
        public string JsonPayload { get; init; } = string.Empty;

        private static readonly Regex TopicRegex = new(
            @"^cs/v1/metadata/([^/]+)/([^/]+)/([^/]+)/cj$",
            RegexOptions.Compiled);

        public static bool TryParse(string topic, string payload,
            [MaybeNullWhen(false)] out DataloggerRecord? record)
        {
            // --- FIX FOR REAL DATA ---
            // Our old regex was wrong. It expected "/cj" at the end.
            // The real topic 'cs/v1/data/cr300/22143/Table10Minute' does NOT have '/cj'.
            // Let's create a new, correct regex.
            const string dataTopicPattern = @"^cs/v1/metadata/([^/]+)/([^/]+)/([^/]+)$";
            var match = Regex.Match(topic, dataTopicPattern, RegexOptions.Compiled);
            
            if (!match.Success)
            {
                record = null;
                return false;
            }

            record = new DataloggerRecord
            {
                DataloggerModel = match.Groups[1].Value,
                SerialNumber = match.Groups[2].Value,
                TableName = match.Groups[3].Value,
                JsonPayload = payload
            };
            return true;
        }

        public override string ToString()
        {
            return $"Model: {DataloggerModel}, Serial: {SerialNumber}, Table: {TableName}";
        }
    }

    // -------------------------------------------------------------------
    // These are the NEW classes for parsing the JSON PAYLOAD
    // -------------------------------------------------------------------

    // This is the root JSON object
    public class TableDataPayload
    {
        [JsonPropertyName("head")]
        public Head? Head { get; set; }

        [JsonPropertyName("data")]
        public List<DataPoint>? Data { get; set; }
    }

    public class Head
    {
        [JsonPropertyName("transaction")]
        public int Transaction { get; set; }

        [JsonPropertyName("signature")]
        public int Signature { get; set; }

        [JsonPropertyName("environment")]
        public Environment? Environment { get; set; }

        [JsonPropertyName("fields")]
        public List<Field>? Fields { get; set; }
    }

    public class Environment
    {
        [JsonPropertyName("station_name")]
        public string? StationName { get; set; }

        [JsonPropertyName("table_name")]
        public string? TableName { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("serial_no")]
        public string? SerialNo { get; set; }

        [JsonPropertyName("os_version")]
        public string? OsVersion { get; set; }

        [JsonPropertyName("prog_name")]
        public string? ProgName { get; set; }
    }

    public class Field
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("process")]
        public string? Process { get; set; }

        [JsonPropertyName("settable")]
        public bool Settable { get; set; }
    }

    public class DataPoint
    {
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } // The parser will handle the string-to-DateTime conversion!

        [JsonPropertyName("vals")]
        public List<double>? Vals { get; set; } // The parser will handle the JSON array!
    }
}