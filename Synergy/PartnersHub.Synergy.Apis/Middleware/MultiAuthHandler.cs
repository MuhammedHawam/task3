using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;


namespace PartnersHub.Synergy.Apis.Middleware;
public class  MultiAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IAuthenticationHandlerProvider _handlerProvider;
    public MultiAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAuthenticationSchemeProvider schemeProvider,
        IAuthenticationHandlerProvider handlerProvider)
        : base(options, logger, encoder, clock)
    {
        _schemeProvider = schemeProvider;
        _handlerProvider = handlerProvider;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var schemesToTry = new[]
        {
        "ActiveDirectoryScheme",
        "CiamSsoScheme",
        "ExternalPortalScheme"
    };

        foreach (var scheme in schemesToTry)
        {
            var handler = await _handlerProvider.GetHandlerAsync(Context, scheme);
            if (handler is IAuthenticationHandler authHandler)
            {
                var result = await authHandler.AuthenticateAsync();
                if (result.Succeeded)
                    return result; // ✔ return as soon as one scheme succeeds
            }
        }

        return AuthenticateResult.Fail("No valid authentication scheme");
    }

}