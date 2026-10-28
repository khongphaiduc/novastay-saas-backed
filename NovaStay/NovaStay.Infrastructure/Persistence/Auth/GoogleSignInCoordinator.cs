using System.Security.Cryptography;
using System.Text;

namespace NovaStay.Infrastructure.Persistence.Auth;

// Bounded stripes serialize a verified identity without retaining user identifiers in a growing dictionary.
public sealed class GoogleSignInCoordinator : IDisposable
{
    private readonly SemaphoreSlim[] stripes = Enumerable.Range(0, 128).Select(_ => new SemaphoreSlim(1, 1)).ToArray();
    public async Task<IDisposable> EnterAsync(string subject, CancellationToken ct)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(subject));
        var stripe = stripes[hash[0] % stripes.Length];
        await stripe.WaitAsync(ct);
        return new Lease(stripe);
    }
    public void Dispose() { foreach (var stripe in stripes) stripe.Dispose(); }
    private sealed class Lease(SemaphoreSlim stripe) : IDisposable
    {
        private SemaphoreSlim? current = stripe;
        public void Dispose() => Interlocked.Exchange(ref current, null)?.Release();
    }
}
