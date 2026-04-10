namespace App.Extensions;

public static class EnumExtensions
{
    public static string ToLocalizedString(this Enum value)
    {
        string key = $"{value.GetType().Name}_{value}";
        string translated = Resources.Enums.ResourceManager.GetString(key);
        
        // Fall back to enum name if key not found
        return translated ?? value.ToString();
    }
}