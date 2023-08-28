using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Extensions;

namespace Nikklim.App.Navigation.Services.Implementation
{
    public class PageNavigationService : BaseNavigationService, IPageNavigationService
    {
        private readonly INavigation _navigation;

        public object? CurrentViewModel
        {
            get { return GetLastView()?.BindingContext; }
        }

        public PageNavigationService(INavigation navigation,
            IViewModelViewsProvider viewModelViewsProvider, IViewModelViewSettingsService viewModelViewSettingsService)
            : base(viewModelViewSettingsService, viewModelViewsProvider)
        {
            _navigation = navigation;
        }

        public async Task Navigate(Type viewModelType, bool animate = true)
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(() =>
                NavigateInternal(viewModelType, DefaultNavigationParameters.Default, animate)).ConfigureAwait(false);
        }

        public async Task Navigate(Type viewModelType, object navigationParameters, bool animate = true)
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(() =>
                NavigateInternal(viewModelType, navigationParameters, animate)).ConfigureAwait(false);
        }

        public async Task NavigateBack(bool animate = true)
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(() =>
                NavigateBackInternal(animate)).ConfigureAwait(false);
        }

        public async Task NavigateBackTo(Type viewModelType, bool animate = true)
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
            {
                bool hasThisViewModel = _navigation.NavigationStack.SkipLast(1)
                    .Any(page => page.BindingContext?.GetType() == viewModelType);

                if (!hasThisViewModel)
                {
                    throw new InvalidOperationException($"There is no page with type {viewModelType?.FullName}");
                }

                while (_navigation.NavigationStack[^2].BindingContext?.GetType() != viewModelType)
                {
                    await RemovePreviousPage();
                }

                await NavigateBackInternal(animate);
            }).ConfigureAwait(false);
        }

        public async Task RemovePreviousPage()
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
            {
                if (_navigation.NavigationStack.Count < 2)
                {
                    return;
                }

                Page pageToRemove = _navigation.NavigationStack[^2];

                if (pageToRemove != null)
                {
                    _navigation.RemovePage(pageToRemove);
                    await GetCleanFunc(pageToRemove)();
                }
            }).ConfigureAwait(false);
        }

        public async Task RemoveAllPreviousPages()
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
            {
                foreach (Page page in _navigation.NavigationStack.SkipLast(1))
                {
                    _navigation.RemovePage(page);
                    await GetCleanFunc(page)();
                }
            });
        }

        public async Task RemovePreviousPagesByViewModel(Type viewModelType)
        {
            await MainThreadExtensions.InvokeOnMainThreadAsync(async () =>
            {
                foreach (Page page in _navigation.NavigationStack.SkipLast(1).ToList()
                             .Where(page => page.BindingContext?.GetType() == viewModelType))
                {
                    _navigation.RemovePage(page);
                    await GetCleanFunc(page)();
                }
            }).ConfigureAwait(false);
        }

        private async Task NavigateBackInternal(bool animate)
        {
            if (_navigation.NavigationStack.Count < 2)
            {
                return;
            }

            Func<Task> cleanFunc = await Pop(animate);

            await Restore(GetLastView());

            await cleanFunc();
        }

        private async Task<Func<Task>> Pop(bool animate)
        {
            Func<Task> cleanFunc = GetCleanFunc(GetLastView());

            await _navigation.PopAsync(animate);

            return cleanFunc;
        }

        protected override BindableObject? GetLastView()
        {
            return _navigation.NavigationStack?.LastOrDefault();
        }

        protected override async Task Navigate(BindableObject newView, bool animate)
        {
            await _navigation.PushAsync(newView as Page, animate);
        }
    }
}
