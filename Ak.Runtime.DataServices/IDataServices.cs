
using System;
namespace NegotiationsPlatform.DataServices.Client
{
    public interface IDataServices
    {
        Type GeType<T>() where T : class;
        T GetProperty<T>() where T : class;
    }

    public interface IDataServices<TIService> : IDataServices
    {
        //Type GeType<T>() where T : class;
       // protected Type targetType = typeof(TIService);
    }
}
