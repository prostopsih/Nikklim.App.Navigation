using System.Threading.Tasks;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Abstractions.Extensions
{
    public static class PageNavigationServiceExtensions
    {
        public static Task RemovePreviousPagesByViewModel<TViewModel>(this IPageNavigationService pageNavigationService)
        {
            return pageNavigationService.RemovePreviousPagesByViewModel(typeof(TViewModel));
        }

        public static Task NavigateBackTo<TViewModel>(this IPageNavigationService pageNavigationService, bool animated = true)
        {
            return pageNavigationService.NavigateBackTo(typeof(TViewModel), animated);
        }

        /// <summary>
        /// Navigates to the view by TViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="pageNavigationService"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public static Task Navigate<TViewModel>(this IPageNavigationService pageNavigationService,
            bool animate = true)
        {
            return pageNavigationService.Navigate(typeof(TViewModel), animate);
        }

        /// <summary>
        /// Navigates to the view by TViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TNavigationParameters"></typeparam>
        /// <param name="animate"></param>
        /// <param name="pageNavigationService"></param>
        /// <param name="navigationParameters"></param>
        /// <returns></returns>
        public static Task Navigate<TViewModel, TNavigationParameters>(
            this IPageNavigationService pageNavigationService,
            TNavigationParameters navigationParameters, bool animate = true)
            where TViewModel : INavigationParameterizedViewModel<TNavigationParameters>
            where TNavigationParameters : class
        {
            return pageNavigationService.Navigate(typeof(TViewModel), navigationParameters, animate);
        }
    }
}