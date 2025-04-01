using System;

public static class Protocolo
{
    public const string CONNECT = "CONNECT";
    public const string DISCONNECT = "DISCONNECT";
    public const string ACK = "ACK";
    public const string NACK = "NACK";
    public const string DATA = "DATA";
    public const string STATUS = "STATUS";
    public const char SEPARATOR = ':';

    public static string BuildConnectMessage(string wavyId) => 
        $"{CONNECT}{SEPARATOR}{wavyId}";

    public static string BuildDataMessage(string wavyId, string dataType, string data) => 
        $"{DATA}{SEPARATOR}{wavyId}{SEPARATOR}{dataType}{SEPARATOR}{data}";

    public static (string command, string[] parameters) ParseMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return ("UNKNOWN", Array.Empty<string>());

        var parts = message.Split(SEPARATOR);
        return (parts[0], parts.Skip(1).ToArray());
    }
}