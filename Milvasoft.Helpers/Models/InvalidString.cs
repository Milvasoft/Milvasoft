using System.Collections.Generic;

namespace Milvasoft.Helpers.Models
{
    /// <summary>
    /// <para><b>EN: </b> Invalid strings for prevent hacking or someting ;)  </para>
    /// <para><b>TR: </b> Hacking veya başka bir şeyi önlemek için geçersiz string değerler ;) </para>
    /// </summary>
    public class InvalidString
    {
        /// <summary>
        ///  <para><b>EN: </b> Name of invalid string.  </para>
        ///  <para><b>TR: </b> Geçersiz string'in adı. </para>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// <para><b>EN: </b> Invalid values.  </para>
        /// <para><b>TR: </b> Geçersiz değerler. </para>
        /// </summary>
        public List<string> Values { get; set; }
    }
}
