using Fody;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using Milvasoft.FileOperations.Abstract;
using Milvasoft.FileOperations.Builder;
using Newtonsoft.Json;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;

namespace Milvasoft.FileOperations.Concrete;

/// <summary>
/// Provides add-edit-delete-get process json files that hold one list.
/// 
/// <para> Default CultureInfo is en-US </para>
/// <para> Default Encoding is UTF8 </para>
/// 
/// <para> Example json file template for use this library methods; </para>
/// <code>
/// 
/// <para> <b>Structure 1 :</b></para>
/// 
///  "connectionstring"
/// 
/// <para> <b>Structure 2 :</b></para>
/// [
///  {
///    "Id": 1,
///    "Name": "poco1",
///    "Which": false
///  },
///  {
///    "Id": 2,
///    "Name": "poco2",
///    "Which": true
///  },
///  {
///    "Id": 3,
///    "Name": "poco3",
///    "Which": true
///  }
/// ]
/// 
/// <para> <b>Structure 3 :</b></para>
/// 
///  {
///    "Id": 1,
///    "Name": "poco1",
///    "Which": false
///  },
///  {
///    "Id": 2,
///    "Name": "poco2",
///    "Which": true
///  },
///  {
///    "Id": 3,
///    "Name": "poco3",
///    "Which": true
///  }
/// 
/// </code>
/// </summary>
[ConfigureAwait(false)]
public class JsonOperations : IJsonOperations
{
    private readonly string _basePath;
    private string _encryptionKey;
    private Encoding _encoding;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    #region Consructors

    /// <summary>
    /// Initializes new instance of <see cref="JsonOperations"/>.
    /// </summary>
    public JsonOperations()
    {
        _encoding = Encoding.UTF8;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("en-US")
        };
    }

    /// <summary>
    /// Initializes new instance of <see cref="JsonOperations"/> with <see cref="IJsonFileOperationOptions"/>.
    /// </summary>
    public JsonOperations(IJsonFileOperationOptions jsonOperationsConfig)
    {
        _basePath = jsonOperationsConfig.BasePath;
        _encryptionKey = jsonOperationsConfig.EncryptionKey;
        _encoding = jsonOperationsConfig.Encoding;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = jsonOperationsConfig.CultureInfo
        };
    }

    #endregion

    /// <summary>
    /// If you change culture info after object creation, use this method.
    /// </summary>
    /// <param name="cultureInfo"></param>
    public void SetCultureInfo(CultureInfo cultureInfo) => _jsonSerializerSettings.Culture = cultureInfo;

    /// <summary>
    /// If you change encryption key after object creation, use this method.
    /// </summary>
    /// <param name="encryptionKey"></param>
    public void SetEncryptionKey(string encryptionKey) => _encryptionKey = encryptionKey;

    /// <summary>
    /// If you change encoding after object creation, use this method.
    /// </summary>
    /// <param name="encoding"></param>
    public void SetEncoding(Encoding encoding) => _encoding = encoding;

    #region Async

    /// <summary>
    /// Gets all content from json file in <paramref name="filePath"/>. 
    /// Returns them as the requested type.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content as type <typeparamref name="T"/>. </returns>
    public async Task<T> GetContentAsync<T>(string filePath)
    {
        filePath = GetFilePath(filePath);

        var jsonContent = await File.ReadAllTextAsync(filePath, _encoding);

        return JsonConvert.DeserializeObject<T>(jsonContent, _jsonSerializerSettings);
    }

    /// <summary>
    /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="content"> Content to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
    /// <returns></returns>
    public async Task AddContentAsync<T>(T content, string filePath, bool contentsHasId = false)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForAdd(new List<T> { content }, jsonContentString, contentsHasId);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> If content has "Id" property which type is "System.Int32". Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
    /// <returns></returns>
    public async Task AddContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId = false)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForAdd(contents, jsonContentString, contentsHasId);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Updates content from json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="content"/>. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task UpdateContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForUpdate([content], jsonContentString, mappingProperty);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/>! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/>.</exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task UpdateContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForUpdate(contents, jsonContentString, mappingProperty);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task DeleteContentAsync<T>(List<dynamic> mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Deletes records from requested json file.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task DeleteContentsAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = await File.ReadAllTextAsync(filePath, _encoding);

        string newJsonResult = GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty);

        await File.WriteAllTextAsync(filePath, newJsonResult);
    }

    /// <summary>
    /// Removes all file data and writes new one.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns></returns>
    public async Task ReplaceOldContentWithNewAsync<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        await File.WriteAllTextAsync(GetFilePath(filePath), newJsonResult);
    }

    /// <summary>
    /// Clears json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns></returns>
    public async Task ClearJSONFileAsync(string filePath) => await File.WriteAllTextAsync(GetFilePath(filePath), "");

    #region With Encryption 

    /// <summary>
    /// Gets content from crypted json file in <paramref name="filePath"/>. 
    /// Returns them as the requested list of type.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    ///
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    ///
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content list of type <typeparamref name="T"/>. </returns>
    public async Task<T> GetCryptedContentAsync<T>(string filePath)
    {
        var jsonContent = await DecryptAndReadAsync(GetFilePath(filePath), _encryptionKey);

        return JsonConvert.DeserializeObject<T>(jsonContent, _jsonSerializerSettings);
    }

    /// <summary>
    /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
    /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public async Task AddCryptedContentAsync<T>(T content, string filePath, bool contentsHasId)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForAdd(new List<T> { content }, jsonContentString, contentsHasId);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
    /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public async Task AddCryptedContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForAdd(contents, jsonContentString, contentsHasId);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.</exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="content"/> .</exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task UpdateCryptedContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForUpdate([content], jsonContentString, mappingProperty);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/>. </exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public async Task UpdateCryptedContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForUpdate(contents, jsonContentString, mappingProperty);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public async Task DeleteCryptedContentAsync<T>(dynamic mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public async Task DeleteCryptedContentAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = await DecryptAndReadAsync(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Removes all file data and writes new one.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>   
    /// <returns> Completed <see cref="Task"/> </returns>
    public async Task ReplaceCryptedOldContentWithNewAsync<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        await EncryptAndWriteAsync(filePath, newJsonResult, _encryptionKey);
    }

    #endregion

    #endregion

    #region Sync

    /// <summary>
    /// Gets all content from json file in <paramref name="filePath"/>. 
    /// Returns them as the requested type.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content as type <typeparamref name="T"/>. </returns>
    public T GetContent<T>(string filePath)
    {
        filePath = GetFilePath(filePath);

        var jsonContent = File.ReadAllText(filePath, _encoding);

        return JsonConvert.DeserializeObject<T>(jsonContent, _jsonSerializerSettings);
    }

    /// <summary>
    /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="content"> Content to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
    /// <returns></returns>
    public void AddContent<T>(T content, string filePath, bool contentsHasId = false)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForAdd(new List<T> { content }, jsonContentString, contentsHasId);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> If content has "Id" property which type is "System.Int32". Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
    /// <returns></returns>
    public void AddContents<T>(List<T> contents, string filePath, bool contentsHasId = false)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForAdd(contents, jsonContentString, contentsHasId);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Updates content from json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="content"/>. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateContent<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForUpdate([content], jsonContentString, mappingProperty);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/>! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/>.</exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateContents<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForUpdate(contents, jsonContentString, mappingProperty);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void DeleteContent<T>(List<dynamic> mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Deletes records from requested json file.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void DeleteContents<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        filePath = GetFilePath(filePath);

        var jsonContentString = File.ReadAllText(filePath, _encoding);

        string newJsonResult = GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty);

        File.WriteAllText(filePath, newJsonResult);
    }

    /// <summary>
    /// Removes all file data and writes new one.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns></returns>
    public void ReplaceOldContentWithNew<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        File.WriteAllText(GetFilePath(filePath), newJsonResult);
    }

    /// <summary>
    /// Clears json file in <paramref name="filePath"/>.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// 
    /// </remarks>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns></returns>
    public void ClearJSONFile(string filePath) => File.WriteAllText(GetFilePath(filePath), "");

    #region With Encryption 

    /// <summary>
    /// Gets content from crypted json file in <paramref name="filePath"/>. 
    /// Returns them as the requested list of type.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    ///
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    ///
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content list of type <typeparamref name="T"/>. </returns>
    public T GetCryptedContent<T>(string filePath)
    {
        var jsonContent = DecryptAndRead(GetFilePath(filePath), _encryptionKey);

        return JsonConvert.DeserializeObject<T>(jsonContent, _jsonSerializerSettings);
    }

    /// <summary>
    /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
    /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public void AddCryptedContent<T>(T content, string filePath, bool contentsHasId)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForAdd(new List<T> { content }, jsonContentString, contentsHasId);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
    /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public void AddCryptedContents<T>(List<T> contents, string filePath, bool contentsHasId)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForAdd(contents, jsonContentString, contentsHasId);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.</exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="content"/> .</exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateCryptedContent<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForUpdate([content], jsonContentString, mappingProperty);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Updates contents from json file in <paramref name="filePath"/>.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> ! </para>
    /// <para> Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="Exception()"> Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>. </exception>
    /// <exception cref="Exception()"> Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/>. </exception>
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateCryptedContents<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForUpdate(contents, jsonContentString, mappingProperty);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in the class remarks. </para> 
    /// <para> Don't send <typeparamref name="T"/> as <see cref="List{T}"/>. You can use <see cref="List{T}"/> overload. </para>
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public void DeleteCryptedContent<T>(dynamic mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Deletes record from requested json file.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// <remarks>
    /// 
    /// <para> This method supports only structure 2 in class remarks. </para> 
    /// <para> This method reads all file. This can cause performance impact with big files. </para> 
    /// 
    /// </remarks>
    /// 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when encryption key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when encryption key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public void DeleteCryptedContent<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        var jsonContentString = DecryptAndRead(filePath, _encryptionKey);

        string newJsonResult = GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    /// <summary>
    /// Removes all file data and writes new one.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
    /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>   
    /// <returns> Completed <see cref="Task"/> </returns>
    public void ReplaceCryptedOldContentWithNew<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        EncryptAndWrite(filePath, newJsonResult, _encryptionKey);
    }

    #endregion

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Gets last id from <paramref name="contentList"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lastId"></param>
    /// <param name="contentList"></param>
    public void GetLastIdIfHasId<T>(out int? lastId, List<T> contentList) => lastId = !contentList.IsNullOrEmpty()
                                                                                      ? (int?)contentList.Max(i => i.GetType().GetProperty("Id").GetValue(i, null))
                                                                                      : null;

    #endregion

    #region Private Helper Methods

    private string GetJsonResultForAdd<T>(List<T> contents, string jsonContentString, bool contentsHasId = false)
    {
        var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, _jsonSerializerSettings);

        var jsonContent = string.IsNullOrWhiteSpace(jsonContentString) ? [] : tempJsonContent;

        if (contentsHasId)
        {
            int? lastId;

            if (jsonContent.Count == 0)
                lastId = 0;
            else
                GetLastIdIfHasId(out lastId, jsonContent);

            foreach (var content in contents)
            {
                lastId++;
                content.GetType().GetProperty("Id").SetValue(content, lastId);
                jsonContent.Add(content);
            }
        }
        else
        {
            foreach (var content in contents)
                jsonContent.Add(content);
        }

        return JsonConvert.SerializeObject(jsonContent, Formatting.Indented);
    }

    private string GetJsonResultForUpdate<T>(List<T> contents, string jsonContentString, Expression<Func<T, dynamic>> mappingProperty)
    {
        var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, _jsonSerializerSettings);

        var jsonContents = string.IsNullOrWhiteSpace(jsonContentString) ? throw new MilvaDeveloperException("No Records for Update") : tempJsonContent;

        var lastContent = jsonContents.LastOrDefault();

        var mappingPropertyName = mappingProperty.GetPropertyName();

        if (lastContent.GetType().GetProperty(mappingPropertyName) == null)
            throw new MilvaDeveloperException($"This content type not have {mappingPropertyName} property");

        var upToDateContents = new List<T>();
        foreach (var content in contents)
        {
            var matchedContent = jsonContents.Find(c => c.GetType().GetProperty(mappingPropertyName).GetValue(c)
                                                            .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content)));

            if (matchedContent != null)
            {
                int index = jsonContents.IndexOf(matchedContent);
                if (index != -1)
                    jsonContents[index] = content;

                upToDateContents.Add(matchedContent);
            }
        }

        if (upToDateContents.Count == 0)
            throw new MilvaDeveloperException("Requested content for update not found!");

        return JsonConvert.SerializeObject(jsonContents, Formatting.Indented);
    }

    private string GetJsonResultForDelete<T>(List<dynamic> mappingValues, string jsonContentString, Expression<Func<T, dynamic>> mappingProperty)
    {
        var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, _jsonSerializerSettings);

        var jsonContent = string.IsNullOrWhiteSpace(jsonContentString) ? throw new MilvaDeveloperException("No Records for Delete") : tempJsonContent;

        var lastContent = jsonContent.LastOrDefault();

        var mappingPropertyName = mappingProperty.GetPropertyName();

        if (lastContent.GetType().GetProperty(mappingPropertyName) == null)
            throw new MilvaDeveloperException($"This content type not have {mappingPropertyName} property ");

        var willRemovedContents = new List<T>();

        foreach (var mappingValue in mappingValues)
        {
            var matchedContent = jsonContent.Find(c => c.GetType().GetProperty(mappingPropertyName).GetValue(c, null).Equals(mappingValue));

            if (matchedContent != null)
                willRemovedContents.Add(matchedContent);
        }

        if (willRemovedContents.Count == 0)
            throw new MilvaDeveloperException("Requested content for delete not found!");

        foreach (var willRemovedContent in willRemovedContents)
            jsonContent.Remove(willRemovedContent);

        return JsonConvert.SerializeObject(jsonContent, Formatting.Indented);
    }

    private static async Task<string> DecryptAndReadAsync(string filePath, string key)
    {
        var encryptionProvider = new MilvaCryptographyProvider(new MilvaCryptographyOptions { Key = key });

        var inputValue = await File.ReadAllTextAsync(filePath);

        return encryptionProvider.Decrypt(inputValue);
    }

    private static async Task EncryptAndWriteAsync(string filePath, string content, string key)
    {
        var encryptionProvider = new MilvaCryptographyProvider(new MilvaCryptographyOptions { Key = key });

        var encryptedContent = encryptionProvider.Encrypt(content);

        await File.WriteAllTextAsync(filePath, encryptedContent, Encoding.UTF8);
    }

    private static string DecryptAndRead(string filePath, string key)
    {
        var plainContent = File.ReadAllBytes(filePath);

        using var algorithm = Aes.Create();

        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length != 16)
            throw new ArgumentOutOfRangeException(key, "Key is not proper length. Key bit length must be 16.");

        algorithm.IV = keyBytes;
        algorithm.Key = keyBytes;
        algorithm.Mode = CipherMode.CBC;
        algorithm.Padding = PaddingMode.PKCS7;

        using var memStream = new MemoryStream();

        using var cryptoStream = new CryptoStream(memStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write);

        cryptoStream.Write(plainContent, 0, plainContent.Length);

        try
        {
            cryptoStream.FlushFinalBlock();
        }
        catch (Exception)
        {
            throw new ArgumentException("Incorrect key.");
        }

        using var streamReader = new StreamReader(memStream);

        memStream.Position = 0;

        return streamReader.ReadToEnd();
    }

    private static void EncryptAndWrite(string filePath, string content, string key)
    {
        byte[] plainContent = Encoding.ASCII.GetBytes(content);

        using var algorithm = Aes.Create();

        var keyBytes = Encoding.UTF8.GetBytes(key);

        if (keyBytes.Length != 16)
            throw new ArgumentOutOfRangeException(key, "Key is not proper length. Key bit length must be 16.");

        algorithm.IV = keyBytes;
        algorithm.Key = keyBytes;
        algorithm.Mode = CipherMode.CBC;
        algorithm.Padding = PaddingMode.PKCS7;

        using var memStream = new MemoryStream();

        using var cryptoStream = new CryptoStream(memStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

        cryptoStream.Write(plainContent, 0, plainContent.Length);
        cryptoStream.FlushFinalBlock();

        File.WriteAllBytes(filePath, memStream.ToArray());
    }

    private string GetFilePath(string filePath) => !string.IsNullOrWhiteSpace(_basePath) ? Path.Combine(_basePath, filePath) : filePath;

    #endregion
}
