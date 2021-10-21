using Milvasoft.Helpers.Identity.Abstract;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.Utils
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class TokenManagement : ITokenManagement
    {
        [OValidateString(1000)]
        public string Secret { get; set; }

        [OValidateString(1000)]
        public string Issuer { get; set; }

        [OValidateString(1000)]
        public string Audience { get; set; }

        [OValidateString(1000)]
        public string LoginProvider { get; set; }

        [OValidateString(1000)]
        public string TokenName { get; set; }

        [OValidateDecimal(100)]
        public int AccessExpiration { get; set; }

        [OValidateDecimal(100)]
        public int RefreshExpiration { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
