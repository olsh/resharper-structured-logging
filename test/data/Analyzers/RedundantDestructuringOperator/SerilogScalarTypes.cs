using System;
using Serilog;

namespace ConsoleApp
{
    public enum Status
    {
        Active
    }

    public static class Program
    {
        public static void Main(
            int count,
            decimal amount,
            bool enabled,
            char initial,
            Status status,
            Guid orderId,
            DateTime createdAt,
            DateTimeOffset updatedAt,
            TimeSpan elapsed,
            Uri endpoint,
            int? retries)
        {
            Log.Logger.Information("Processed {@Count} items for {@Amount}", count, amount);
            Log.Logger.Information("Enabled {@Enabled} with initial {@Initial} and status {@Status}", enabled, initial, status);
            Log.Logger.Information("Order {@OrderId} created at {@CreatedAt}, updated at {@UpdatedAt}", orderId, createdAt, updatedAt);
            Log.Logger.Information("Took {@Elapsed} for {@Endpoint} after {@Retries} retries", elapsed, endpoint, retries);
        }
    }
}
