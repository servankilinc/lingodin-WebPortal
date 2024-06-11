using WebApp.Models.Pagination;

namespace Core.Utils;

public class FSPModel
{
    public PagingRequest? PagingRequest { get; set; } = new PagingRequest(0, 5);
    public WebApp.Models.DynamicQuery.DynamicQuery? DynamicQuery { get; set; }
}
