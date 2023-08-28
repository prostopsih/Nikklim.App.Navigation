using System.Threading.Tasks;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Abstractions.Extensions
{
    public static class TabNavigationServiceExtensions
    {
        /// <summary>
        /// Navigates to the view by TViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="tabNavigationService"></param>
        /// <param name="animate"></param>
        /// <returns></returns>
        public static Task Navigate<TViewModel>(this ITabNavigationService tabNavigationService,
            bool animate = true)
            where TViewModel : ITabViewModel
        {
            return tabNavigationService.Navigate(typeof(TViewModel), animate);
        }

        /// <summary>
        /// Navigates to the view by TViewModel
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TNavigationParameters"></typeparam>
        /// <param name="animate"></param>
        /// <param name="tabNavigationService"></param>
        /// <param name="navigationParameters"></param>
        /// <returns></returns>
        public static Task Navigate<TViewModel, TNavigationParameters>(this ITabNavigationService tabNavigationService,
            TNavigationParameters navigationParameters,
            bool animate = true)
            where TViewModel : INavigationParameterizedViewModel<TNavigationParameters>, ITabViewModel
            where TNavigationParameters : class
        {
            return tabNavigationService.Navigate(typeof(TViewModel), navigationParameters, animate);
        }
    }
}