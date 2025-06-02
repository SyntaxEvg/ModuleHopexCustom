using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
/// <summary>
/// Собирает аттрибуты, и которые станут доступны в любом запросе 
/// </summary>
public interface IHeaderCollector
{
    Dictionary<string, string> Headers { get; }
}

public class HeaderCollector : IHeaderCollector
{
    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
}

public class CollectHeadersAttribute : ActionFilterAttribute
{
    private readonly string[] _requiredHeaders;
    private readonly bool _validateRequired;

    public CollectHeadersAttribute(bool validateRequired = true, params string[] requiredHeaders)
    {
        _requiredHeaders = requiredHeaders.Length > 0
            ? requiredHeaders
            : new[] { "x-api-key" };
        _validateRequired = validateRequired;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var headerCollector = context.HttpContext.RequestServices
            .GetRequiredService<IHeaderCollector>();

        var requestHeaders = context.HttpContext.Request.Headers;

        foreach (var headerName in _requiredHeaders)
        {
            if (requestHeaders.TryGetValue(headerName, out var value))
            {
                headerCollector.Headers[headerName] = value.ToString();
            }
            else if (_validateRequired)
            {
                context.Result = new BadRequestObjectResult($"Missing required header: {headerName}");
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}