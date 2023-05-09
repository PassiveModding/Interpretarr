namespace Interpretarr.Clients.Sonarr.Model.CommandPost
{
    using J = Newtonsoft.Json.JsonPropertyAttribute;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class CommandPostResource
    {
        [J("name")] public string Name { get; set; }
        [J("path")] public string Path { get; set; }
        [J("importMode")] public string ImportMode { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
