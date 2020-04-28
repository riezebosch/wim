using System;
using System.Collections.Generic;

namespace AzureDevOpsRest
{
    public interface IRequest<TData>
    {
        string Resource { get; }
        IDictionary<string, object> QueryParams { get; }
        IDictionary<string, object> Headers { get; }
        Uri Url { get; }
    }
}