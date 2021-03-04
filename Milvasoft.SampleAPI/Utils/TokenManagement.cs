﻿using Milvasoft.Helpers.Identity.Concrete;
using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.Utils
{
    public class TokenManagement : ITokenManagement
    {
        [OValidateString(1000)]
        public string Secret { get; set; }

        [OValidateString(1000)]
        public string Issuer { get; set; }

        [OValidateString(1000)]
        public string Audience { get; set; }

        public int AccessExpiration { get; set; }

        public int RefreshExpiration { get; set; }
    }
}
