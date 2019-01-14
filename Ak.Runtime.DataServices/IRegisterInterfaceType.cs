namespace NegotiationsPlatform.DataServices.Client
{
    public interface IRegisterInterfaceType
    {
        void RegisterType<TInterface, TClass>() where TClass : TInterface;
    }
}