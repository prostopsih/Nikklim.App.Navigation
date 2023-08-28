using System.Threading.Tasks;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Abstractions.Extensions
{
    public static class PopupNavigationServiceExtensions
    {
        public static Task PushPopup<TViewModel>(this IPopupNavigationService popupNavigationService,
            bool animate = true)
        {
            return popupNavigationService.PushPopup(typeof(TViewModel), animate);
        }

        public static Task PushPopup<TViewModel, TNavigationParameters>(this IPopupNavigationService popupNavigationService,
            TNavigationParameters navigationParameters,
            bool animate = true)
            where TViewModel : INavigationParameterizedViewModel<TNavigationParameters>
            where TNavigationParameters : class
        {
            return popupNavigationService.PushPopup(typeof(TViewModel), navigationParameters, animate);
        }
    }
}