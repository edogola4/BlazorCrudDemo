using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BlazorCrudDemo.Web.Middleware
{
    [HtmlTargetElement("script", Attributes = "nonce")]
    public class NonceScriptTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NonceScriptTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                var nonce = _httpContextAccessor.HttpContext.Items["CspNonce"]?.ToString();
                if (!string.IsNullOrEmpty(nonce))
                {
                    output.Attributes.SetAttribute("nonce", nonce);
                }
            }
        }
    }
}
