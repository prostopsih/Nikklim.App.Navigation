using Nikklim.App.Navigation.Extensions;
using Nikklim.App.Navigation.Services.Implementation;
using Moq;
using Nikklim.App.Navigation.Abstractions.Exceptions;
using Nikklim.App.Navigation.Abstractions.Extensions;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Tests.Services;

[TestFixture]
public class TabNavigationServiceTests
{
    private Mock<INavigation> _navigationMock;
    private Mock<IViewModelViewsProvider> _viewModelViewsProviderMock;
    private Mock<IViewModelViewSettingsService> _viewModelViewSettingsServiceMock;
    
    [SetUp]
    public void SetUp()
    {
        MainThreadExtensions.IsBeingRunFromTest = true;
        _navigationMock = new Mock<INavigation>();
        _viewModelViewsProviderMock = new Mock<IViewModelViewsProvider>();
        _viewModelViewSettingsServiceMock = new Mock<IViewModelViewSettingsService>();
    }
    
    private void Init(out TabNavigationService tabNavigationService)
    {
        tabNavigationService = new TabNavigationService(_navigationMock.Object,
            _viewModelViewSettingsServiceMock.Object, _viewModelViewsProviderMock.Object);
    }

    [Test]
    public async Task When_Navigate_WithNotITabbedPageViewModel_ExceptionThrown()
    {
        //Arrange
        TestView testView = new TestView();
        testView.BindingContext = new TestViewModel();
        _navigationMock.SetupGet(navigation => navigation.NavigationStack)
            .Returns(new List<Page> {testView});
        Init(out TabNavigationService tabNavigationService);
        //Act
        //Assert
        Assert.ThrowsAsync<NavigationException>(async () =>
        {
            await tabNavigationService.Navigate<TestViewModel>();
        });
    }
    
    [Test]
    public async Task When_CleanPage_TabsThatWereOpened_Cleaned()
    {
        //Arrange
        int counter = 0;
        Action action = () => counter++;

        Mock<ITabbedViewModel> tabbedViewModelMock = new Mock<ITabbedViewModel>();
        tabbedViewModelMock.SetupSet(model => model.CurrentTabViewModel = It.IsAny<ITabViewModel>());
        TestView testView = new TestView();
        testView.BindingContext = tabbedViewModelMock.Object;
        _navigationMock.SetupGet(navigation => navigation.NavigationStack)
            .Returns(new List<Page> {testView});
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
            .ReturnsAsync(() => new TestView());
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(CleanableViewModel1)))
            .ReturnsAsync(new CleanableViewModel1(action));
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(CleanableViewModel1)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(CleanableViewModel1)
            });
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(CleanableViewModel2)))
            .ReturnsAsync(new CleanableViewModel2(action));
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(CleanableViewModel2)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(CleanableViewModel2)
            });
        Init(out TabNavigationService tabNavigationService);
        //Act
        await tabNavigationService.Navigate<CleanableViewModel1>();
        await tabNavigationService.Navigate<CleanableViewModel2>();
        await tabNavigationService.Navigate<CleanableViewModel1>();
        await tabNavigationService.Navigate<CleanableViewModel2>();
        await tabNavigationService.CleanPage(tabbedViewModelMock.Object);
        await tabNavigationService.CleanPage(tabbedViewModelMock.Object);
        //Assert
        Assert.That(counter, Is.EqualTo(2));
    }
    
    [Test]
    public async Task When_LeavePage_LastActiveTab_Leaved()
    {
        //Arrange
        int counterOnPageLeave = 0;
        int counterBeforeTabbedPageLeave = 0;
        bool isCalledFrom2 = false;
        bool useOnLeaveCounter = false;
        Action action1 = () =>
        {
            if (useOnLeaveCounter)
            {
                counterOnPageLeave++;
            }
            else
            {
                counterBeforeTabbedPageLeave++;
            }
        };
        Action action2 = () =>
        {
            action1();
            isCalledFrom2 = true;
        };

        Mock<ITabbedViewModel> tabbedViewModelMock = new Mock<ITabbedViewModel>();
        tabbedViewModelMock.SetupSet(model => model.CurrentTabViewModel = It.IsAny<ITabViewModel>());
        TestView testView = new TestView();
        testView.BindingContext = tabbedViewModelMock.Object;
        _navigationMock.SetupGet(navigation => navigation.NavigationStack)
            .Returns(new List<Page> {testView});
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
            .ReturnsAsync(() => new TestView());
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(LeavableViewModel1)))
            .ReturnsAsync(new LeavableViewModel1(action1));
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(LeavableViewModel1)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(LeavableViewModel1)
            });
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(LeavableViewModel2)))
            .ReturnsAsync(new LeavableViewModel2(action2));
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(LeavableViewModel2)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(LeavableViewModel2)
            });
        Init(out TabNavigationService tabNavigationService);
        //Act
        await tabNavigationService.Navigate<LeavableViewModel1>();
        await tabNavigationService.Navigate<LeavableViewModel2>();
        await tabNavigationService.Navigate<LeavableViewModel1>();
        await tabNavigationService.Navigate<LeavableViewModel2>();
        useOnLeaveCounter = true;
        await tabNavigationService.LeavePage(tabbedViewModelMock.Object);
        //Assert
        Assert.That(counterBeforeTabbedPageLeave, Is.EqualTo(3));
        Assert.That(counterOnPageLeave, Is.EqualTo(1));
        Assert.That(isCalledFrom2);
    }

    [Test]
    public async Task When_Navigate_WithSamePageTwoTimes_SecondTimeItsRestored()
    {
        //Arrange
        int navigateCounter = 0;
        int restoreCounter = 0;
        Action navigateAction = () => navigateCounter++;
        Action restoreAction = () => restoreCounter++;

        Mock<ITabbedViewModel> tabbedViewModelMock = new Mock<ITabbedViewModel>();
        tabbedViewModelMock.SetupSet(model => model.CurrentTabViewModel = It.IsAny<ITabViewModel>());
        TestView testView = new TestView();
        testView.BindingContext = tabbedViewModelMock.Object;
        _navigationMock.SetupGet(navigation => navigation.NavigationStack)
            .Returns(new List<Page> {testView});
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
            .ReturnsAsync(() => new TestView());
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(NavigableRestorableViewModel)))
            .ReturnsAsync(new NavigableRestorableViewModel(navigateAction, restoreAction));
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(NavigableRestorableViewModel)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(NavigableRestorableViewModel)
            });
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestViewModel)))
            .ReturnsAsync(new TestViewModel());
        _viewModelViewSettingsServiceMock.Setup(service => service.GetByViewModel(typeof(TestViewModel)))
            .Returns(new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(TestViewModel)
            });
        Init(out TabNavigationService tabNavigationService);
        //Act
        await tabNavigationService.Navigate<NavigableRestorableViewModel>();
        await tabNavigationService.Navigate<TestViewModel>();
        await tabNavigationService.Navigate<NavigableRestorableViewModel>();
        await tabNavigationService.Navigate<TestViewModel>();
        //Assert
        Assert.That(navigateCounter, Is.EqualTo(1));
        Assert.That(restoreCounter, Is.EqualTo(1));
    }
    
    private class NavigableRestorableViewModel : IPreNavigableViewModel, IRestorableViewModel, ITabViewModel
    {
        private readonly Action _navigate;
        private readonly Action _restore;

        public NavigableRestorableViewModel(Action navigate, Action restore)
        {
            _navigate = navigate;
            _restore = restore;
        }
        
        public Task PreNavigate()
        {
            _navigate();
            return Task.CompletedTask;
        }

        public Task Restore()
        {
            _restore();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    
    private class CleanableViewModel1: ICleanableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public CleanableViewModel1(Action action)
        {
            _action = action;
        }

        public Task Clean()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    
    private class CleanableViewModel2: ICleanableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public CleanableViewModel2(Action action)
        {
            _action = action;
        }
        public Task Clean()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class LeavableViewModel1 : ILeaveableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public LeavableViewModel1(Action action)
        {
            _action = action;
        }
        public Task Leave()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class LeavableViewModel2 : ILeaveableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public LeavableViewModel2(Action action)
        {
            _action = action;
        }
        public Task Leave()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class RestorableViewModel1 : IRestorableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public RestorableViewModel1(Action action)
        {
            _action = action;
        }
        public Task Restore()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class RestorableViewModel2 : IRestorableViewModel, ITabViewModel
    {
        private readonly Action _action;

        public RestorableViewModel2(Action action)
        {
            _action = action;
        }
        public Task Restore()
        {
            _action();
            return Task.CompletedTask;
        }

        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class TestViewModel : ITabViewModel
    {
        public ITabbedViewModel ParentViewModel { get; set; }
    }
    private class TestView : ContentPage { }
}