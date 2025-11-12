using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CommunityCar.Api.Models;

public class ActionContext
{
    public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    public RouteData RouteData { get; set; } = new RouteData();
    public ActionDescriptor ActionDescriptor { get; set; } = new ActionDescriptor();
}
