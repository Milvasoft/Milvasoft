using Mapster;
using MilvasoftHelper.MapperImplementation.Implementation;

namespace MilvasoftHelper.MapperImplementation.MilvasoftMapster
{
    //MTL bu sınıf hiç bir yerde kullanılmıyorki neden yazılmış anlamış değilim

    /// <summary>
    /// <para><b>EN: </b>The mapping operation made to the object we want to access from the source object, written for use in test classes</para>
    /// <para><b>TR: </b>Test sınıflarında kullanılmak üzere yazılmış Kaynak nesneden ulaşmak istediğimiz nesneye yapılan maplemi işlemi</para>
    /// </summary>
    public class TestMapper : MapsterMapper.Mapper, IMapper
    {
        public TestMapper(TypeAdapterConfig typeAdapterConfig) : base(typeAdapterConfig.Default.PreserveReference(true).MaxDepth(2).IgnoreNullValues(true).Config)
        {

        }
        public TestMapper()
        { }

        /// <summary>
        /// <para><b>EN: </b>Responsible for mapping from the source object to the object we want to access</para>
        /// <para><b>TR: </b>Kaynak nesneden ulaşmak istediğimiz nesneye mapleme işlemini yapmakta sorumlu</para>
        /// </summary>
        public TDestination Map<TDestination>(object source, int maxDepth = 2)
        {
            TypeAdapterConfig<object, TDestination>.NewConfig()
                        .IgnoreNullValues(true)
                        .PreserveReference(true)
                        .MaxDepth(maxDepth);
            return source.Adapt<TDestination>();
        }
    }
}
