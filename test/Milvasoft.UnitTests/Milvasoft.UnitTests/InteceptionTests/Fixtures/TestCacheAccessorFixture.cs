using Milvasoft.Core.Abstractions.Cache;

namespace Milvasoft.UnitTests.InteceptionTests.Fixtures;

public class TestCacheAccessorFixture : ICacheAccessor<TestCacheAccessorFixture>
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public Dictionary<string, object> Cache { get; set; } = [];

    public async Task<bool> SetAsync(string key, object value) => Cache.TryAdd("Test", value);
    public async Task<bool> SetAsync(string key, object value, TimeSpan? expiration) => Cache.TryAdd("Test", value);
    public async Task<T> GetAsync<T>(string key) where T : class => Cache.Count != 0 ? (T)Cache["Test"] : null;
    public async Task<object> GetAsync(string key, Type returnType) => Cache.Count != 0 ? Cache["Test"] : null;
    public async Task<string> GetAsync(string key) => throw new NotImplementedException();
    public Task<IEnumerable<T>> GetAsync<T>(IEnumerable<string> keys) => throw new NotImplementedException();
    public T Get<T>(string key) where T : class => Cache.Count != 0 ? (T)Cache["Test"] : null;
    public string Get(string key) => throw new NotImplementedException();
    public IEnumerable<T> Get<T>(IEnumerable<string> keys) => throw new NotImplementedException();
    public bool IsConnected() => throw new NotImplementedException();
    public bool KeyExists(string key) => throw new NotImplementedException();
    public Task<bool> KeyExistsAsync(string key) => throw new NotImplementedException();
    public bool KeyExpire(string key, TimeSpan expiration) => throw new NotImplementedException();
    public bool KeyExpire(string key, DateTime? expiration) => throw new NotImplementedException();
    public Task<bool> KeyExpireAsync(string key, TimeSpan expiration) => throw new NotImplementedException();
    public Task<bool> KeyExpireAsync(string key, DateTime? expiration) => throw new NotImplementedException();
    public bool Remove(string key) => throw new NotImplementedException();
    public Task<bool> RemoveAsync(string key) => throw new NotImplementedException();
    public Task<long> RemoveAsync(IEnumerable<string> keys) => throw new NotImplementedException();
    public bool Set(string key, string value) => throw new NotImplementedException();
    public bool Set<T>(string key, T value) where T : class => throw new NotImplementedException();
    public bool Set(string key, object value, TimeSpan? expiration) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
}