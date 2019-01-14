using System;
using NegotiationsPlatform.DataServices.Client.Channel;
using NegotiationsPlatform.Logger;


namespace NegotiationsPlatform.DataServices.Client
{

    public abstract class DuplexDataServices<TIService> : DuplexProxy<TIService> where TIService : class
    {
     
        private readonly string _endPointConfigurationName;
        private static readonly object SyncLock = new object();

        protected DuplexDataServices(string endPoint) : base(endPoint)
        {
            _endPointConfigurationName = endPoint;
        }

        protected DuplexDataServices()
        {
        }

        public TIService Channel
        {
            get
            {
                if (InnerChannelFactory == null)
                {
                    lock (SyncLock)
                    {
                        if (InnerChannelFactory == null)
                        {
                            Initialize(_endPointConfigurationName);
                        }
                    }
                }
                
                /* open channel */
                Open();
                //#if DEBUG return InnerChannel; #endif
                return InnerProxy;
            }
            protected set { if (value == null) throw new ArgumentNullException(nameof(value)); }
        }

        protected static TIService PrivateChannel { get; set; }
    }
}

