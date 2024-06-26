﻿using Castle.DynamicProxy;
using System.Reflection;

namespace Milvasoft.Interception.Decorator;

/// <summary>
/// Generic helper extension methods.
/// </summary>
public static class GenericExtensions
{
    /// <summary>
    /// Gets the underlying not decorated object from a decorated object.
    /// </summary>
    /// <returns>Underlying object or same object if it was not decorated.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "<Pending>")]
    public static T UnwrapDecorated<T>(this T instance) where T : class
        => ProxyUtil.IsProxy(instance)
            ? (T)instance.GetType()
                .GetField("__target", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(instance)
            : instance;
}
