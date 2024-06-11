﻿namespace WebApp.Models.DynamicQuery;

public class DynamicQuery
{
    public IEnumerable<Sort>? Sort { get; set; }
    public Filter? Filter { get; set; }
}
