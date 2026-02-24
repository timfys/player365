using System.Collections.Generic;

namespace SmartWinners.Models;

public class PlisioTransaction
{
    public string Status { get; set; }
    public Data Data { get; set; }
    public Links _Links { get; set; }
    public Meta _Meta { get; set; }
    
}


public class Data
{
    public List<Operation> Operations { get; set; }
}

public class Operation
{
    public string TxnId { get; set; }
    public string InvoiceUrl { get; set; }
    public string InvoiceTotalSum { get; set; }
    public int UserId { get; set; }
    public string ShopId { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public object TxUrl { get; set; }
    public string Id { get; set; }
}

public class Links
{
    public Link Self { get; set; }
    public Link First { get; set; }
    public Link Last { get; set; }
}

public class Link
{
    public string Href { get; set; }
}

public class Meta
{
    public int TotalCount { get; set; }
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
}