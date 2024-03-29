﻿using System.Diagnostics;
using Marten;
using Seatpicker.Application.Features;
using Shared;

namespace Seatpicker.Infrastructure.Adapters.Database;

public class AggregateRepository : IAggregateRepository
{
    private readonly IDocumentStore store;
    private readonly ITenantProvider tenantProvider;

    public AggregateRepository(IDocumentStore store, ITenantProvider tenantProvider)
    {
        this.store = store;
        this.tenantProvider = tenantProvider;
    }

    public IAggregateTransaction CreateTransaction()
    {
        var session = store.LightweightSession(tenantProvider.GetTenant());
        return new AggregateTransaction(session);
    }
}

public class AggregateTransaction : IAggregateTransaction
{
    private readonly IDocumentSession session;

    public AggregateTransaction(IDocumentSession session)
    {
        this.session = session;
    }

    public void Update<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.Append(aggregate.Id, aggregate.RaisedEvents);
    }

    public void Create<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.StartStream<TAggregate>(aggregate.Id, aggregate.RaisedEvents);
    }

    public void Archive<TAggregate>(TAggregate aggregate)
        where TAggregate : AggregateBase
    {
        session.Events.ArchiveStream(aggregate.Id);
    }

    public Task Commit()
    {
        return session.SaveChangesAsync();
    }

    public Task<TAggregate?> Aggregate<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        return session.Events.AggregateStreamAsync<TAggregate>(id);
    }

    public async Task<bool> Exists<TAggregate>(Guid id)
        where TAggregate : AggregateBase
    {
        var streamState = await session.Events.FetchStreamStateAsync(id);
        if (streamState is null) return false;
        return !streamState.IsArchived;
    }

    public void Dispose()
    {
        session.Dispose();
    }
}