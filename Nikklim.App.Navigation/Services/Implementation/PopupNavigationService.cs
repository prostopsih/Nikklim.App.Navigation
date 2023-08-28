using Mopups.Interfaces;
using Mopups.Pages;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Extensions;

namespace Nikklim.App.Navigation.Services.Implementation;

public class PopupNavigationService : BaseNavigationService, IPopupNavigationService
{
    private readonly IPopupNavigation _popupNavigation;
    
    public object? CurrentViewModel
    {
        get { return GetLastView()?.BindingContext; }
    }

    public PopupNavigationService(IPopupNavigation popupNavigation,
        IViewModelViewsProvider viewModelViewsProvider, IViewModelViewSettingsService viewModelViewSettingsService)
        : base(viewModelViewSettingsService, viewModelViewsProvider)
    {
        _popupNavigation = popupNavigation;
    }

    public async Task PushPopup(Type viewModelType, bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(() =>
            NavigateInternal(viewModelType, DefaultNavigationParameters.Default, animate)).ConfigureAwait(false);
    }

    public async Task PushPopup(Type viewModelType, object navigationParameters, bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(() =>
            NavigateInternal(viewModelType, navigationParameters, animate)).ConfigureAwait(false);
    }

    public async Task PopPopup(bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
        {
            if (_popupNavigation.PopupStack == null || _popupNavigation.PopupStack.Count == 0)
            {
                return;
            }

            Func<Task> cleanFunc = GetCleanFunc(_popupNavigation.PopupStack.Last());

            await _popupNavigation.PopAsync(animate);

            await Restore(GetLastView());

            await cleanFunc();
        }).ConfigureAwait(false);

    }

    public async Task PopAllPopups(bool animate = true)
    {
        await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
        {
            if (_popupNavigation.PopupStack == null || _popupNavigation.PopupStack.Count == 0)
            {
                return;
            }

            List<Func<Task>> cleanFuncs =
                _popupNavigation.PopupStack.Select(GetCleanFunc).ToList();

            await _popupNavigation.PopAllAsync(animate);

            foreach (Func<Task> cleanFunc in cleanFuncs)
            {
                await cleanFunc();
            }
        }).ConfigureAwait(false);
    }

    protected override BindableObject GetLastView()
    {
        return (_popupNavigation.PopupStack?.LastOrDefault() as BindableObject)!;
    }

    protected override async Task Navigate(BindableObject newView, bool animate)
    {
        await _popupNavigation.PushAsync((newView as PopupPage)!, animate);
    }
}