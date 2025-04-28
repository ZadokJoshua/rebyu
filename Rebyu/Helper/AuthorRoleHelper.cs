using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;

namespace Rebyu.Helper;

internal static class AuthorRoleHelper
{
    private static readonly Dictionary<string, AuthorRole> _roleMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "system", AuthorRole.System },
            { "assistant", AuthorRole.Assistant },
            { "user", AuthorRole.User },
            { "tool", AuthorRole.Tool }
        };

    public static AuthorRole? FromString(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _roleMap.TryGetValue(name, out var role) ? role : null;
    }
}
