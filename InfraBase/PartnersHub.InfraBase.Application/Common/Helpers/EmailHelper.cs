namespace PartnersHub.InfraBase.Application.Common.Helpers;

public static class EmailHelper
{
    public static string ExtractNameFromEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return "User";
        
        // Extract name from email (part before @) and format it
        var parts = email.Split('@');
        if (parts.Length > 0)
        {
            var namePart = parts[0];
            // Replace dots and underscores with spaces, and capitalize
            namePart = namePart.Replace('.', ' ').Replace('_', ' ');
            // Capitalize first letter of each word
            var words = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Join(" ", words);
        }
        
        return email;
    }
}
