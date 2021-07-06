using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Milvasoft.Helpers.Exceptions;
using Xunit;

namespace Milvasoft.Helpers.Test.Integration
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
        public static void CheckResponseForSecurity(ResponseObject testExpectected, ResponseObject objectResponse, bool isAccepted, IStringLocalizer stringLocalizer)
        {
            if (isAccepted)
            {
                NotEqual(objectResponse.StatusCode, StatusCodes.Status403Forbidden);
                NotEqual(objectResponse.StatusCode, StatusCodes.Status401Unauthorized);
            }
            else
            {
                CheckMessage(testExpectected.Message, objectResponse.Message, stringLocalizer);
                Equal(testExpectected.StatusCode, objectResponse.StatusCode);
                Equal(testExpectected.Successful, objectResponse.Successful);
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
