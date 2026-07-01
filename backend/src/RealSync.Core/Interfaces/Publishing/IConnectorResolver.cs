using RealSync.Core.Enums;

namespace RealSync.Core.Interfaces.Publishing;

public interface IConnectorResolver
{
    IPublishingConnector Resolve(PublishingChannelType channelType);
}
