using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;

namespace Microsoft.AspNet.NodeServices.Angular
{
    [HtmlTargetElement(Attributes = PrerenderModuleAttributeName)]
    public class AngularPrerenderTagHelper : TagHelper
    {
        static INodeServices fallbackNodeServices; // Used only if no INodeServices was registered with DI
        
        const string PrerenderModuleAttributeName = "asp-ng2-prerender-module";
        const string PrerenderExportAttributeName = "asp-ng2-prerender-export";
        
        [HtmlAttributeName(PrerenderModuleAttributeName)]
        public string ModuleName { get; set; }
        
        [HtmlAttributeName(PrerenderExportAttributeName)]
        public string ExportName { get; set; }

        private IHttpContextAccessor contextAccessor;
        private INodeServices nodeServices;

        public AngularPrerenderTagHelper(IServiceProvider nodeServices, IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
            this.nodeServices = (INodeServices)nodeServices.GetService(typeof (INodeServices)) ?? fallbackNodeServices;
            
            // Consider removing the following. Having it means you can get away with not putting app.AddNodeServices()
            // in your startup file, but then again it might be confusing that you don't need to.
            if (this.nodeServices == null) {
                this.nodeServices = fallbackNodeServices = Configuration.CreateNodeServices(NodeHostingModel.Http);
            }
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var result = await AngularRenderer.RenderToString(
                nodeServices: this.nodeServices,
                componentModuleName: this.ModuleName,
                componentExportName: this.ExportName,
                componentTagName: output.TagName,
                requestUrl: UriHelper.GetEncodedUrl(this.contextAccessor.HttpContext.Request)
            );
            output.SuppressOutput();
            output.PostElement.AppendEncoded(result);
        }
    }
}
