using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;

namespace DevExpress.Web.OfficeAzureCommunication {
    public class ServiceBusBase<T> {
        ServiceBusSettings serviceBusSettings;

        public ServiceBusBase(ServiceBusSettings serviceBusSettings) {
            this.serviceBusSettings = serviceBusSettings;
        }

        public ServiceEndpoint CreateEndpoint() {
            var contractDescription = ContractDescription.GetContract(typeof(T));
            var endPointAddress = new EndpointAddress(GetServiceButUri());
            var binding = new NetEventRelayBinding(EndToEndSecurityMode.None, RelayEventSubscriberAuthenticationType.None);
            
            var endpoint = new ServiceEndpoint(contractDescription, binding, endPointAddress);

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(serviceBusSettings.SharedAccessKeyName, serviceBusSettings.SharedAccessKey);
            var behavior = new TransportClientEndpointBehavior(tokenProvider);
            endpoint.EndpointBehaviors.Add(behavior);

            return endpoint;
        }

        private Uri GetServiceButUri() {
            Uri serviceButUri = ServiceBusEnvironment.CreateServiceUri(
                serviceBusSettings.ServiceBusURISchema,
                serviceBusSettings.ServiceNamespace,
                serviceBusSettings.ServicePath);

            return serviceButUri;
        }
    }

    public class ServiceBusPublisher<T> : ServiceBusBase<T> where T : IClientChannel {
        ChannelFactory<T> channelFactory;
        T channel;
        bool disposed = false;

        public ServiceBusPublisher(ServiceBusSettings serviceBusSettings) : base(serviceBusSettings) {
            CreateChannel();
        }
        public void CreateChannel() {
            var serviceEndpoint = CreateEndpoint();

            channelFactory = new ChannelFactory<T>(serviceEndpoint);
            channel = channelFactory.CreateChannel();
            channel.Faulted += OnChannelFaulted;
        }

        public T Channel {
            get {
                while(channel.State != CommunicationState.Opened) {
                    if(channel.State == CommunicationState.Opening) {
                        Thread.Sleep(100);
                    } else {
                        try {
                            channel.Open();
                        } catch {
                        }
                    }

                }
                return channel;
            }
        }

        void OnChannelFaulted(object sender, EventArgs e) {
            var faultedChannel = (ICommunicationObject)sender;
            faultedChannel.Faulted -= OnChannelFaulted;

            KillChannel(faultedChannel);
            KillChannelFactory(channelFactory);

            CreateChannel();
        }

        void KillChannel(ICommunicationObject channelToKill) {
            if(channelToKill.State == CommunicationState.Opened) {
                channelToKill.Close();
            } else {
                channelToKill.Abort();
            }
        }


        void KillChannelFactory(ChannelFactory<T> channelFactoryToKill) {
            if(channelFactoryToKill.State == CommunicationState.Opened) {
                channelFactoryToKill.Close();
            } else {
                channelFactoryToKill.Abort();
            }
        }


        public void Dispose(bool disposing) {
            if(!disposed) {
                if(disposing) {
                    try {
                        KillChannel(channel);
                    } catch {
                    }


                    try {
                        KillChannelFactory(channelFactory);
                    } catch {
                    }

                    disposed = true;
                }
            }
        }

        ~ServiceBusPublisher() {
            Dispose(false);
        }
    }

    public class ServiceBusSubscriber<T> : ServiceBusBase<T> {
        ServiceHost serviceHost;
        bool disposed = false;

        public ServiceBusSubscriber(ServiceBusSettings serviceBusSettings)
            : base(serviceBusSettings) {
            CreateServiceHost();
        }
        public void CreateServiceHost() {
            var serviceEndpoint = CreateEndpoint();
            serviceHost = new ServiceHost(Activator.CreateInstance(typeof(T)));
            serviceHost.Faulted += OnServiceHostFaulted;
            serviceHost.Description.Endpoints.Add(serviceEndpoint);
            serviceHost.Open();
        }

        void OnServiceHostFaulted(object sender, EventArgs e) {
            ServiceHost faultedHost = (ServiceHost)sender;
            faultedHost.Faulted -= OnServiceHostFaulted;

            KillHost(faultedHost);

            CreateServiceHost();
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void KillHost(ServiceHost hostToKill) {
            if(hostToKill.State == CommunicationState.Opened) {
                hostToKill.Close();
            } else {
                hostToKill.Abort();
            }
        }

        public void Dispose(bool disposing) {
            if(!disposed) {
                if(disposing) {
                    try {
                        KillHost(serviceHost);
                    } catch {
                    } finally {
                        disposed = true;
                    }
                }
            }
        }

        ~ServiceBusSubscriber() {
            Dispose(false);
        }
    }
}
