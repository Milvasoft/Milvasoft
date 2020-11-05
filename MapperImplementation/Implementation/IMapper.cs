namespace Milvasoft.MapperImplementation.Implementation
{
    /// <summary>
    /// <para><b>EN: </b>Responsible interface for mapping from the source object to the object we want to access</para>
    /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu inteface</para>
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        /// <para><b>EN: </b>TDestination: The object we want to reach for us, TSource: The object that we will display as the source. For example: I want to map an entity object to a dto object, in this case "TDestination" should be a "dto" object, and "TSource" should be an "entity" object.</para>
        /// <para><b>TR: </b>TDestination : Bizim için ulaşmak istediğimiz nesne, TSource : Kaynak olarak göstereceğimiz nesne.Örn : Bir entity nesnesini dto nesnesine maplemek istiyorum, bu durumda “TDestination” bir “dto” nesnesi, “TSource” ise bir “entity” nesnesi olmalıdır.</para>
        /// </summary>
        TDestination Map<TDestination>(object source, int maxDepth = 2);

        /// <summary>
        /// <para><b>EN: </b>Responsible for mapping from the source object to the object we want to access</para>
        /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu metot</para>
        /// </summary>
        TDestination Map<TSource, TDestination>(TSource source);

        TDestination Map<TDestination>(object source);
    }
}
