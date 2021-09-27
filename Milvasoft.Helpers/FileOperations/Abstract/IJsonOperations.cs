﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.FileOperations.Abstract
{

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
    public interface IJsonOperations
    {
        /// <summary>
        /// If you change culture info after object creation, use this method.
        /// </summary>
        /// <param name="cultureInfo"></param>
        void SetCultureInfo(CultureInfo cultureInfo);

        /// <summary>
        /// If you change encryption key after object creation, use this method.
        /// </summary>
        /// <param name="encryptionKey"></param>
        void SetEncryptionKey(string encryptionKey);

        /// <summary>
        /// If you change encoding after object creation, use this method.
        /// </summary>
        /// <param name="encoding"></param>
        void SetEncoding(Encoding encoding);

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
        Task<T> GetContentAsync<T>(string filePath);

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
        Task AddContentAsync<T>(T content, string filePath, bool contentsHasId = false);

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
        Task AddContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId = false);

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
        Task UpdateContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task UpdateContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task DeleteContentAsync<T>(List<dynamic> mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task DeleteContentsAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task ReplaceOldContentWithNewAsync<T>(T content, string filePath);

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
        Task ClearJSONFileAsync(string filePath);

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
        Task<T> GetCryptedContentAsync<T>(string filePath);

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
        Task AddCryptedContentAsync<T>(T content, string filePath, bool contentsHasId);

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
        Task AddCryptedContentsAsync<T>(List<T> contents, string filePath, bool contentsHasId);

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
        Task UpdateCryptedContentAsync<T>(T content, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task UpdateCryptedContentsAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task DeleteCryptedContentAsync<T>(dynamic mappingValue, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task DeleteCryptedContentAsync<T>(List<dynamic> mappingValues, string filePath, Expression<Func<T, dynamic>> mappingProperty);

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
        Task ReplaceCryptedOldContentWithNewAsync<T>(T content, string filePath);

        #endregion

        #endregion


        #region JSON File Operations Async

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        Task<List<T>> GetRequiredContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content list is null or empty. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        Task<List<T>> GetContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
        /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
        /// </summary>
        /// <remarks>
        /// 
        /// <para> If <paramref name="cultureInfo"/> is null, default is ("en-US")</para> 
        /// 
        /// <para><b> yourjsonfile.json file must contains list or must be empty.</b> </para> 
        /// 
        /// </remarks>
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be added. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        Task AddContentToJsonFileAsync<T>(List<T> contents, string filePath, bool contentsHasId, CultureInfo cultureInfo = null);

        /// <summary>
        /// Updates contents from json file in <paramref name="filePath"/>.
        /// </summary>
        /// <remarks>
        /// Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> !!!
        /// </remarks>
        /// <exception cref="Exception()">
        /// Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.
        /// </exception>
        /// <exception cref="Exception()">
        /// Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/> .
        /// </exception>
        /// <remarks>
        /// 
        /// Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        /// <returns> Completed <see cref="Task"/> </returns>
        Task UpdateContentFromJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null);

        /// <summary>
        /// Deletes record from requested json file.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        /// <returns> Completed <see cref="Task"/> </returns>
        Task DeleteContentFromJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        Task<T> GetRequiredSingleContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content is null. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null.  </returns>
        Task<T> GetSingleContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
        /// (like Auto Increment)
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// yourjsonfile.json file must contains exact same type of <typeparamref name="T"/> or file must be empty.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="content"> Content to be added or updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        Task AddOrUpdateSingleContentToJsonFileAsync<T>(T content, string filePath, CultureInfo cultureInfo = null);

        #region With Encryption

        /// <summary>
        /// Gets all content from crypted json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>

        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        Task<List<T>> GetRequiredContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content list is null or empty. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        Task<List<T>> GetContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <remarks>
        /// 
        /// <para> If <paramref name="cultureInfo"/> is null, default is ("en-US")</para> 
        /// 
        /// <para><b> yourjsonfile.json file must contains list or must be empty.</b> </para> 
        /// 
        /// </remarks>
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be added. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        Task AddContentToCryptedJsonFileAsync<T>(List<T> contents, string filePath, bool contentsHasId, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Updates contents from json file in <paramref name="filePath"/> with  <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <remarks>
        /// Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> !!!
        /// </remarks>
        /// <exception cref="Exception()">
        /// Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.
        /// </exception>
        /// <exception cref="Exception()">
        /// Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/> .
        /// </exception>
        /// <remarks>
        /// 
        /// Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="key"> Key of encrypted file. </param>        
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        /// <returns> Completed <see cref="Task"/> </returns>
        Task UpdateContentFromCryptedJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Deletes record from requested json file with <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        /// <returns> Completed <see cref="Task"/> </returns>
        Task DeleteContentFromCryptedJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        Task<T> GetRequiredSingleContentCryptedFromJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content is null. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <param name="key"> Key of encrypted file. </param>        
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null.  </returns>
        Task<T> GetSingleContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// (like Auto Increment)
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// yourjsonfile.json file must contains exact same type of <typeparamref name="T"/> or file must be empty.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="content"> Content to be added or updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <param name="key"> Key of encrypted file. </param>       
        /// <returns> Completed <see cref="Task"/> </returns>
        Task AddOrUpdateSingleContentToCryptedJsonFileAsync<T>(T content, string filePath, string key, CultureInfo cultureInfo = null);

        #endregion

        #endregion

        #region JSON File Operations Sync

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content list is null or empty. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        List<T> GetRequiredContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        List<T> GetContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/>.
        /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
        /// </summary>
        /// <remarks>
        /// 
        /// <para> If <paramref name="cultureInfo"/> is null, default is ("en-US")</para> 
        /// 
        /// <para><b> yourjsonfile.json file must contains list or must be empty.</b> </para> 
        /// 
        /// </remarks>
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be added. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns></returns>
        void AddContentToJsonFile<T>(List<T> contents, string filePath, bool contentsHasId, CultureInfo cultureInfo = null);

        /// <summary>
        /// Updates contents from json file in <paramref name="filePath"/>.
        /// </summary>
        /// <remarks>
        /// Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> !!!
        /// </remarks>
        /// <exception cref="Exception()">
        /// Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.
        /// </exception>
        /// <exception cref="Exception()">
        /// Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/> .
        /// </exception>
        /// <remarks>
        /// 
        /// Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        void UpdateContentFromJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null);

        /// <summary>
        /// Deletes record from requested json file.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        void DeleteContentFromJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        T GetRequiredSingleContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/>. 
        /// Returns them as the requested list of type.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content is null. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null. </returns>
        T GetSingleContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/>.
        /// (like Auto Increment)
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// yourjsonfile.json file must contains exact same type of <typeparamref name="T"/> or file must be empty.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="content"> Content to be added or updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        void AddOrUpdateSingleContentToJsonFile<T>(T content, string filePath, CultureInfo cultureInfo = null);

        /// <summary>
        /// Clears json file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> Path to json file to get data from. </param>
        void ClearJSONFile(string filePath);

        #region With Encryption

        /// <summary>
        /// Gets all content from crypted json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        List<T> GetRequiredContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets all content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content list is null or empty. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        List<T> GetContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="contents"/> to json file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// If content has "Id" property which type is "System.Int32" . Send <paramref name="contentsHasId"/> param "true". The code will be increase Id automatically.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <remarks>
        /// 
        /// <para> If <paramref name="cultureInfo"/> is null, default is ("en-US")</para> 
        /// 
        /// <para><b> yourjsonfile.json file must contains list or must be empty.</b> </para> 
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be added. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="contentsHasId"> Determines whether the contents are added in auto increment by "Id" property. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        void AddContentToCryptedJsonFile<T>(List<T> contents, string filePath, bool contentsHasId, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Updates contents from json file in <paramref name="filePath"/> with  <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// <remarks>
        /// Updates by requested Property. Don't forget send <paramref name="mappingProperty"/> !!!
        /// </remarks>
        /// <exception cref="Exception()">
        /// Throwns when not valid <paramref name="mappingProperty"/> or requested Entity type does not have that <paramref name="mappingProperty"/>.
        /// </exception>
        /// <exception cref="Exception()">
        /// Throwns when json file in <paramref name="filePath"/> not contains <paramref name="contents"/> .
        /// </exception>
        /// <remarks>
        /// 
        /// Send all the properties of the object to be updated. Otherwise, unsent properties are updated to null.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="key"> Key of encrypted file. </param>        
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        /// <returns> Completed <see cref="Task"/> </returns>
        void UpdateContentFromCryptedJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Deletes record from requested json file with <paramref name="key"/>.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="contents"> Contents to be updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="mappingProperty"> The data to be updated is extracted from the file according to this property. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        /// <returns> Completed <see cref="Task"/> </returns>
        void DeleteContentFromCryptedJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="key"> Key of encrypted file. </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        T GetRequiredSingleContentCryptedFromJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Gets single content from json file in <paramref name="filePath"/> with <paramref name="key"/>. 
        /// Returns them as the requested list of type.
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// <para> Example json file template for use this library methods; </para>
        /// <code>
        /// 
        ///  {
        ///    "Id": 1,
        ///    "Name": "poco1",
        ///    "Which": false
        ///  }
        ///  
        ///  OR
        ///  
        ///  "Hello World!"
        /// 
        /// </code>
        /// 
        /// </summary>
        /// 
        /// <remarks> 
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException"> Throwns when returned content is null. </exception>
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <param name="key"> Key of encrypted file. </param>        
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null.  </returns>
        T GetSingleContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null);

        /// <summary>
        /// Adds <paramref name="content"/> to json file in <paramref name="filePath"/> with <paramref name="key"/>.
        /// (like Auto Increment)
        /// !!! Milvasoft Corporation is not responsible of possible data loss.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// yourjsonfile.json file must contains exact same type of <typeparamref name="T"/> or file must be empty.
        /// 
        /// If <paramref name="cultureInfo"/> is null, default is ("en-US")
        /// 
        /// </remarks>
        /// 
        /// <exception cref="ArgumentOutOfRangeException"> Throwns when key is not proper lenght. </exception>
        /// <exception cref="ArgumentException"> Throwns when key is incorrect. </exception>
        /// 
        /// <typeparam name="T"> Return type. </typeparam>
        /// <param name="content"> Content to be added or updated. </param>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <param name="key"> Key of encrypted file. </param>       
        /// <returns> Completed <see cref="Task"/> </returns>
        void AddOrUpdateSingleContentToCryptedJsonFile<T>(T content, string filePath, string key, CultureInfo cultureInfo = null);

        #endregion

        #endregion

        #region  Helper Methods

        /// <summary>
        /// Gets last id from <paramref name="contentList"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lastId"></param>
        /// <param name="contentList"></param>
        void GetLastIdIfHasId<T>(out int? lastId, List<T> contentList);

        #endregion
    }
}
