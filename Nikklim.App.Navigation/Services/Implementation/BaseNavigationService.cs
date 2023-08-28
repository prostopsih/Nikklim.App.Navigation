using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;
using Nikklim.App.Navigation.Abstractions.Views.Abstractions;

namespace Nikklim.App.Navigation.Services.Implementation;

public abstract class BaseNavigationService
{
    protected readonly IViewModelViewSettingsService _viewModelViewSettingsService;
    protected readonly IViewModelViewsProvider _services;

    protected BaseNavigationService(IViewModelViewSettingsService viewModelViewSettingsService,
        IViewModelViewsProvider services)
    {
        _viewModelViewSettingsService = viewModelViewSettingsService;
        _services = services;
    }

    protected async Task NavigateInternal(
        Type viewModelType, object navigationParameters, bool animate = true)
    {
        ViewModelViewResponse viewModelViewResponse = _viewModelViewSettingsService.GetByViewModel(viewModelType);

        BindableObject? lastView = GetLastView();

        BindableObject newView = (await _services.GetServiceAsync(viewModelViewResponse.View) as BindableObject)!;

        if (newView.BindingContext == null)
        {
            object viewModel = (await _services.GetServiceAsync(viewModelViewResponse.ViewModel));

            newView.BindingContext = viewModel;
        }

        SetViewModelNavigationParameters(newView.BindingContext, navigationParameters);

        await PreNavigate(newView);

        await Navigate(newView, animate);

        await PostNavigate(newView);

        await Leave(lastView);
    }

    protected void SetViewModelNavigationParameters(object viewModel, object? parameters)
    {
        var interfaceType = viewModel.GetType().GetInterfaces()
            .FirstOrDefault(t => t.IsGenericType &&
                                 t.GetGenericTypeDefinition() == typeof(INavigationParameterizedViewModel<>) &&
                                 t.GetGenericArguments().FirstOrDefault() == parameters?.GetType());

        if (interfaceType != null)
        {
            var navigationParametersProperty = interfaceType.GetProperty(nameof(INavigationParameterizedViewModel<DefaultNavigationParameters>.NavigationParameters));
            navigationParametersProperty?.SetValue(viewModel, parameters);
        }
    }

    protected async Task PreNavigate(BindableObject? view)
    {
        await Task.WhenAll(PreNavigateViewModel(view?.BindingContext), PreNavigateView(view));
    }

    private async Task PreNavigateViewModel(object viewModel)
    {
        if (viewModel is IPreNavigableViewModel preNavigableViewModel)
        {
            await preNavigableViewModel.PreNavigate();
        }
    }
    
    private async Task PreNavigateView(object view)
    {
        if (view is IPreNavigableView preNavigableView)
        {
            await preNavigableView.PreNavigate();
        }
    }
    
    protected async Task PostNavigate(BindableObject? view)
    {
        await Task.WhenAll(PostNavigateViewModel(view?.BindingContext), PostNavigateView(view));
    }

    private async Task PostNavigateViewModel(object viewModel)
    {
        if (viewModel is IPostNavigableViewModel postNavigableViewModel)
        {
            await postNavigableViewModel.PostNavigate();
        }
    }
    
    private async Task PostNavigateView(object view)
    {
        if (view is IPostNavigableView postNavigableView)
        {
            await postNavigableView.PostNavigate();
        }
    }
    
    protected async Task Restore(BindableObject? view)
    {
        await Task.WhenAll(RestoreViewModel(view?.BindingContext), RestoreView(view));
    }

    private async Task RestoreViewModel(object viewModel)
    {
        if (viewModel is IRestorableViewModel restorableViewModel)
        {
            await restorableViewModel.Restore();
        }
    }
    
    private async Task RestoreView(object view)
    {
        if (view is IRestorableView restorableView)
        {
            await restorableView.Restore();
        }
    }
    
    protected async Task Leave(BindableObject? view)
    {
        await Task.WhenAll(LeaveViewModel(view?.BindingContext), LeaveView(view));
    }

    private async Task LeaveViewModel(object viewModel)
    {
        if (viewModel is ILeaveableViewModel leaveableViewModel)
        {
            await leaveableViewModel.Leave();
        }
    }
    
    private async Task LeaveView(object view)
    {
        if (view is ILeaveableView leaveableView)
        {
            await leaveableView.Leave();
        }
    }
    
    protected Func<Task> GetCleanFunc(BindableObject? view)
    {
        object? viewModel = view?.BindingContext;
        return () => Task.WhenAll(CleanViewModel(viewModel), CleanView(view));
    }

    private async Task CleanViewModel(object viewModel)
    {
        if (viewModel is ICleanableViewModel cleanableViewModel)
        {
            await cleanableViewModel.Clean();
        }
    }
    
    private async Task CleanView(object view)
    {
        if (view is ICleanableView cleanableView)
        {
            await cleanableView.Clean();
        }
    }

    protected abstract BindableObject? GetLastView();

    protected abstract Task Navigate(BindableObject newView, bool animate);
}