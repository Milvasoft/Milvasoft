using Mapster;
using MilvasoftHelper.MapperImplementation.Implementation;

namespace MilvasoftHelper.MapperImplementation.MilvasoftMapster
{
    /// <summary>
    /// <para><b>EN: </b>Class written instead of manual mapping. It is written to map DTO class object to entity class.</para>
    /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu. DTO sınıfı nesnesini entity sınıfına maplemek için yazılmıştır</para>
    /// </summary>
    public class Mapper : IMapper
    {
        /// <summary>
        /// <para><b>EN: </b>TDestination: The object we want to reach for us, TSource: The object that we will display as the source. For example: I want to map an entity object to a dto object, in this case "TDestination" should be a "dto" object, and "TSource" should be an "entity" object.</para>
        /// <para><b>TR: </b>TDestination : Bizim için ulaşmak istediğimiz nesne, TSource : Kaynak olarak göstereceğimiz nesne.Örn : Bir entity nesnesini dto nesnesine maplemek istiyorum, bu durumda “TDestination” bir “dto” nesnesi, “TSource” ise bir “entity” nesnesi olmalıdır.</para>
        /// </summary>
        public TDestination Map<TSource, TDestination>(TSource source)
        {
            TypeAdapterConfig<TSource, TDestination>.NewConfig().MaxDepth(2);
            return source.Adapt<TDestination>();
        }

        //MTL MaxDepth metodu ne iş yapıyor en fazla 2 sınıfmı mapleme yapabilsin anlammına geliyor ve Alttaki metot ile bu metotun farkı ne ? 

        /// <summary>
        /// <para><b>EN: </b>Responsible for mapping from the source object to the object we want to access</para>
        /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu metot</para>
        /// </summary>
        public TDestination Map<TDestination>(object source, int maxDepth = 2)
        { 
            TypeAdapterConfig<object, TDestination>.NewConfig()
                      .IgnoreNullValues(true)
                      .PreserveReference(true)
                      .MaxDepth(maxDepth);
            return source.Adapt<TDestination>();

        }

        //MTL Burada hep 2 degerini alacak (MaxDepth metodu) yukarıdaki metotta parametre olarak alıyor ee ne gerek var bu metoda ? yukarıdaki metot ile aynı işi yapıyor bu metotun olması saçma geldi bana
        public TDestination Map<TDestination>(object source)
        {
            TypeAdapterConfig<object, TDestination>.NewConfig()
                      .IgnoreNullValues(true)
                      .PreserveReference(true)
                      .MaxDepth(2);
            return source.Adapt<TDestination>();
        }
    }
}