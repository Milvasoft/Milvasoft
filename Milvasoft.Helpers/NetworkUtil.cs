using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Milvasoft.Helpers;

/// <summary>
/// Provides access network statistics.
/// </summary>
public static class NetworkUtil
{
    #region Async

    /// <summary>
    /// Gets all the IP addresses of the server machine hosting the application.
    /// </summary>
    /// <returns>a string array containing all the IP addresses of the server machine</returns>
    public static async Task<IPAddress[]> GetIPAddressesAsync() => (await Dns.GetHostEntryAsync(Dns.GetHostName()).ConfigureAwait(false)).AddressList;

    /// <summary>
    /// Gets the IP address of the server machine hosting the application.
    /// </summary>
    /// <param name="num">if set, it will return the Nth available IP address: if not set, the first available one will be returned.</param>
    /// <returns>the (first available or chosen) IP address of the server machine</returns>
    public static async Task<IPAddress> GetIPAddressAsync(int num = 0) => (await GetIPAddressesAsync().ConfigureAwait(false))[num];

    /// <summary>
    /// Checks if the given IP address is one of the IP addresses registered to the server machine hosting the application.
    /// </summary>
    /// <param name="ipAddress">the IP Address to check</param>
    /// <returns>TRUE if the IP address is registered, FALSE otherwise</returns>
    public static async Task<bool> HasIPAddressAsync(IPAddress ipAddress) => (await GetIPAddressesAsync().ConfigureAwait(false)).Contains(ipAddress);

    /// <summary>
    /// Checks if the given IP address is one of the IP addresses registered to the server machine hosting the application.
    /// </summary>
    /// <param name="ipAddress">the IP Address to check</param>
    /// <returns>TRUE if the IP address is registered, FALSE otherwise</returns>
    public static async Task<bool> HasIPAddressAsync(string ipAddress) => await HasIPAddressAsync(IPAddress.Parse(ipAddress)).ConfigureAwait(false);

    #endregion

    #region Sync

    /// <summary>
    /// Gets all the IP addresses of the server machine hosting the application.
    /// </summary>
    /// <returns>a string array containing all the IP addresses of the server machine</returns>
    public static IPAddress[] GetIPAddresses() => Dns.GetHostEntry(Dns.GetHostName()).AddressList;

    /// <summary>
    /// Gets the IP address of the server machine hosting the application.
    /// </summary>
    /// <param name="num">if set, it will return the Nth available IP address: if not set, the first available one will be returned.</param>
    /// <returns>the (first available or chosen) IP address of the server machine</returns>
    public static IPAddress GetIPAddress(int num = 0) => GetIPAddresses()[num];

    /// <summary>
    /// Checks if the given IP address is one of the IP addresses registered to the server machine hosting the application.
    /// </summary>
    /// <param name="ipAddress">the IP Address to check</param>
    /// <returns>TRUE if the IP address is registered, FALSE otherwise</returns>
    public static bool HasIPAddress(IPAddress ipAddress) => GetIPAddresses().Contains(ipAddress);

    /// <summary>
    /// Checks if the given IP address is one of the IP addresses registered to the server machine hosting the application.
    /// </summary>
    /// <param name="ipAddress">the IP Address to check</param>
    /// <returns>TRUE if the IP address is registered, FALSE otherwise</returns>
    public static bool HasIPAddress(string ipAddress) => HasIPAddress(IPAddress.Parse(ipAddress));

    #endregion
}
