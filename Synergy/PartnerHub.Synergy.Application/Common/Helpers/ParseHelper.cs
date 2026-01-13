using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.Synergy.Application.Common.Helpers;

public static class ParseHelper
{
    public static (string field, bool descending) ParseSort(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return ("createdat", true);

        var parts = sortBy.Split(':', StringSplitOptions.RemoveEmptyEntries);

        var field = parts[0].ToLower();
        var descending = parts.Length == 1
            || !parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase);

        return (field, descending);
    }
}
