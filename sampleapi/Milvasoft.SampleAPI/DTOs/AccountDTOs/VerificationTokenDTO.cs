using Milvasoft.SampleAPI.Utils.Attributes.ValidationAttributes;

namespace Milvasoft.SampleAPI.DTOs.AccountDTOs
{
    //TODO OGZHAN YORUM SATILARI EKLENECEK.
    public record VerificationTokenDTO
    {
        [OValidateString(2, 100)]
        public string UserName { get; set; }

        [OValidateString(20, 1000, MemberNameLocalizerKey = "InvalidVerificationToken")]
        public string TokenString { get; init; }
    }
}
