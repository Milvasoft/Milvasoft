using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Exceptions;
using Milvasoft.Helpers.Models.Response;
using Milvasoft.Helpers.Utils;
using Xunit;

namespace Milvasoft.Helpers.MilvaTest.Integration
{
    /// <summary>
    /// Includes custom control methods for the Assert class.
    /// </summary>
    public partial class MilvaAssert : Assert
    {
        /// <summary>
        /// Checks the result of safety tests.
        /// </summary>
        /// <param name="testExpectected"></param>
        /// <param name="objectResponse"></param>
        /// <param name="isAccepted"></param>
        /// <param name="stringLocalizer"></param>
        public static void CheckResponseForSecurity(TestExpectected testExpectected, ObjectResponse<object> objectResponse, bool isAccepted, IStringLocalizer stringLocalizer)
        {
            if (isAccepted)
            {
                NotEqual(objectResponse.StatusCode, MilvaStatusCodes.Status403Forbidden);
                NotEqual(objectResponse.StatusCode, MilvaStatusCodes.Status401Unauthorized);
            }
            else
            {
                CheckMessage(testExpectected.MessageKey, objectResponse.Message, stringLocalizer);
                Equal(testExpectected.StatusCode, objectResponse.StatusCode);
                Equal(testExpectected.Successful, objectResponse.Success);
            }
        }

        /// <summary>
        /// Checks messages returned from test results.
        /// </summary>
        /// <param name="messageKey"></param>
        /// <param name="responseMessage"></param>
        /// <param name="stringLocalizer"></param>
        public static void CheckMessage(string messageKey, string responseMessage, IStringLocalizer stringLocalizer)
        {
            var expectedMessage = stringLocalizer[messageKey];

            if (expectedMessage == messageKey)
                throw new MilvaTestException("Message not found.");

            Equal(expectedMessage, responseMessage);
        }
    }
}
