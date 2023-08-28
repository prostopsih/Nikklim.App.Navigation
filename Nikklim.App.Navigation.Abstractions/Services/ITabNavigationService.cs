using System;
using System.Threading.Tasks;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Abstractions.Services
{
    public interface ITabNavigationService
    {
        object CurrentViewModel { get; }
        
        Task Navigate(Type viewModelType, bool animate = true);

        Task Navigate(Type viewModelType, object navigationParameters, bool animate = true);

        Task CleanPage(ITabbedViewModel tabbedViewModel);
        Task RestorePage(ITabbedViewModel tabbedViewModel);
        Task LeavePage(ITabbedViewModel tabbedViewModel);
    }
}