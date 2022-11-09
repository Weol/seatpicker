using Azure;
using Azure.Data.Tables;

namespace Seatpicker.Adapters.Common;

public abstract class AbstractTableEntity : ITableEntity
{
    public abstract string Id { get; set; }
    
    public string PartitionKey { get; set; }
    public string RowKey { get => Id; set => Id = value; }
    public DateTimeOffset? Timestamp
    {
        get => DateTimeOffset.UtcNow; 
        set => throw new InvalidOperationException("Cannot set timestamp, its is always UTC now");
    }

    public ETag ETag { get; set; }
}