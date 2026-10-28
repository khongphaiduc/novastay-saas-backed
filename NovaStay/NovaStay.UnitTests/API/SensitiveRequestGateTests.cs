using NovaStay.API.Security;

namespace NovaStay.UnitTests.API;

public class SensitiveRequestGateTests
{
    private sealed class Clock : TimeProvider
    {
        public DateTimeOffset Now = new(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
    [Fact]
    public void ResetCooldownAndHourlyBudget_AreAtomicAndExpire()
    {
        var clock = new Clock(); using var gate = new SensitiveRequestGate(clock);
        var key = gate.Key("reset", "person@example.invalid");
        Assert.DoesNotContain("person", key);
        for (var n = 0; n < 5; n++)
        {
            Assert.Equal(0, gate.Reserve(key, 5, TimeSpan.FromHours(1), TimeSpan.FromMinutes(2)));
            Assert.Equal(120, gate.Reserve(key, 5, TimeSpan.FromHours(1), TimeSpan.FromMinutes(2)));
            clock.Now = clock.Now.AddMinutes(2);
        }
        Assert.True(gate.Reserve(key, 5, TimeSpan.FromHours(1)) > 0);
        clock.Now = clock.Now.AddHours(1);
        Assert.Equal(0, gate.Reserve(key, 5, TimeSpan.FromHours(1)));
    }
    [Fact]
    public void ParallelPasswordRequests_CannotBypassFailureBudget_AndSuccessClearsFailures()
    {
        var clock = new Clock(); using var gate = new SensitiveRequestGate(clock);
        var key = gate.Key("login", "192.0.2.10", "0900000001");
        var outcomes = new int[20]; Parallel.For(0, 20, n => outcomes[n] = gate.BeginLogin(key));
        Assert.Equal(5, outcomes.Count(x => x == 0));
        for (var n = 0; n < 5; n++) gate.EndLogin(key, true, false);
        Assert.Equal(300, gate.BeginLogin(key));
        clock.Now = clock.Now.AddMinutes(6); Assert.Equal(0, gate.BeginLogin(key));
        gate.EndLogin(key, false, true); Assert.Equal(0, gate.BeginLogin(key));
    }
    [Fact]
    public void RefreshKeysDoNotRetainRawTokens_AndCountersAreIndependent()
    {
        using var gate = new SensitiveRequestGate(TimeProvider.System);
        var key = gate.Key("refresh", "test-only-refresh-token");
        Assert.DoesNotContain("test-only-refresh-token", key);
        for (var n = 0; n < 30; n++) Assert.Equal(0, gate.Reserve(key, 30, TimeSpan.FromMinutes(1)));
        Assert.True(gate.Reserve(key, 30, TimeSpan.FromMinutes(1)) > 0);
        Assert.Equal(0, gate.Reserve(gate.Key("refresh", "another-token"), 30, TimeSpan.FromMinutes(1)));
    }
}
