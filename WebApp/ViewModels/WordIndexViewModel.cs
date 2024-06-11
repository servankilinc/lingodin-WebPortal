using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using WebApp.Models.Dtos.WordDtos;
using WebApp.Models.Pagination;

namespace WebApp.ViewModels;

public class WordIndexViewModel
{
    public WordIndexFilterModel? FilterModel { get; set; }
    public Paginate<WordResponseDto>? Resource { get; set; }


    // helper methods ...
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
        var query = QueryHelpers.ParseQuery(request.QueryString.ToString());

        foreach (var param in request.Query)
        {
            if (!string.IsNullOrEmpty(param.Value))
                query[param.Key] = param.Value;
        }
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