using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using NegotiationsPlatform.Logger;

namespace NegotiationsPlatform.DataServices.Client.Channel
{
    public abstract class DuplexProxy<T> : ProxyBase<T> where T : class
    {
        private static readonly Log Logger = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // state
        private bool IsOpened { get; set; }
      

        // lock
        private readonly object m_channelLock = new object();
        
        private bool _isProxyCreated = false;
        private readonly ManualResetEvent _mProxyRecreationLock = new ManualResetEvent(true);
        

        // channel
        private T m_channel = default(T);
        // proxy
        private T m_proxy = default(T);

        protected override ChannelFactory<T> InnerChannelFactory { get; set; }

        #region Constructors

        protected DuplexProxy()
        {
            InnerChannelFactory = null;
        }

        protected DuplexProxy(string endpointConfigurationName)
            : this()
        {
            //Initialize(endpointConfigurationName);
        }

        protected override void Initialize(string endpointConfigurationName)
        {
            if (this._isInitialized) throw new InvalidOperationException("Object already initialized.");
            this._isInitialized = true;
            Logger.Info(string.Format("DataServicesProxyBase:CreateChannelFactory:Initialize EndPoint {0}", endpointConfigurationName));

            InnerChannelFactory = new DuplexChannelFactory<T>(new InstanceContext(this), endpointConfigurationName);
            this.m_channel = null;
        }

        #endregion

        #region Communication events

     
     
        #endregion

        #region ICommunicationObject Members

        //public event EventHandler Closed;
        //public event EventHandler Closing;
        //public event EventHandler Faulted;
        //public event EventHandler Opened;
        //public event EventHandler Opening;

        #endregion

        #region Invoke

        //public delegate void RetryInvokeHandler(out Message unreadMessage);
        //public event RetryInvokeHandler RetryInvoke;

        #endregion

    }
}
