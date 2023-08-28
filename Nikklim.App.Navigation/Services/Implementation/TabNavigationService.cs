using Nikklim.App.Navigation.Abstractions.Exceptions;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;
using Nikklim.App.Navigation.Extensions;

namespace Nikklim.App.Navigation.Services.Implementation;

public class TabNavigationService : BaseNavigationService, ITabNavigationService
{
    private readonly INavigation _navigation;
    private Dictionary<ITabbedViewModel, BindableObject> _currentTabViewDictionary;
    private Dictionary<ITabbedViewModel, Dictionary<Type, BindableObject>> _usedTabsDictionary;

    public object CurrentViewModel
    {
        get { return GetLastView()?.BindingContext; }
    }
    
    public TabNavigationService(INavigation navigation, IViewModelViewSettingsService viewModelViewSettingsService,
        IViewModelViewsProvider services) : base(viewModelViewSettingsService, services)
    {
        _navigation = navigation;
        _usedTabsDictionary = new Dictionary<ITabbedViewModel, Dictionary<Type, BindableObject>>();
        _currentTabViewDictionary = new Dictionary<ITabbedViewModel, BindableObject>();
    }

    #region Navigation

    public async Task Navigate(Type viewModelType, bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
        {
            GetCurrentTabbedViewModel();
            await NavigateTabInternal(viewModelType, DefaultNavigationParameters.Default, animate);
        }).ConfigureAwait(false);
    }

    public async Task Navigate(Type viewModelType, object navigationParameters, bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
        {
            GetCurrentTabbedViewModel();
            await NavigateTabInternal(viewModelType, navigationParameters, animate);
        }).ConfigureAwait(false);
    }
    
    private async Task NavigateTabInternal(Type viewModelType, object navigationParameters, bool animate = true)
    {
        ViewModelViewResponse viewModelViewResponse = _viewModelViewSettingsService.GetByViewModel(viewModelType);
        
        bool wasNavigated = _usedTabsDictionary.TryGetValue(GetCurrentTabbedViewModel(),
            out Dictionary<Type, BindableObject> usedTabs) && usedTabs.ContainsKey(viewModelType);

        BindableObject lastView = GetLastView();

        if (!wasNavigated)
        {
            BindableObject newView = (await _services.GetServiceAsync(viewModelViewResponse.View) as BindableObject);

            if (newView.BindingContext == null)
            {
                object viewModel = await _services.GetServiceAsync(viewModelViewResponse.ViewModel);

                newView.BindingContext = viewModel;
            }

            SetViewModelNavigationParameters(newView.BindingContext, navigationParameters);
            
            StoreInformationAboutNavigation(newView);
            
            await PreNavigate(newView);

            await Navigate(newView, animate);

            await PostNavigate(newView);
        }
        else
        {
            BindableObject newView = _usedTabsDictionary[GetCurrentTabbedViewModel()][viewModelType];
            
            SetViewModelNavigationParameters(newView.BindingContext, navigationParameters);
            
            StoreInformationAboutNavigation(newView);
            
            await Navigate(newView, animate);

            await Restore(newView);
        }

        await Leave(lastView);
    }
    
    protected override async Task Navigate(BindableObject newView, bool animate)
    {
        ContentPage contentPage =
            _navigation.NavigationStack?.LastOrDefault() as ContentPage
            ?? throw new NavigationException($"Current View must be {typeof(ContentPage).FullName}");
        contentPage.Content = newView as View;
    }

    #endregion

    #region Tabbed View Model Navigation

    public async Task CleanPage(ITabbedViewModel tabbedViewModel)
    {
        if (_usedTabsDictionary.TryGetValue(tabbedViewModel, out Dictionary<Type, BindableObject> usedTabs))
        {
            await Task.WhenAll(usedTabs.Values
                .Select(GetCleanFunc)
                .Select(func => func.Invoke())
                .ToArray());
            _usedTabsDictionary.Remove(tabbedViewModel);
        }

        if (_currentTabViewDictionary.ContainsKey(tabbedViewModel))
        {
            _currentTabViewDictionary.Remove(tabbedViewModel);
        }
    }

    public async Task RestorePage(ITabbedViewModel tabbedViewModel)
    {
        if (_currentTabViewDictionary.TryGetValue(tabbedViewModel, out BindableObject bindableObject))
        {
            await Restore(bindableObject);
        }
    }

    public async Task LeavePage(ITabbedViewModel tabbedViewModel)
    {
        if (_currentTabViewDictionary.TryGetValue(tabbedViewModel, out BindableObject bindableObject))
        {
            await Leave(bindableObject);
        }
    }

    #endregion

    #region Caching Info

    protected override BindableObject GetLastView()
    {
        return _currentTabViewDictionary.TryGetValue(GetCurrentTabbedViewModel(), out var value) ? value : default;
    }
    
    private void StoreInformationAboutNavigation(BindableObject newView)
    {
        ITabbedViewModel tabbedViewModel = GetCurrentTabbedViewModel();
        ITabViewModel currentTabViewModel = newView.BindingContext as ITabViewModel;
        tabbedViewModel.CurrentTabViewModel = currentTabViewModel;
        _currentTabViewDictionary[tabbedViewModel] = newView;
        currentTabViewModel.ParentViewModel = tabbedViewModel;
        SetUsedTab(tabbedViewModel, newView);
    }

    private void SetUsedTab(ITabbedViewModel tabbedViewModel, BindableObject tabView)
    {
        Dictionary<Type, BindableObject> usedTabs;
        if (!_usedTabsDictionary.ContainsKey(tabbedViewModel))
        {
            usedTabs = new Dictionary<Type, BindableObject>();
            _usedTabsDictionary[tabbedViewModel] = usedTabs;
        }
        else
        {
            usedTabs = _usedTabsDictionary[tabbedViewModel];
        }

        if (usedTabs.ContainsKey(tabView.BindingContext.GetType()))
        {
            usedTabs.Remove(tabView.BindingContext.GetType());
        }

        usedTabs.Add(tabView.BindingContext.GetType(), tabView);
    }
    
    private ITabbedViewModel GetCurrentTabbedViewModel()
    {
        ITabbedViewModel tabbedViewModel =
            _navigation.NavigationStack?.LastOrDefault()?.BindingContext as ITabbedViewModel
            ?? throw new NavigationException($"Current ViewModel must be {typeof(ITabbedViewModel).FullName}");

        return tabbedViewModel;
    }

    #endregion
}