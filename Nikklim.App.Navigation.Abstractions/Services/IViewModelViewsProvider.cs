using System;
using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.Services
{
    public interface IViewModelViewsProvider
    {
        Task<object> GetServiceAsync(Type serviceType);
    }
}