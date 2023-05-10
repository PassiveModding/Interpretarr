namespace Interpretarr.Model
{
    public interface IMiddlemanHelper
    {
        SupportedSite[] SupportedSites { get; }

        // For parsing sonarr queries into a format that can be used by the indexer

        Task<FormatResult> FormatKeywordsAsync(string keywords);

        // For parsing the release title into a format that can be used by sonarr

        Task<FormatResult> FormatReleaseTitleAsync(string releaseTitle);
    }

    public class FormatResult
    {
        public FormatResult(bool success, string? result)
        {
            Success = success;
            Result = result;
        }

        public bool Success { get; set; }
        public string? Result { get; set; }
    }
}
