using System;
using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.Services
{
    public interface IPageNavigationService
    {
        object CurrentViewModel { get; }

        Task Navigate(Type viewModelType, bool animate = true);

        Task Navigate(Type viewModelType,
            object navigationParameters, bool animate = true);

        /// <summary>
        /// Navigates to the previous view, if current view is last will do nothing
        /// </summary>
        /// <param name="animate"></param>
        /// <returns></returns>
        Task NavigateBack(bool animate = true);

        Task NavigateBackTo(Type viewModelType, bool animate = true);

        Task RemovePreviousPage();

        Task RemoveAllPreviousPages();

        Task RemovePreviousPagesByViewModel(Type viewModelType);
    }
}
