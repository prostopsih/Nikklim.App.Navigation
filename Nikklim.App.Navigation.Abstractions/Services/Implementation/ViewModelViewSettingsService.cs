using System;
using System.Collections.Generic;
using System.Linq;
using Nikklim.App.Navigation.Abstractions.Models;

namespace Nikklim.App.Navigation.Abstractions.Services.Implementation
{
    public class ViewModelViewSettingsService : IViewModelViewSettingsService
    {
        private readonly List<ViewModelViewResponse> _settings;

        public ViewModelViewSettingsService()
        {
            _settings = new List<ViewModelViewResponse>();
        }

        public void Register(Type viewType, Type viewModelType)
        {
            _settings.Add(new ViewModelViewResponse
            {
                ViewModel = viewModelType,
                View = viewType
            });
        }

        public ViewModelViewResponse GetByViewModel(Type viewModelType)
        {
            return _settings.Single(response => response.ViewModel == viewModelType);
        }

        public ViewModelViewResponse GetByView(Type viewType)
        {
            return _settings.Single(response => response.View == viewType);
        }
    }
}
