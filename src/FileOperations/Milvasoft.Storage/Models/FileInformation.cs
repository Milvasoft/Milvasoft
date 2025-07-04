﻿using Milvasoft.Core.EntityBases.Concrete;

namespace Milvasoft.Storage.Models;

/// <summary>
/// File information model.
/// </summary>
public class FileInformation : EntityBase<string>
{
    /// <summary>
    /// File url.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// File path.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// File name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Version of file.
    /// </summary>
    public int FileVersion { get; set; }

    /// <summary>
    /// Order of display of file. Lowest comes first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Alt text of file.
    /// </summary>
    public string AltText { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInformation"/> class with a new unique identifier.
    /// </summary>
    public FileInformation()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInformation"/> class with a new unique identifier. 
    /// </summary>
    public FileInformation(string id = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            Id = Guid.NewGuid().ToString();
        else
            Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileInformation"/> class.
    /// </summary>
    /// <param name="fileUploadRequest"></param>
    public FileInformation(FileUploadRequest fileUploadRequest) : this(fileUploadRequest.Id)
    {
        if (string.IsNullOrWhiteSpace(fileUploadRequest.Id))
            Id = fileUploadRequest.Id;

        AltText = fileUploadRequest.AltText;
        FileName = fileUploadRequest.FileName;
        Order = fileUploadRequest.Order;
        FileVersion = 0;
    }
}
