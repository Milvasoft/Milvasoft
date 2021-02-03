using Milvasoft.Helpers.FileOperations.Abstract;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Milvasoft.Helpers.FileOperations.Concrete
{
    /// <summary>
    /// Provides add-edit-delete-get process json files that hold one list.
    /// 
    /// <para> Example json file template for use this library methods; </para>
    /// <code>
    /// 
    /// 
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
    /// </code>
    /// </summary>
    public class JsonOperations : IJsonOperations
    {
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
        public async Task<List<T>> GetRequiredContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        public async Task<List<T>> GetContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            var contentList = JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (IsNullOrEmpty(contentList)) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        public async Task AddContentToJsonFileAsync<T>(List<T> contents, string filePath, bool contentsHasId, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? new List<T>() : tempJsonContent;

            if (contentsHasId == true)
            {
                int? lastId;

                if (jsonContent.Count == 0) lastId = 0;
                else GetLastIdIfHasId(out lastId, jsonContent);

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

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, newJsonResult).ConfigureAwait(false);
        }

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
        public async Task UpdateContentFromJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Update") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var upToDateContents = new List<T>();
            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType().GetProperty(mappingPropertyName).GetValue(c)
                                                                .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content)));

                if (matchedContent != null)
                {
                    int index = jsonContent.IndexOf(matchedContent);
                    if (index != -1)
                        jsonContent[index] = content;

                    upToDateContents.Add(matchedContent);
                }
            }

            if (upToDateContents.Count == 0) throw new Exception("Requested content for update not found!!!");

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, newJsonResult).ConfigureAwait(false);
        }

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
        public async Task DeleteContentFromJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Delete") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var willRemovedContents = new List<T>();

            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => (c.GetType().GetProperty(mappingPropertyName).GetValue(c, null))
                                                                 .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content, null)));

                if (matchedContent != null)
                    willRemovedContents.Add(matchedContent);
            }

            if (willRemovedContents.Count == 0) throw new Exception("Requested content for delete not found!!!");

            foreach (var willRemovedContent in willRemovedContents)
                jsonContent.Remove(willRemovedContent);

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await File.WriteAllTextAsync(filePath, newJsonResult).ConfigureAwait(false);
        }

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
        public async Task<T> GetRequiredSingleContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        public async Task<T> GetSingleContentFromJsonFileAsync<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath, Encoding.UTF8).ConfigureAwait(false);

            var contentList = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (contentList != null) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        public async Task AddOrUpdateSingleContentToJsonFileAsync<T>(T content, string filePath, CultureInfo cultureInfo = null)
        {
            string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            await File.WriteAllTextAsync(filePath, newJsonResult).ConfigureAwait(false);
        }

        /// <summary>
        /// Clears json file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> Path to json file to get data from. </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        public async Task ClearJSONFileAsync(string filePath) => await File.WriteAllTextAsync(filePath, "").ConfigureAwait(false);

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        public async Task<List<T>> GetRequiredContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        public async Task<List<T>> GetContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            var contentList = JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (IsNullOrEmpty(contentList)) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        public async Task AddContentToCryptedJsonFileAsync<T>(List<T> contents, string filePath, bool contentsHasId, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? new List<T>() : tempJsonContent;

            if (contentsHasId == true)
            {
                int? lastId;

                if (jsonContent.Count == 0) lastId = 0;
                else GetLastIdIfHasId(out lastId, jsonContent);

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

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await EncryptAndWriteAsync(filePath, newJsonResult, key).ConfigureAwait(false);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>        
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        /// <returns> Completed <see cref="Task"/> </returns>
        public async Task UpdateContentFromCryptedJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Update") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var upToDateContents = new List<T>();
            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType()
                                                            .GetProperty(mappingPropertyName)
                                                            .GetValue(c)
                                                            .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content)));

                if (matchedContent != null)
                {
                    int index = jsonContent.IndexOf(matchedContent);
                    if (index != -1)
                        jsonContent[index] = content;

                    upToDateContents.Add(matchedContent);
                }
            }

            if (upToDateContents.Count == 0) throw new Exception("Requested content for update not found!!!");

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await EncryptAndWriteAsync(filePath, newJsonResult, key).ConfigureAwait(false);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        /// <returns> Completed <see cref="Task"/> </returns>
        public async Task DeleteContentFromCryptedJsonFileAsync<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Delete") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var willRemovedContents = new List<T>();

            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType()
                                                            .GetProperty(mappingPropertyName)
                                                            .GetValue(c, null)
                                                            .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content, null)));

                if (matchedContent != null)
                    willRemovedContents.Add(matchedContent);
            }

            if (willRemovedContents.Count == 0) throw new Exception("Requested content for delete not found!!!");

            foreach (var willRemovedContent in willRemovedContents)
                jsonContent.Remove(willRemovedContent);

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            await EncryptAndWriteAsync(filePath, newJsonResult, key).ConfigureAwait(false);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        public async Task<T> GetRequiredSingleContentCryptedFromJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>        
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null.  </returns>
        public async Task<T> GetSingleContentFromCryptedJsonFileAsync<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = await DecryptAndReadAsync(filePath, key).ConfigureAwait(false);

            var contentList = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (contentList != null) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR  </param>       
        /// <returns> Completed <see cref="Task"/> </returns>
        public async Task AddOrUpdateSingleContentToCryptedJsonFileAsync<T>(T content, string filePath, string key, CultureInfo cultureInfo = null)
        {
            string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            await EncryptAndWriteAsync(filePath, newJsonResult, key).ConfigureAwait(false);
        }

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
        public List<T> GetRequiredContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            var contentList = JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (IsNullOrEmpty(contentList)) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        public List<T> GetContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            var contentList = JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            return contentList;
        }

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
        public void AddContentToJsonFile<T>(List<T> contents, string filePath, bool contentsHasId, CultureInfo cultureInfo = null)
        {
            var jsonContentString = File.ReadAllText(filePath, Encoding.UTF8);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(tempJsonContent) ? new List<T>() : tempJsonContent;

            if (contentsHasId == true)
            {
                int? lastId;

                if (jsonContent.Count == 0) lastId = 0;
                else GetLastIdIfHasId(out lastId, jsonContent);

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


            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            File.WriteAllText(filePath, newJsonResult);
        }

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
        public void UpdateContentFromJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null)
        {
            var jsonContentString = File.ReadAllText(filePath, Encoding.UTF8);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(tempJsonContent) ? throw new Exception("No Records for Update") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var upToDateContents = new List<T>();
            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType().GetProperty(mappingPropertyName).GetValue(c)
                                                                .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content)));

                if (matchedContent != null)
                {
                    int index = jsonContent.IndexOf(matchedContent);
                    if (index != -1)
                        jsonContent[index] = content;

                    upToDateContents.Add(matchedContent);
                }

            }

            if (upToDateContents.Count == 0) throw new Exception("Requested content for update not found!!!");

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            File.WriteAllText(filePath, newJsonResult);
        }

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
        public void DeleteContentFromJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, CultureInfo cultureInfo = null)
        {
            var jsonContentString = File.ReadAllText(filePath, Encoding.UTF8);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(tempJsonContent) ? throw new Exception("No Records for Delete") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);


            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var willRemovedContents = new List<T>();

            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => (c.GetType().GetProperty(mappingPropertyName).GetValue(c, null))
                                                                 .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content, null)));

                if (matchedContent != null)
                    willRemovedContents.Add(matchedContent);

            }

            if (willRemovedContents.Count == 0) throw new Exception("Requested content for delete not found!!!");

            foreach (var willRemovedContent in willRemovedContents)
            {
                jsonContent.Remove(willRemovedContent);
            }


            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            File.WriteAllText(filePath, newJsonResult);

        }

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
        public T GetRequiredSingleContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            var content = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            return content;
        }

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
        public T GetSingleContentFromJsonFile<T>(string filePath, CultureInfo cultureInfo = null)
        {
            var jsonContent = File.ReadAllText(filePath, Encoding.UTF8);

            var contentList = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (contentList != null)
                return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        public void AddOrUpdateSingleContentToJsonFile<T>(T content, string filePath, CultureInfo cultureInfo = null)
        {
            string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, new JsonSerializerSettings()
            { Culture = cultureInfo ?? new CultureInfo("en-US") });

            File.WriteAllText(filePath, newJsonResult);
        }

        /// <summary>
        /// Clears json file in <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> Path to json file to get data from. </param>
        public void ClearJSONFile(string filePath) => File.WriteAllText(filePath, "");

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        public List<T> GetRequiredContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = DecryptAndRead(filePath, key);

            return JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A content list of type <typeparamref name="T"/>. </returns>
        public List<T> GetContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = DecryptAndRead(filePath, key);

            var contentList = JsonConvert.DeserializeObject<List<T>>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (IsNullOrEmpty(contentList)) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> Completed <see cref="Task"/> </returns>
        public void AddContentToCryptedJsonFile<T>(List<T> contents, string filePath, bool contentsHasId, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = DecryptAndRead(filePath, key);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? new List<T>() : tempJsonContent;

            if (contentsHasId == true)
            {
                int? lastId;

                if (jsonContent.Count == 0) lastId = 0;
                else GetLastIdIfHasId(out lastId, jsonContent);

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

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            EncryptAndWrite(filePath, newJsonResult, key);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>        
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>  
        /// <returns> Completed <see cref="Task"/> </returns>
        public void UpdateContentFromCryptedJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, dynamic>> mappingProperty, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = DecryptAndRead(filePath, key);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Update") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var upToDateContents = new List<T>();
            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType().GetProperty(mappingPropertyName).GetValue(c)
                                                                .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content)));

                if (matchedContent != null)
                {
                    int index = jsonContent.IndexOf(matchedContent);
                    if (index != -1)
                        jsonContent[index] = content;

                    upToDateContents.Add(matchedContent);
                }
            }

            if (upToDateContents.Count == 0) throw new Exception("Requested content for update not found!!!");

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            EncryptAndWrite(filePath, newJsonResult, key);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param> 
        /// <returns> Completed <see cref="Task"/> </returns>
        public void DeleteContentFromCryptedJsonFile<T>(List<T> contents, string filePath, Expression<Func<T, object>> mappingProperty, string key, CultureInfo cultureInfo = null)
        {
            var jsonContentString = DecryptAndRead(filePath, key);

            var tempJsonContent = JsonConvert.DeserializeObject<List<T>>(jsonContentString, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            var jsonContent = IsNullOrEmpty(jsonContentString) ? throw new Exception("No Records for Delete") : tempJsonContent;

            var lastContent = jsonContent.Last();

            var mappingPropertyName = GetPropertyNameFromExpression(mappingProperty);

            if (lastContent.GetType().GetProperty(mappingPropertyName) == null) throw new Exception($"This content type not have {mappingPropertyName} property ");

            var willRemovedContents = new List<T>();

            foreach (var content in contents)
            {
                var matchedContent = jsonContent.Find(c => c.GetType()
                                                            .GetProperty(mappingPropertyName)
                                                            .GetValue(c, null)
                                                            .Equals(content.GetType().GetProperty(mappingPropertyName).GetValue(content, null)));

                if (matchedContent != null)
                    willRemovedContents.Add(matchedContent);
            }

            if (willRemovedContents.Count == 0) throw new Exception("Requested content for delete not found!!!");

            foreach (var willRemovedContent in willRemovedContents)
                jsonContent.Remove(willRemovedContent);

            string newJsonResult = JsonConvert.SerializeObject(jsonContent, Formatting.Indented);

            EncryptAndWrite(filePath, newJsonResult, key);
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>      
        /// <param name="cultureInfo"> Culture information. Default is "en-US". </param>
        /// <returns> A single content of type <typeparamref name="T"/>. </returns>
        public T GetRequiredSingleContentCryptedFromJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = DecryptAndRead(filePath, key);

            return JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>        
        /// <returns> A single content of type <typeparamref name="T"/> if content isn't null.  </returns>
        public T GetSingleContentFromCryptedJsonFile<T>(string filePath, string key, CultureInfo cultureInfo = null)
        {
            var jsonContent = DecryptAndRead(filePath, key);

            var contentList = JsonConvert.DeserializeObject<T>(jsonContent, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            if (contentList != null) return contentList;
            else throw new ArgumentNullException("No Records");
        }

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
        /// <param name="key"> Key of encrypted file. Example key: 4u7x!A%D*F-JaNdR </param>       
        /// <returns> Completed <see cref="Task"/> </returns>
        public void AddOrUpdateSingleContentToCryptedJsonFile<T>(T content, string filePath, string key, CultureInfo cultureInfo = null)
        {
            string newJsonResult = JsonConvert.SerializeObject(content, Formatting.Indented, new JsonSerializerSettings() { Culture = cultureInfo ?? new CultureInfo("en-US") });

            EncryptAndWrite(filePath, newJsonResult, key);
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
        public void GetLastIdIfHasId<T>(out int? lastId, List<T> contentList) => lastId = !IsNullOrEmpty(contentList)
                                                                                          ? (int?)contentList.Max(i => i.GetType().GetProperty("Id").GetValue(i, null))
                                                                                          : null;

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Determines the right casting and pets property name from expression with this right casting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        private string GetPropertyNameFromExpression<T>(Expression<Func<T, object>> expression)
        {
            if (expression.Body is MemberExpression returnExpression)
            {
                return returnExpression.Member.Name;
            }
            else
            {
                var op = ((UnaryExpression)expression.Body).Operand;
                return ((MemberExpression)op).Member.Name;
            }
        }

        /// <summary>
        /// <para><b>EN: </b>Checks whether or not collection is null or empty. Assumes collection can be safely enumerated multiple times.</para>
        /// <para><b>TR: </b>Koleksiyonun boş veya boş olup olmadığını denetler. Koleksiyonun birden çok kez güvenli bir şekilde numaralandırılabileceğini varsayar.</para>
        /// </summary>
        private static bool IsNullOrEmpty(IEnumerable @this) => @this == null || @this.GetEnumerator().MoveNext() == false;

        private async Task<string> DecryptAndReadAsync(string filePath, string key)
        {
            var plainContent = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            //byte[] plainContent = Encoding.ASCII.GetBytes(content);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

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

        private async Task EncryptAndWriteAsync(string filePath, string content, string key)
        {
            //var plainContent = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            byte[] plainContent = Encoding.ASCII.GetBytes(content);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);

            try
            {
                cryptoStream.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                throw;
            }

            await File.WriteAllBytesAsync(filePath, memStream.ToArray()).ConfigureAwait(false);
        }

        private string DecryptAndRead(string filePath, string key)
        {
            var plainContent = File.ReadAllBytes(filePath);
            //byte[] plainContent = Encoding.ASCII.GetBytes(content);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

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

        private void EncryptAndWrite(string filePath, string content, string key)
        {
            //var plainContent = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            byte[] plainContent = Encoding.ASCII.GetBytes(content);

            using var algorithm = new AesCryptoServiceProvider();

            var keyBytes = Encoding.UTF8.GetBytes(key);

            if (keyBytes.Length != 16) throw new ArgumentOutOfRangeException("Key is not proper length. Key bit length must be 16.");

            algorithm.IV = keyBytes;
            algorithm.Key = keyBytes;
            algorithm.Mode = CipherMode.CBC;
            algorithm.Padding = PaddingMode.PKCS7;

            using var memStream = new MemoryStream();

            using var cryptoStream = new CryptoStream(memStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainContent, 0, plainContent.Length);

            try
            {
                cryptoStream.FlushFinalBlock();
            }
            catch (CryptographicException)
            {
                throw;
            }

            File.WriteAllBytes(filePath, memStream.ToArray());
        }

        #endregion
    }
}
