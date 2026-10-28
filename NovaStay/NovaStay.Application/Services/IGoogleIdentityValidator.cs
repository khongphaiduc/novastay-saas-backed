namespace NovaStay.Application.Services;

public sealed record GoogleIdentity(string Subject, string Email, string Name);

public interface IGoogleIdentityValidator
{
    Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
