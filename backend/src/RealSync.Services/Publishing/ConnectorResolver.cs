using System;
using System.Collections.Generic;
using System.Linq;
using RealSync.Core.Enums;
using RealSync.Core.Interfaces.Publishing;

namespace RealSync.Services.Publishing;

public class ConnectorResolver : IConnectorResolver
{
    private readonly IEnumerable<IPublishingConnector> _connectors;

    public ConnectorResolver(IEnumerable<IPublishingConnector> connectors)
    {
        _connectors = connectors;
    }

    public IPublishingConnector Resolve(PublishingChannelType channelType)
    {
        var connector = _connectors.FirstOrDefault(c => c.ChannelType == channelType);
        if (connector == null)
            throw new NotSupportedException($"Kênh xuất bản {channelType} chưa được hỗ trợ.");
        return connector;
    }
}
