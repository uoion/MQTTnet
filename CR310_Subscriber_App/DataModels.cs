using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace CR310_Subscriber_App
{
    // This enum tells us what kind of topic we parsed
    public enum RecordType
    {
        Data,
        Metadata,
        Unknown
    }

    // -------------------------------------------------------------------
    // This class parses the TOPIC STRING
    // -------------------------------------------------------------------
    public record DataloggerRecord
    {
        public string DataloggerModel { get; init; } = "Unknown";
        public string SerialNumber { get; init; } = "Unknown";
        public string TableName { get; init; } = "Unknown";
        public string JsonPayload { get; init; } = string.Empty;
        public RecordType Type { get; init; } = RecordType.Unknown; // Store what type it is

        // We now have TWO regex patterns: one for 'data' and one for 'metadata'
        private static readonly Regex DataTopicRegex = new(
            @"^cs/v1/data/([^/]+)/([^/]+)/([^/]+)$", RegexOptions.Compiled);
        
        private static readonly Regex MetadataTopicRegex = new(
            @"^cs/v1/metadata/([^/]+)/([^/]+)/([^/]+)$", RegexOptions.Compiled);

        public static bool TryParse(string topic, string payload,
            [MaybeNullWhen(false)] out DataloggerRecord? record)
        {
            Match match = DataTopicRegex.Match(topic);
            RecordType type = RecordType.Data;

            if (!match.Success)
            {
                // It wasn't a 'data' topic, try 'metadata'
                match = MetadataTopicRegex.Match(topic);
                type = RecordType.Metadata;
            }

            if (!match.Success)
            {
                // It wasn't metadata either
                record = null;
                return false;
            }

            // A match was found!
            record = new DataloggerRecord
            {
                DataloggerModel = match.Groups[1].Value,
                SerialNumber = match.Groups[2].Value,
                TableName = match.Groups[3].Value,
                JsonPayload = payload,
                Type = type // Store whether it was 'Data' or 'Metadata'
            };
            return true;
        }

        public override string ToString()
        {
            // Now the topic type is printed in the log
            return $"[{Type}] Model: {DataloggerModel}, Serial: {SerialNumber}, Table: {TableName}";
        }
    }

    // -------------------------------------------------------------------
    // These are the classes for parsing the 'Data' JSON PAYLOAD
    // -------------------------------------------------------------------
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
        
        // ... other properties can be added from your log ...
        [JsonPropertyName("fields")]
        public List<Field>? Fields { get; set; }
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
        public DateTime Time { get; set; }

        [JsonPropertyName("vals")]
        public List<double>? Vals { get; set; }
    }

    // -------------------------------------------------------------------
    // These are the NEW classes for parsing the 'Metadata' JSON PAYLOAD
    // -------------------------------------------------------------------
    public class MetadataPayload
    {
        [JsonPropertyName("fields")]
        public MetadataFields? Fields { get; set; }
    }

    public class MetadataFields
    {
        [JsonPropertyName("key")]
        public List<string>? Key { get; set; }

        [JsonPropertyName("definitions")]
        public Dictionary<string, List<object>>? Definitions { get; set; }
    }
}