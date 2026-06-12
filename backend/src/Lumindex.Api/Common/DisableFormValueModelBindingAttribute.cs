using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Lumindex.Api.Common;

/// <summary>
/// Removes the form value provider factories for an action so the framework does not eagerly buffer
/// the request body. Required for endpoints that read large multipart uploads directly off the stream.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        for (var i = factories.Count - 1; i >= 0; i--)
        {
            if (factories[i] is FormValueProviderFactory
                or FormFileValueProviderFactory
                or JQueryFormValueProviderFactory)
            {
                factories.RemoveAt(i);
            }
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
