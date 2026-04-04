using System.Threading.Tasks;

namespace CentralLicenceApp.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync(string viewPath, object model);
    }
}
