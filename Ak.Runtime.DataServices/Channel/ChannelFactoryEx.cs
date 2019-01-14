using System;
using System.Collections.Generic;
using System.ServiceModel;

using NegotiationsPlatform.Logger;


namespace NegotiationsPlatform.DataServices.Client.Channel
{
    internal class ChannelFactoryEx<T> : ChannelFactory<T> where T : class
    {
        private static readonly Log Logger = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static object _syncRoot = new object();
        private readonly ILogger _logger;

        private T _proxy;

        public T InnerProxy {get { return _proxy; }}

        /// <summary>
        /// Saves the number of channels which should be
        /// created per each endpoint
        /// </summary>
        private readonly int _numOfChannels;

        /// <summary>
        /// Constructor
        /// </summary>
        public ChannelFactoryEx(string ep, int numOfChannels = 1)
            : base(ep)
        {
            _logger = new ConsoleLogger();
            _numOfChannels = numOfChannels;
            
        }

        private IList<T> _channels;
        public IList<T> Channels
        {
            get
            {
                if (_channels != null) return _channels;
                _channels = new List<T>();
                return _channels;
            }
        }
        public T Channel { get; set; }
        public override T CreateChannel(EndpointAddress address, Uri via)
        {
            Logger.Info(string.Format("ChannelFactoryEx:CreateChannel: {0}", typeof(T)));

            //for (int i = 0; i < _numOfChannels; i++)
            //{
            var innerChannel = base.CreateChannel(address, via);

            var clientChannel = (ICommunicationObject)innerChannel;
            clientChannel.Open();

            Logger.Info(string.Format("ChannelFactoryEx:CreateChannel: Channel State {0}", clientChannel.State));
            ((ICommunicationObject)innerChannel).Faulted += Channel_Faulted;
             
            Channel = innerChannel;

            //}

            var extendedProxy = new ProxyWrapper<T>(typeof(T), _logger, this);
            //extendedProxy.Faulted += Channel_Faulted;
            _proxy = (T)extendedProxy.GetTransparentProxy();
            
            return InnerProxy;

        }
        
        public void Channel_Faulted(object obj, EventArgs events)
        {
            var faultedChannel = (IClientChannel)obj;
            (faultedChannel).Faulted -= Channel_Faulted;

            if (faultedChannel.State == CommunicationState.Faulted || faultedChannel.State == CommunicationState.Closed)
            {
                Logger.Fatal(string.Format("ChannelFactoryEx:Channel_Faulted: {0}, Channel State After if: {1}", typeof(T), faultedChannel.State));

                lock (_syncRoot)
                {
                    Logger.Fatal(string.Format("ChannelFactoryEx:Channel_Faulted: {0}, Channel State After Lock: {1}", typeof(T), faultedChannel.State));

                //    //Here we test the possible new one if thread is already past in this part of code
                  //  var index = (Channels as List<T>).FindIndex((p) => ReferenceEquals(p, faultedChannel));
                    

                    //Logger.NegotiationsPlatformLogger.Log.Critical(CategoryLog.DataServices,
                    //               string.Format("ChannelFactoryEx:Channel_Faulted: Get Index State : {0}", index));

                    //if (index >= 0)
                    //{

                        //var targetChannel = Channels[index] as IClientChannel;

                        if (faultedChannel.State == CommunicationState.Faulted ||
                            faultedChannel.State == CommunicationState.Closed)
                        {
                        Logger.Fatal(string.Format(
                                    "ChannelFactoryEx:Channel_Faulted: {0}, Channel State After if (twice): {1}",
                                    typeof (T), faultedChannel.State));
                            //(faultedChannel).Faulted -= Channel_Faulted;

                            faultedChannel.Abort();

                        Logger.Fatal(string.Format("ChannelFactoryEx:Channel_Faulted: {0}, Abort Channel: {1}",
                                    typeof (T), faultedChannel.State));                     

                            //Call virtual method from ChannelFactory
                            //and create again the proxy wrapper                       
                            CreateChannel();

                        }
                        else
                        {
                        Logger.Fatal(string.Format("ChannelFactoryEx:Channel_Faulted:  After Lock:TargetChannel {0}",
                                    faultedChannel.State));
                        }
                    //}
                }
            }
        }
    }
    
}
