using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
    public static class HttpExtensions
    {
      public static void AddPaginationHeader(this HttpResponse response, int currentPage, int itemsPerPage,
                                             int totalItems, int totalPages)
      {
        var paginationHeader=new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
        var options=new JsonSerializerOptions
        {
            PropertyNamingPolicy=JsonNamingPolicy.CamelCase
        };
        /*And what we need to do is serialize this because our response headers when we add this it takes a key 
        and a string value.
        So what we can do is serialize this as json. And we'll need to bring this in from system.*/
        response.Headers.Add("Pagination",JsonSerializer.Serialize(paginationHeader, options));
        /*And then what we need to do because we're adding a custom header, we need to add a cause header onto
        this to make this header available.*/
        response.Headers.Add("Access-Control-Expose-Headers","Pagination");
      }  
    }
}