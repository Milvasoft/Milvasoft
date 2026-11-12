using Microsoft.IO;
using Milvasoft.Cryptography.Builder;
using Milvasoft.Cryptography.Concrete;
using Milvasoft.FileOperations.Abstract;
using Milvasoft.FileOperations.Builder;
using Newtonsoft.Json;
using System.Collections.Concurrent;
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
public class JsonOperations : IJsonOperations
{
    private readonly string _basePath;
    private string _encryptionKey;
    private MilvaCryptographyProvider _cryptographyProvider;
    private Encoding _encoding;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

    #region Consructors

    /// <summary>
    /// Initializes new instance of <see cref="JsonOperations"/>.
    /// </summary>
    public JsonOperations(RecyclableMemoryStreamManager recyclableMemoryStreamManager)
    {
        _encoding = Encoding.UTF8;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("en-US")
        };
        _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
    }

    /// <summary>
    /// Initializes new instance of <see cref="JsonOperations"/> with <see cref="IJsonFileOperationOptions"/>.
    /// </summary>
    public JsonOperations(IJsonFileOperationOptions jsonOperationsConfig, RecyclableMemoryStreamManager recyclableMemoryStreamManager)
    {
        _basePath = jsonOperationsConfig.BasePath;
        _encryptionKey = jsonOperationsConfig.EncryptionKey;
        _encoding = jsonOperationsConfig.Encoding;
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            Culture = jsonOperationsConfig.CultureInfo
        };
        _recyclableMemoryStreamManager = recyclableMemoryStreamManager;
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
    public void SetEncryptionKey(string encryptionKey)
    {
        _encryptionKey = encryptionKey;
        _cryptographyProvider = null;
    }

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
        var jsonContent = await ReadAsync(filePath, encrypted: false);

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
    public Task AddContentAsync<T>(T content, string filePath, bool contentsHasId = false)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForAdd([content], jsonContentString, contentsHasId));

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
    public Task AddContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId = false)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForAdd(contents, jsonContentString, contentsHasId));

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
    public Task UpdateContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForUpdate([content], jsonContentString, mappingProperty));

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
    public Task UpdateContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForUpdate(contents, jsonContentString, mappingProperty));

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
    public Task DeleteContentAsync<T>(List<dynamic> mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForDelete(mappingValue, jsonContentString, mappingProperty));

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
    public Task DeleteContentsAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: false, jsonContentString => GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty));

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
    public Task ReplaceOldContentWithNewAsync<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        return WriteAsync(filePath, newJsonResult, encrypted: false);
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
    public Task ClearJSONFileAsync(string filePath) => WriteAsync(filePath, string.Empty, encrypted: false);

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    ///
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content list of type <typeparamref name="T"/>. </returns>
    public async Task<T> GetCryptedContentAsync<T>(string filePath)
    {
        var jsonContent = await ReadAsync(filePath, encrypted: true);

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public Task AddCryptedContentAsync<T>(T content, string filePath, bool contentsHasId)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForAdd([content], jsonContentString, contentsHasId));

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public Task AddCryptedContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForAdd(contents, jsonContentString, contentsHasId));

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public Task UpdateCryptedContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForUpdate([content], jsonContentString, mappingProperty));

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public Task UpdateCryptedContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForUpdate(contents, jsonContentString, mappingProperty));

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public Task DeleteCryptedContentAsync<T>(dynamic mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty));

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public Task DeleteCryptedContentAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
        => ModifyAsync(filePath, encrypted: true, jsonContentString => GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty));

    /// <summary>
    /// Removes all file data and writes new one.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
    /// <exception cref="CryptographicException"> Throwns when key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>   
    /// <returns> Completed <see cref="Task"/> </returns>
    public Task ReplaceCryptedOldContentWithNewAsync<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        return WriteAsync(filePath, newJsonResult, encrypted: true);
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
        var jsonContent = Read(filePath, encrypted: false);

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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForAdd([content], jsonContentString, contentsHasId));
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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForAdd(contents, jsonContentString, contentsHasId));
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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForUpdate([content], jsonContentString, mappingProperty));
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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForUpdate(contents, jsonContentString, mappingProperty));
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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForDelete(mappingValue, jsonContentString, mappingProperty));
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
        Modify(filePath, encrypted: false, jsonContentString => GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty));
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

        Write(filePath, newJsonResult, encrypted: false);
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
    public void ClearJSONFile(string filePath) => Write(filePath, string.Empty, encrypted: false);

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    ///
    /// <typeparam name="T"> Return type. </typeparam>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <returns> A content list of type <typeparamref name="T"/>. </returns>
    public T GetCryptedContent<T>(string filePath)
    {
        var jsonContent = Read(filePath, encrypted: true);

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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public void AddCryptedContent<T>(T content, string filePath, bool contentsHasId)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForAdd([content], jsonContentString, contentsHasId));
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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be added. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param> 
    /// <returns></returns>
    public void AddCryptedContents<T>(List<T> contents, string filePath, bool contentsHasId)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForAdd(contents, jsonContentString, contentsHasId));
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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateCryptedContent<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForUpdate([content], jsonContentString, mappingProperty));
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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="contents"> Contents to be updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns></returns>
    public void UpdateCryptedContents<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForUpdate(contents, jsonContentString, mappingProperty));
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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValue"> Mapping value of content to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public void DeleteCryptedContent<T>(dynamic mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForDelete([mappingValue], jsonContentString, mappingProperty));
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
    /// <exception cref="CryptographicException"> Throwns when encryption key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="mappingValues"> Mapping values of contents to be deleted. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>
    /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
    /// <returns> Completed <see cref="Task"/> </returns>
    public void DeleteCryptedContent<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty)
    {
        Modify(filePath, encrypted: true, jsonContentString => GetJsonResultForDelete(mappingValues, jsonContentString, mappingProperty));
    }

    /// <summary>
    /// Removes all file data and writes new one.
    /// ! Milvasoft Corporation is not responsible of possible data loss.
    /// </summary>
    /// 
    /// <para> This method support all structures in class remarks. </para> 
    /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
    /// <exception cref="CryptographicException"> Throwns when key is incorrect or the file is corrupted. </exception>
    /// 
    /// <typeparam name="T"> Model type in json. </typeparam>
    /// <param name="content"> Content to be added or updated. </param>
    /// <param name="filePath"> Path to json file to get data from. </param>   
    /// <returns> Completed <see cref="Task"/> </returns>
    public void ReplaceCryptedOldContentWithNew<T>(T content, string filePath)
    {
        string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, _jsonSerializerSettings);

        Write(filePath, newJsonResult, encrypted: true);
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

        if (lastContent?.GetType().GetProperty(mappingPropertyName) == null)
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

        if (lastContent?.GetType().GetProperty(mappingPropertyName) == null)
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


    /// <summary>
    /// One lock per file, shared by all <see cref="JsonOperations"/> instances, so read-modify-write operations on the same file do not lose updates.
    /// </summary>
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    private static SemaphoreSlim GetFileLock(string fullPath) => _fileLocks.GetOrAdd(Path.GetFullPath(fullPath), _ => new SemaphoreSlim(1, 1));

    private async Task<string> ReadAsync(string filePath, bool encrypted)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        await fileLock.WaitAsync();

        try
        {
            return await ReadUnlockedAsync(fullPath, encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private string Read(string filePath, bool encrypted)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        fileLock.Wait();

        try
        {
            return ReadUnlocked(fullPath, encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task WriteAsync(string filePath, string content, bool encrypted)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        await fileLock.WaitAsync();

        try
        {
            await WriteUnlockedAsync(fullPath, content, encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private void Write(string filePath, string content, bool encrypted)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        fileLock.Wait();

        try
        {
            WriteUnlocked(fullPath, content, encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task ModifyAsync(string filePath, bool encrypted, Func<string, string> modify)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        await fileLock.WaitAsync();

        try
        {
            var content = await ReadUnlockedAsync(fullPath, encrypted);

            await WriteUnlockedAsync(fullPath, modify(content), encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private void Modify(string filePath, bool encrypted, Func<string, string> modify)
    {
        var fullPath = GetFilePath(filePath);
        var fileLock = GetFileLock(fullPath);

        fileLock.Wait();

        try
        {
            var content = ReadUnlocked(fullPath, encrypted);

            WriteUnlocked(fullPath, modify(content), encrypted);
        }
        finally
        {
            fileLock.Release();
        }
    }

    private async Task<string> ReadUnlockedAsync(string fullPath, bool encrypted)
    {
        if (!encrypted)
            return await File.ReadAllTextAsync(fullPath, _encoding);

        return Decrypt(await File.ReadAllBytesAsync(fullPath));
    }

    private string ReadUnlocked(string fullPath, bool encrypted)
    {
        if (!encrypted)
            return File.ReadAllText(fullPath, _encoding);

        return Decrypt(File.ReadAllBytes(fullPath));
    }

    private async Task WriteUnlockedAsync(string fullPath, string content, bool encrypted)
    {
        var tempPath = GetTempPath(fullPath);

        try
        {
            if (encrypted)
                await File.WriteAllTextAsync(tempPath, await GetCryptographyProvider().EncryptAsync(content), _utf8WithoutBom);
            else
                await File.WriteAllTextAsync(tempPath, content, GetWriteEncoding());

            File.Move(tempPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private void WriteUnlocked(string fullPath, string content, bool encrypted)
    {
        var tempPath = GetTempPath(fullPath);

        try
        {
            if (encrypted)
                File.WriteAllText(tempPath, GetCryptographyProvider().Encrypt(content), _utf8WithoutBom);
            else
                File.WriteAllText(tempPath, content, GetWriteEncoding());

            File.Move(tempPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Writes go to a temporary file in the same folder that replaces the target only when complete, so a crash never leaves a torn file.
    /// </summary>
    private static string GetTempPath(string fullPath) => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(fullPath)), $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");

    /// <summary>
    /// Keeps the previous output format: UTF-8 without byte order mark.
    /// </summary>
    private Encoding GetWriteEncoding() => _encoding is UTF8Encoding ? _utf8WithoutBom : _encoding;

    private static readonly UTF8Encoding _utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    private MilvaCryptographyProvider GetCryptographyProvider()
    {
        if (string.IsNullOrEmpty(_encryptionKey))
            throw new MilvaDeveloperException("Please provide an encryption key for encrypted json operations.");

        return _cryptographyProvider ??= new MilvaCryptographyProvider(new MilvaCryptographyOptions { Key = _encryptionKey }, _recyclableMemoryStreamManager);
    }

    /// <summary>
    /// Decrypts an encrypted json file.
    /// </summary>
    /// <remarks>
    /// Files are written by <see cref="MilvaCryptographyProvider"/> (UTF-8 text). Files written by the synchronous methods of
    /// previous versions (raw AES-CBC bytes with IV = key, ASCII plaintext) are still readable; the next write stores them in the
    /// current format.
    /// </remarks>
    private string Decrypt(byte[] fileContent)
    {
        Exception providerException;

        try
        {
            return GetCryptographyProvider().Decrypt(_utf8WithoutBom.GetString(fileContent).Trim());
        }
        catch (Exception ex) when (ex is not MilvaDeveloperException)
        {
            providerException = ex;
        }

        if (TryDecryptLegacySyncFormat(fileContent, _encryptionKey, out var legacyContent))
            return legacyContent;

        // Never put the key into the exception.
        throw new CryptographicException("The file could not be decrypted. The encryption key is incorrect or the file is corrupted.", providerException);
    }

    /// <summary>
    /// Decrypts the format written by the synchronous encryption methods of previous versions: raw AES-128-CBC/PKCS7 bytes
    /// with IV = key. Used only for reading, so existing files stay readable.
    /// </summary>
    internal static bool TryDecryptLegacySyncFormat(byte[] cipherText, string key, out string plainText)
    {
        plainText = null;

        var keyBytes = Encoding.UTF8.GetBytes(key ?? string.Empty);

        if (keyBytes.Length != 16 || cipherText.Length == 0 || cipherText.Length % 16 != 0)
            return false;

        try
        {
            using var aes = Aes.Create();

            aes.Key = keyBytes;

            var plainBytes = aes.DecryptCbc(cipherText, keyBytes, PaddingMode.PKCS7);

            plainText = Encoding.UTF8.GetString(plainBytes);

            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private string GetFilePath(string filePath) => !string.IsNullOrWhiteSpace(_basePath) ? Path.Combine(_basePath, filePath) : filePath;

    #endregion
}
