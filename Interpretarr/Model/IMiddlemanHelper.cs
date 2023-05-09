namespace Interpretarr.Model
{
    public interface IMiddlemanHelper
    {
        SupportedSite SupportedSite { get; }

        string HandleResponse(string path, string responseContent);

        bool HandleRequest(string path, out string updatedPath);

        bool ParseReleaseTitle(string releaseTitle, out string updatedTitle);
    }
}
