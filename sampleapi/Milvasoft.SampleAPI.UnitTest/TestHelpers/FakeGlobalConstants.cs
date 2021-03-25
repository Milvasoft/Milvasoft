using System;

namespace Milvasoft.SampleAPI.UnitTest.TestHelpers
{
    /// <summary>
    /// Fake global constant for test project.
    /// </summary>
    public static class FakeGlobalConstants
    {
        /// <summary>
        /// <para><b>EN: </b>Rootpath of application. </para>
        /// <para><b>TR: </b>Uygulamanın kök yolu.</para>
        /// </summary>
        public static string RootPath { get; } = Environment.CurrentDirectory;

        public static string MilvaKey { get; } = "w!z%C*F-JaNdRgUk";
    }
}
