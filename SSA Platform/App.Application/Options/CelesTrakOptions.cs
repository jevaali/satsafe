namespace App.Application.Options;

public class CelesTrakOptions
{
    public const string Section = "CelesTrak";

    /// <summary>
    /// When false the "Fetch from CelesTrak" button is hidden and the fetch action
    /// returns an error. Set to false before deployment to prevent live HTTP calls.
    /// </summary>
    public bool FetchEnabled { get; set; } = true;
}
