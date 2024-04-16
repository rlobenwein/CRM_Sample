using Microsoft.AspNetCore.Http;


namespace RLBW_ERP.Common
{
    public class GetUrl
    {
        private HttpContext _currentContext;
        public GetUrl (HttpContext currentContext)
        {
            _currentContext = currentContext;

        }
        public string GetCurrentUrl()
        {
            var request = _currentContext.Request;

            var host = request.Host.ToUriComponent();
            var pathBase = request.PathBase.ToUriComponent();
            var url = $"{request.Scheme}://{host}{pathBase}";

            return url;
        }
    }
}
