using Milvasoft.Caching.Redis;
using Milvasoft.Caching.Redis.Options;
using Milvasoft.Types;

namespace Milvasoft.Localization.Redis;

public class RedisLocalizationManager(IRedisAccessor redisService, IRedisCachingOptions redisCacheServiceOptions) : ILocalizationManager
{
    private readonly IRedisAccessor _redisService = redisService;
    private readonly IRedisCachingOptions _redisCacheServiceOptions = redisCacheServiceOptions;

    // TODO : Redise language code a göre prefix'le yazan methodlar eklenecek. Okuyan methodlarda buna göre konfigüre edilecek. 
    // IStringLocalizer dili nasıl set ediyor. RequestLocalization ile birlikte ve değilken nasıl yapıyor bunu incele.
    // Request localization olmadan da kendi başına bir localizasyon altyapısı olmalı. Bunu sadece defaultta headerdan alacak şekilde kurgula fakat flexible olsun ki ihtiyaca göre genişletilebilsin.
    // Request localization'a göre bir yapı yazılacak. WithRequestLocalization gibisinden. Bunun için IOptions<RequestLocalizationOptions> nesnesinden faydalanarak bişeyler yyapabilirsin. 
    // Aynı zamanda dili manuel değiştirilebilir olmalı 
    // GetContentByLanguage() methodu eklenecek. Güzel method lazım olabiliyor bazen. 

    /// <summary>
    /// Gets the string resource with the given key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The string resource as a <see cref="LocalizedValue"/>.</returns>
    public virtual LocalizedValue this[string key]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var value = _redisService.Get(key);

            return new LocalizedValue(key, value, value == null, GetSearchLocation(key));
        }
    }

    /// <summary>
    /// Gets the string resource with the given key and formatted with the supplied arguments.
    /// </summary>
    /// <param name="key">The key of the string resource.</param>
    /// <param name="arguments">The values to format the string with.</param>
    /// <returns>The formatted string resource as a <see cref="LocalizedValue"/>.</returns>
    public virtual LocalizedValue this[string key, params object[] arguments]
    {
        get
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var value = _redisService.Get(key);

            if (value != null)
            {
                value = string.Format(value, arguments);
            }

            return new LocalizedValue(key, value, value == null, GetSearchLocation(key));
        }
    }

    /// <summary>
    /// Gets all string resources.
    /// </summary>
    /// <param name="includeParentCultures">
    /// A <see cref="bool"/> indicating whether to include strings from parent cultures.
    /// </param>
    /// <returns>The strings.</returns>
    public IEnumerable<LocalizedValue> GetAllStrings(bool includeParentCultures)
    {
        var server = _redisService.GetServer(_redisCacheServiceOptions.ConfigurationOptions.EndPoints[0]);

        var keys = server.Keys().Select(k => k.ToString());
        var values = _redisService.Get(keys);

        return values.Select(v => new LocalizedValue(v, v, !v.HasValue, GetSearchLocation("keys[]")));
    }

    private string GetSearchLocation(string key) => $"{_redisCacheServiceOptions.ConfigurationOptions.EndPoints[0]}_{key}";
}
