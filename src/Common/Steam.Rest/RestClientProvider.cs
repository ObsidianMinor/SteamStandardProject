using System;

namespace Steam.Rest
{
    public delegate IRestClient RestClientProvider(Uri baseUri);
}
