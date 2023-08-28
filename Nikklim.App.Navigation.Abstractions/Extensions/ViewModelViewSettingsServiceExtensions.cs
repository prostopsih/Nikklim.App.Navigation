using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;

namespace Nikklim.App.Navigation.Abstractions.Extensions
{
    public static class ViewModelViewSettingsServiceExtensions
    {
        public static void Register<TView, TViewModel>(this IViewModelViewSettingsService viewModelViewSettingsService)
        {
            viewModelViewSettingsService.Register(typeof(TView), typeof(TViewModel));
        }

        public static ViewModelViewResponse GetByView<TView>(this IViewModelViewSettingsService viewModelViewSettingsService)
        {
            return viewModelViewSettingsService.GetByView(typeof(TView));
        }

        public static ViewModelViewResponse GetByViewModel<TViewModel>(this IViewModelViewSettingsService viewModelViewSettingsService)
        {
            return viewModelViewSettingsService.GetByViewModel(typeof(TViewModel));
        }
    }
}