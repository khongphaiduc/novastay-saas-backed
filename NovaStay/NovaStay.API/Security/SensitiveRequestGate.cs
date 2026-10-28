using System.Security.Cryptography;
using System.Text;

namespace NovaStay.API.Security;

// Per-process, bounded counters; HMAC keys keep email, phone and refresh tokens out of cache keys/logs.
public sealed class SensitiveRequestGate(TimeProvider clock) : IDisposable
{
    private readonly object sync = new();
    private readonly Dictionary<string, Counter> counters = [];
    private readonly byte[] secret = RandomNumberGenerator.GetBytes(32);
    private int checks;
    public string Key(string operation, params string[] parts) => operation + ":" +
        Convert.ToHexString(HMACSHA256.HashData(secret, Encoding.UTF8.GetBytes(string.Join('\0', parts))));

    public int BeginLogin(string key, int limit = 5)
    {
        lock (sync)
        {
            Cleanup();
            var now = clock.GetUtcNow();
            if (!counters.TryGetValue(key, out var counter) || counter.Expires <= now && counter.InFlight == 0)
            {
                if (counters.Count >= 10000) return 60;
                counters[key] = counter = new Counter { Expires = now.AddMinutes(5) };
            }
            if (counter.Count >= limit) return Seconds(counter.Expires);
            if (counter.Count + counter.InFlight >= limit) return 5;
            counter.InFlight++; return 0;
        }
    }
    public void EndLogin(string key, bool failed, bool succeeded)
    {
        lock (sync)
        {
            if (!counters.TryGetValue(key, out var counter)) return;
            counter.InFlight = Math.Max(0, counter.InFlight - 1);
            if (succeeded) counter.Count = 0;
            else if (failed) counter.Count++;
            if (counter.Count == 0 && counter.InFlight == 0) counters.Remove(key);
        }
    }

    public int Reserve(string key, int limit, TimeSpan window, TimeSpan? cooldown = null)
    {
        lock (sync)
        {
            Cleanup(); var now = clock.GetUtcNow();
            if (!counters.TryGetValue(key, out var counter) || counter.Expires <= now)
            {
                if (counters.Count >= 10000) return 60;
                counters[key] = counter = new Counter { Expires = now + window };
            }
            if (counter.Next > now) return Seconds(counter.Next);
            if (counter.Count >= limit) return Seconds(counter.Expires);
            counter.Count++; counter.Next = now + (cooldown ?? TimeSpan.Zero);
            return 0;
        }
    }
    private int Seconds(DateTimeOffset until) => Math.Max(1, (int)Math.Ceiling((until - clock.GetUtcNow()).TotalSeconds));
    private void Cleanup()
    {
        if (++checks % 64 != 0 && counters.Count < 10000) return;
        var now = clock.GetUtcNow();
        foreach (var key in counters.Where(x => x.Value.Expires <= now && x.Value.InFlight == 0).Select(x => x.Key).ToArray()) counters.Remove(key);
    }
    public void Dispose() { CryptographicOperations.ZeroMemory(secret); lock (sync) counters.Clear(); }
    private sealed class Counter { public DateTimeOffset Expires, Next; public int Count, InFlight; }
}
