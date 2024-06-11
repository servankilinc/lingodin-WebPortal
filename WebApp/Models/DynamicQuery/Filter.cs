﻿namespace WebApp.Models.DynamicQuery;

public class Filter
{
    public string? Field { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public string? Logic { get; set; }
    public List<Filter>? Filters { get; set; }
}
