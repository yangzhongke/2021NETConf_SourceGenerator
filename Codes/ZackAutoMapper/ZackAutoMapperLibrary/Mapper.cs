namespace AutoMapperLibrary
{
    public interface IMapper<TSource,TDest>
    {
        TDest Map(TSource src);
    }
}
