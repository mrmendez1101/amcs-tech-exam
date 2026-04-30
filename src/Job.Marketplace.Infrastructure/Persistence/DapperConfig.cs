using System.Data;
using Dapper;

namespace Job.Marketplace.Infrastructure.Persistence;

public static class DapperConfig
{
    public static void Apply()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }
}

public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
        => value is DateOnly d ? d : DateOnly.FromDateTime((DateTime)value);

    public override void SetValue(IDbDataParameter p, DateOnly value)
        => p.Value = value.ToDateTime(TimeOnly.MinValue);
}

public sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
        => value is DateTime dt ? new DateTimeOffset(dt) : (DateTimeOffset)value;

    public override void SetValue(IDbDataParameter p, DateTimeOffset value)
        => p.Value = value.UtcDateTime;
}
