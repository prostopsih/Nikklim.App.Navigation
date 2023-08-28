using System;
using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.Services
{
    public interface IPopupNavigationService
    {
        object CurrentViewModel { get; }

        Task PushPopup(Type viewModelType, bool animate = true);

        Task PushPopup(Type viewModelType, object navigationParameters,
            bool animate = true);

        Task PopPopup(bool animate = true);

        Task PopAllPopups(bool animate = true);
    }
}