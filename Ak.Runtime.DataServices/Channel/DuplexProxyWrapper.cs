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
    internal class DuplexProxyWrapper<T> : RealProxy where T : class 
    {
        //private static readonly Log Logger = Log.GetLogger<ProxyWrapper<T>>();

        private static readonly Log Logger = Log.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      
     
        private T _channel;
#pragma warning disable 414
        private int _index;
#pragma warning restore 414
      
        private readonly DuplexProxy<T> _proxyBase;
        private Type _proxyType;

        public DuplexProxyWrapper(Type proxyType, DuplexProxy<T> proxyBase) : this(proxyType, proxyBase.InnerChannel)
        {
            _proxyBase = proxyBase;

        }
        public DuplexProxyWrapper(Type proxyType, T channel) : base(proxyType)
        {
          
            _proxyType = proxyType;
            _channel = channel;
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
                Logger.Error(string.Format("FaultException: ProxyWrapper<T> '{0}', Operation '{1}' ended with exception: ", typeof(T), methodCall.MethodName), e);
                
                messageException = e;
            }
            catch (CommunicationException e)
            {
                // any other faults 
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
  
}
