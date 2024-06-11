using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using WebApp.Models.Dtos.UserDtos;
using WebApp.Models.Pagination;

namespace WebApp.ViewModels;

public class UserIndexViewModel
{
    public UserIndexFilterModel? FilterModel { get; set; }
    public Paginate<UserDetailResponseDto>? Resource { get; set; }


    // helper methods...
    public string CreateFormUrl(HttpRequest request)
    {
        var query = QueryGenerator(request);

        string url = UrlGenerator(request, query);
        return url;
    }

    public string CreateSortUrl(HttpRequest request, string sortBy)
    {
        var query = QueryGenerator(request);
        query["sortBy"] = sortBy;

        string url = UrlGenerator(request, query);
        return url;
    }

    public string CreateSizeUrl(HttpRequest request, int size)
    {
        var query = QueryGenerator(request);
        query["size"] = size.ToString();

        string url = UrlGenerator(request, query);
        return url;
    }

    public string CreatePageUrl(HttpRequest request, int page)
    {
        var query = QueryGenerator(request);
        query["page"] = page.ToString();

        string url = UrlGenerator(request, query);
        return url;
    }



    private Dictionary<string, StringValues> QueryGenerator(HttpRequest request)
    {
        string? size = request.Query.Where(q => q.Key == "size").SingleOrDefault().Value;
        string? sortBy = request.Query.Where(q => q.Key == "sortBy").SingleOrDefault().Value;
        string? page = request.Query.Where(q => q.Key == "page").SingleOrDefault().Value;

        var query = QueryHelpers.ParseQuery(request.QueryString.ToString());

        if (!string.IsNullOrEmpty(size)) query["size"] = size;
        if (!string.IsNullOrEmpty(sortBy)) query["sortBy"] = sortBy;
        if (!string.IsNullOrEmpty(page)) query["page"] = page;

        return query;
    }

    private string UrlGenerator(HttpRequest request, Dictionary<string, StringValues> query)
    {
        var queryString = new QueryString();
        foreach (var param in query)
        {
            foreach (var value in param.Value)
            {
                if (value != null) queryString = queryString.Add(param.Key, value);
            }
        }
        var uriBuilder = new UriBuilder($"{request.Scheme}://{request.Host}{request.Path}");
        uriBuilder.Query = queryString.ToString().TrimStart('?');

        return uriBuilder.ToString();
    }
}
