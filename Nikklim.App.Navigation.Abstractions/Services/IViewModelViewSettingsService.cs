using System;
using Nikklim.App.Navigation.Abstractions.Models;

namespace Nikklim.App.Navigation.Abstractions.Services
{
    public interface IViewModelViewSettingsService
    {
        void Register(Type viewType, Type viewModelType);

        ViewModelViewResponse GetByViewModel(Type viewModelType);

        ViewModelViewResponse GetByView(Type viewType);
    }
}
