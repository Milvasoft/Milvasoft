using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace Milvasoft.Localization.Resx.ResxManipulator;

/// <summary>
/// Create and write to resx resource files
/// </summary>
public class ResxWriter
{
    private readonly XDocument _xd;
    private readonly ILogger _logger;
    private readonly string _resourceFilePath;

    /// <summary>
    /// Create a new instance of <see cref="ResxWriter"/>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="location"></param>
    /// <param name="culture"></param>
    /// <param name="loggerFactory"></param>
    public ResxWriter(Type type, string culture, string resourcePath, ILogger logger)
    {
        _resourceFilePath = Path.Combine(resourcePath, $"{type.Name}.{culture}.resx");

        if (!File.Exists(_resourceFilePath))
            CreateNewResxFile(type);

        _xd = XDocument.Load(_resourceFilePath);
        _logger = logger;
        _logger?.LogInformation($"Resource file loaded: '{_resourceFilePath}'");
    }

    private bool CreateNewResxFile(Type type)
    {
        bool success;


        try
        {
            // Create a copy of the template resx resource
            var resxTemplate = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Milvasoft.Localization.Resx.ResxManipulator.Templates.ResxTemplate.xml");

            _logger?.LogInformation($"ResxTemplate: {resxTemplate == null}");

            using (Stream file = File.Create(_resourceFilePath))
                resxTemplate.CopyTo(file);

            _logger?.LogInformation($"Resx file created: '{_resourceFilePath}'");

            success = true;
        }
        catch (Exception e)
        {
            throw new FileLoadException($"Can't create resource file. {e.Message}");
        }

        return success;
    }

    /// <summary>
    /// Add array of elements to the resource file
    /// </summary>
    /// <param name="elements"></param>
    /// <param name="overWriteExistingKeys"></param>
    /// <returns></returns>
    public async Task<int> AddRangeAsync(IEnumerable<ResxElement> elements, bool overWriteExistingKeys)
    {
        var total = 0;

        foreach (var e in elements.Distinct())
        {
            if (string.IsNullOrWhiteSpace(e.Key) || string.IsNullOrWhiteSpace(e.Value))
                continue;

            var success = await AddAsync(e, overWriteExistingKeys);

            if (success)
                total++;
        }

        return total;
    }

    /// <summary>
    /// Add an element to the resource file
    /// </summary>
    /// <param name="element"></param>
    /// <param name="overWriteExistingKeys"></param>
    /// <returns></returns>
    public async Task<bool> AddAsync(ResxElement element, bool overWriteExistingKeys = false)
    {
        // Look for an existing element with the same key
        var elmnt = await FindAsync(element.Key);
        var tsk = new TaskCompletionSource<bool>();

        // If no similar element add the new one
        if (elmnt == null)
        {
            try
            {
                _xd.Root.Add(element.ToXElement());
                await SaveAsync();
                tsk.SetResult(true);
            }
            catch (Exception e)
            {
                _logger?.LogError("Error while adding element to resource file.");
                _logger?.LogError(e.Message);
                tsk.SetException(e);
                tsk.SetResult(false);
                return await tsk.Task;
            }
        }

        // If a similar element is existing and overwrite = false
        if (elmnt != null && overWriteExistingKeys == false)
        {
            tsk.SetResult(false);
        }

        // If a similar element is existing and overwrite = true
        if (elmnt != null && overWriteExistingKeys == true)
        {
            try
            {
                _xd.Root.Elements("data").FirstOrDefault(x => x == elmnt).ReplaceWith(element.ToXElement());
                await SaveAsync();
                tsk.SetResult(true);
            }
            catch (Exception e)
            {
                _logger?.LogError("Resource exporting error! An error occord during adding element to resx file.");
                _logger?.LogError(e.Message);
                tsk.SetException(e);
                tsk.TrySetResult(false);
            }
        }

        return await tsk.Task;
    }

    /// <summary>
    /// Add an element to the resource file
    /// </summary>
    /// <param name="element"></param>
    /// <param name="overWriteExistingKeys"></param>
    /// <returns></returns>
    public async Task<bool> RemoveAsync(ResxElement element)
    {
        // Look for an existing element with the same key
        var elmnt = await FindAsync(element.Key);
        var tsk = new TaskCompletionSource<bool>();

        // If no similar element add the new one
        if (elmnt == null)
            return await tsk.Task;

        try
        {
            _xd.Root.Elements("data").FirstOrDefault(x => x == elmnt).Remove();
            await SaveAsync();
            tsk.SetResult(true);
        }
        catch (Exception e)
        {
            _logger?.LogError("Resource removing error! An error occord during removing element from resx file.");
            _logger?.LogError(e.Message);

            tsk.SetException(e);
            tsk.TrySetResult(false);
        }

        return await tsk.Task;
    }

    /// <summary>
    /// Find resource by its key value
    /// </summary>
    /// <param name="key"></param>
    /// <returns>XElement</returns>
    public async Task<XElement> FindAsync(string key)
    {
        var tsk = new TaskCompletionSource<XElement>();

        await Task.Run(() =>
        {
            var elmnt = _xd.Root.Descendants("data").FirstOrDefault(x => x.Attribute("name").Value.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (elmnt == null)
                tsk.SetResult(null);

            else
                tsk.SetResult(elmnt);
        });

        return await tsk.Task;
    }

    /// <summary>
    /// save the resource file
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SaveAsync()
    {
        var tsk = new TaskCompletionSource<bool>();

        await Task.Run(() =>
        {
            try
            {
                _xd.Save(_resourceFilePath);
                tsk.SetResult(true);
            }
            catch (Exception e)
            {
                _logger?.LogError(e.Message);
                tsk.SetResult(false);
            }
        });

        return await tsk.Task;
    }
}
