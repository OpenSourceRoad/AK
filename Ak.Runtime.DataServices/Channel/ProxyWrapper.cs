using System;

using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel;
using System.ServiceModel.Security;

using NegotiationsPlatform.Logger;


namespace NegotiationsPlatform.DataServices.Client.Channel
{
    internal class ProxyWrapper<T> : RealProxy where T : class 
    {
        //private static readonly Log Logger = Log.GetLogger<ProxyWrapper<T>>();

        private static readonly Log Logger = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object _synchro = new object();
        private Type _proxyType;
        //private readonly ILogger _logger;
        private T[] _channels;
        private T _channel;
#pragma warning disable 414
        private int _index;
#pragma warning restore 414
        private readonly ChannelFactoryEx<T> _channelFactory;
        private readonly ProxyBase<T> _proxyBase;

        public ProxyWrapper(Type proxyType,
             ProxyBase<T> proxyBase)
            : this(proxyType, proxyBase.InnerChannel)
        {
            _proxyBase = proxyBase;

        }

        public ProxyWrapper(Type proxyType,
            ILogger logger, ChannelFactoryEx<T> channelFactory)
            : this(proxyType, logger, channelFactory.Channels.ToArray())
        {
            _channelFactory = channelFactory;

        }

        public ProxyWrapper(
            Type proxyType,
            T channel)
            : base(proxyType)
        {
           // _logger = new ConsoleLogger();
            
           _proxyType = proxyType;
            _channel = channel;
            _index = 0;
           
        }
        public ProxyWrapper(
            Type proxyType,
            ILogger logger,
            T[] channels)
            : base(proxyType)
        {
            //_logger = logger;
            _proxyType = proxyType;
            _channels = channels;
            _index = 0;
           
        }
       

        /// <summary>
        /// See <see cref="RealProxy.Invoke"/>
        /// </summary>
        public override IMessage Invoke(IMessage message)
        {
            //_proxyBase.Open();

            var methodCall = message as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;

            var result = _proxyBase.Invoke<object>(message);
           // var innerProxy = _channels[_index++ % _channels.Count()];
            var innerProxy = _channel;
            Debug.Assert(innerProxy != null);
            Exception messageException = null;

            try
            {
              // var result = methodInfo.Invoke(innerProxy, methodCall.InArgs);
                var returnMessage = new ReturnMessage(
                    result, // Operation result
                    null, // Out arguments
                    0, // Out arguments count
                    methodCall.LogicalCallContext, // Call context
                    methodCall); // Original message



                return returnMessage;
            }
            catch (CommunicationObjectAbortedException e)
            {
                // Object should be discarded if this is reached.  
                // Debugging discovered the following exception here:
                // "Connection can not be established because it has been aborted" 
                Logger.Error(string.Format("CommunicationObjectAbortedException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (CommunicationObjectFaultedException e)
            {
                Logger.Error(string.Format("CommunicationObjectFaultedException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (MessageSecurityException e)
            {
                Logger.Error(string.Format("MessageSecurityException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (ChannelTerminatedException e)
            {
                Logger.Error(string.Format("ChannelTerminatedException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (ServerTooBusyException e)
            {
                Logger.Error(string.Format("ServerTooBusyException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (EndpointNotFoundException e)
            {
                Logger.Error(string.Format("EndpointNotFoundException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (FaultException e)
            {
                // any other faults 
                //_logger.WriteLine(severity.Error,
                //    "Operation '{0}' ended with exception '{1}'",
                //    methodCall.MethodName, ex);
                Logger.Error(string.Format("FaultException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                
                messageException = e;
            }
            catch (CommunicationException e)
            {
                // any other faults 
                //_logger.WriteLine(severity.Error,
                //    "Operation '{0}' ended with exception '{1}'",
                //    methodCall.MethodName, ex);
                Logger.Error(string.Format("CommunicationException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
            catch (TimeoutException e)
            {
                // Sample error found during debug: 

                // The message could not be transferred within the allotted timeout of 
                //  00:01:00. There was no space available in the reliable channel's 
                //  transfer window. The time allotted to this operation may have been a 
                //  portion of a longer timeout.
                Logger.Error(string.Format("TimeoutException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                messageException = e;
            }
          
            catch (Exception e)
            {
                messageException = e;
                Logger.Fatal(string.Format("Exception: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
            }
           
            return new ReturnMessage(messageException, methodCall);
             
        }
      
    }
    /// <summary>
    /// The severity of the log
    /// </summary>
    public enum severity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Logger
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write the message to the log system
        /// </summary>
        void WriteLine(severity severity,
            string message,
            params object[] args);
    }

    /// <summary>
    /// Logger which output the messages to console
    /// </summary>
    internal class ConsoleLogger : ILogger
    {
        #region ILogger Members

        /// <summary>
        /// See <see cref="ILogger.WriteLine"/>
        /// </summary>
        public void WriteLine(severity severity,
            string message,
            params object[] args)
        {
            Console.WriteLine("{0}: {1}", severity.ToString(), string.Format(message, args));
        }
        #endregion
    }
}
