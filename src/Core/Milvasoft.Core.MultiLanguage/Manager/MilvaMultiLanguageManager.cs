namespace Milvasoft.Core.MultiLanguage.Manager;

public class MilvaMultiLanguageManager(IServiceProvider serviceProvider) : MultiLanguageManager(serviceProvider)
{
    static MilvaMultiLanguageManager()
    {
        Languages.Clear();
        Languages = [.. LanguagesSeed.Seed];
    }
}

