
using System;

using System.ServiceModel;
using System.ServiceModel.Channels;
using NegotiationsPlatform.DataServices.Client.Channel;
using NegotiationsPlatform.Logger;


//http://blogs.msdn.com/b/wenlong/archive/2007/10/26/best-practice-always-open-wcf-client-proxy-explicitly-when-it-is-shared.aspx


namespace NegotiationsPlatform.DataServices.Client
{
    [Obsolete]
    public abstract class DataServices : IDataServices
    {
        private static readonly object _syncRoot = new object();
        IChannelFactory _innerFactory;

        public Type GeType<T>() where T : class
        {
            return typeof(T);
        }

        public T GetProperty<T>() where T : class
        {
            if (this._innerFactory != null)
            {
                return this._innerFactory.GetProperty<T>();
            }
            else
            {
                return null;
            }
        }

        protected IChannelFactory InnerChannelFactory
        {
            get { return this._innerFactory; }
            set { this._innerFactory = value; }
        }

    }

    [Obsolete]
    public class DataServices<TIService> : DataServices, IDataServices<TIService>, IDataServices where TIService : class
    {
        private static readonly Log Logger = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type serviceType;
        protected static string EndPoint;
        private static ChannelFactory<TIService> _innerFactory;
        internal new ChannelFactory<TIService> InnerChannelFactory
        {
            get { return _innerFactory; }
            private set { _innerFactory = value; }
        }
        internal TIService InnerChannel = default(TIService);

        private static object _syncRoot = new object();


        protected DataServices()
            : this(typeof(TIService))
        {

        }

        protected DataServices(Type serviceType)
        {
            this.serviceType = serviceType;
        }

        public TIService Channel
        {
            get
            {
                if (InnerChannelFactory == null)
                {
                    lock (_syncRoot)
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (InnerChannelFactory == null)
                        {
                            try
                            {
                                Logger.Info(string.Format("Create Channel Factory for {0}", typeof(TIService)));
                                

                                //Create
                                InnerChannelFactory = new ChannelFactoryEx<TIService>(EndPoint);

                                //Adjust endPoint
                                //InnerChannelFactory.AdjustEndpointAddress(EndPoint);

                                InnerChannel = InnerChannelFactory.CreateChannel();

                            }
                            catch (Exception ex)
                            {
                                Logger.Fatal(string.Format("Exception for Create Channel Factory: {0}", typeof(TIService), ex));                                
                            }

                        }
                    }
                }
                else
                {
                    if (!(Equals(InnerChannel, default(TIService))))
                    {
                        //var channel = InnerChannel as IClientChannel;
                        //////it ssems that channel obejct here is not protected by lock syncronization
                        //if ((channel.State == CommunicationState.Faulted) || (channel.State == CommunicationState.Closed))
                        //{
                        ////    //InnerChannelFactory = null;
                        //Channel_Faulted(channel, null);
                        //}
                    }
                    else
                    {
                        Logger.Error(string.Format("InnerChannel Equals(InnerChannel, default(TIService)): {0}", Equals(InnerChannel, default(TIService))));
                        lock (_syncRoot)
                        {
                            if (Equals(InnerChannel, default(TIService)))
                            {
                                try
                                {
                                    Logger.Error(string.Format("Inner Channel {0} is default(TIService)... cannot be arrived here!!!!", typeof(TIService)));

                                    InnerChannel = InnerChannelFactory.CreateChannel();

                                }
                                catch (Exception ex)
                                {
                                    Logger.Fatal(string.Format("Exception for Create Channel Factory: {0} ", typeof(TIService), ex));                                    
                                }
                            }
                        }

                    }


                }
                // Channel must be tested before return !!!!!
                return InnerChannel;
            }
        }

        private void Channel_Faulted(object obj, EventArgs events)
        {
            var faultedChannel = (ICommunicationObject)obj;
            (faultedChannel).Faulted -= Channel_Faulted;
            Logger.Error(string.Format("Faulted Channel: {0}", typeof(TIService)));

            if (faultedChannel.State == CommunicationState.Faulted)
            {
                lock (_syncRoot)
                {
                    if (faultedChannel.State == CommunicationState.Faulted)
                    {
                        Logger.Warn(string.Format("Abort Channel for {0}", typeof(TIService)));

                        faultedChannel.Abort();

                        Logger.Info(string.Format("Create Channel for {0}", typeof(TIService)));

                        InnerChannel = InnerChannelFactory.CreateChannel();
                        ((IClientChannel)InnerChannel).Open();
                        ((ICommunicationObject)InnerChannel).Faulted += Channel_Faulted;
                    }
                }
            }
        }


        protected void CloseChannel()
        {
            var channel = InnerChannel;
            if (((IChannel)channel).State == CommunicationState.Opened)
            {
                try
                {
                    ((IChannel)channel).Close();
                }
                catch (TimeoutException /* timeout */)
                {
                    // Handle the timeout exception
                    ((IChannel)channel).Abort();
                }
                catch (CommunicationException /* communicationException */)
                {
                    // Handle the communication exception
                    ((IChannel)channel).Abort();
                }
            }
        }


        protected void CloseFactory()
        {
            if (InnerChannelFactory.State == CommunicationState.Opened)
            {
                try
                {
                    InnerChannelFactory.Close();
                }
                catch (TimeoutException /* timeout */)
                {
                    // Handle the timeout exception
                    InnerChannelFactory.Abort();
                }
                catch (CommunicationException /* communicationException */)
                {
                    // Handle the communication exception
                    InnerChannelFactory.Abort();
                }
            }
        }
    }
}
