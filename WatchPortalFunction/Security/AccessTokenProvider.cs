namespace Security
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Http;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// Validates a incoming request and extracts any <see cref="ClaimsPrincipal"/> contained within the bearer token.
    /// </summary>
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string AuthHeaderName = "Authorization";
        private const string BearerPrefix = "Bearer ";
        private readonly string audience;
        private readonly string issuer;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> configurationManager;

        public AccessTokenProvider(string audience, string issuer)
        {
            this.audience = audience;
            this.issuer = issuer;

            var documentRetriever = new HttpDocumentRetriever();
            documentRetriever.RequireHttps = issuer.StartsWith("https://");

            this.configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{issuer}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public async Task<ClaimsPrincipal> ValidateToken(HttpRequest request)
        {
            if (request == null 
                || !request.Headers.ContainsKey(AuthHeaderName) 
                || !request.Headers[AuthHeaderName].ToString().StartsWith(BearerPrefix))
            {
                return null;
            }

            for (int tries = 0; tries < 2; tries++)
            {
                try 
                {
                    var token = request.Headers[AuthHeaderName].ToString().Substring(BearerPrefix.Length);
                    var config = await this.configurationManager.GetConfigurationAsync(CancellationToken.None);
                    var tokenParams = new TokenValidationParameters()
                    {
                        RequireSignedTokens = true,
                        ValidAudience = this.audience,
                        ValidateAudience = true,
                        ValidIssuer = this.issuer,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKeys = config.SigningKeys
                    };

                    var handler = new JwtSecurityTokenHandler();
                    return handler.ValidateToken(token, tokenParams, out var securityToken);
                }
                catch (SecurityTokenSignatureKeyNotFoundException)
                {
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    this.configurationManager.RequestRefresh();
                }
                catch (SecurityTokenException)
                {
                    break;
                }
            }
                 
            return null;
        }
    }
}
