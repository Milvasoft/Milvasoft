﻿namespace Milvasoft.DataAccess.MongoDB.Utils;

/// <summary>
/// Interface of mongo connection settings.
/// </summary>
public interface IMongoDbSettings
{
    /// <summary>
    /// Database name of to be connected database.
    /// </summary>
    string DatabaseName { get; set; }

    /// <summary>
    /// Connection string of to be connected database.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Uses DateTime.UtcNow if its true.
    /// </summary>
    bool UseUtcForDateTimes { get; set; }
}