namespace Interpretarr.Clients.Sonarr.Model.Command
{
    using System;
    using J = Newtonsoft.Json.JsonPropertyAttribute;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class CommandResource
    {
        [J("name")] public string Name { get; set; }
        [J("commandName")] public string CommandName { get; set; }
        [J("body")] public Body Body { get; set; }
        [J("priority")] public string Priority { get; set; }
        [J("status")] public string Status { get; set; }
        [J("queued")] public DateTimeOffset Queued { get; set; }
        [J("trigger")] public string Trigger { get; set; }
        [J("clientUserAgent")] public string ClientUserAgent { get; set; }
        [J("sendUpdatesToClient")] public bool SendUpdatesToClient { get; set; }
        [J("updateScheduledTask")] public bool UpdateScheduledTask { get; set; }
        [J("id")] public long Id { get; set; }
    }

    public partial class Body
    {
        [J("path")] public string Path { get; set; }
        [J("importMode")] public string ImportMode { get; set; }
        [J("sendUpdatesToClient")] public bool SendUpdatesToClient { get; set; }
        [J("updateScheduledTask")] public bool UpdateScheduledTask { get; set; }
        [J("completionMessage")] public string CompletionMessage { get; set; }
        [J("requiresDiskAccess")] public bool RequiresDiskAccess { get; set; }
        [J("isExclusive")] public bool IsExclusive { get; set; }
        [J("name")] public string Name { get; set; }
        [J("trigger")] public string Trigger { get; set; }
        [J("suppressMessages")] public bool SuppressMessages { get; set; }
        [J("clientUserAgent")] public string ClientUserAgent { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
