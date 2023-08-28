using Moq;
using Nikklim.App.Navigation.Abstractions.Extensions;
using Nikklim.App.Navigation.Extensions;
using Nikklim.App.Navigation.Services.Implementation;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Tests.Services
{
    [TestFixture]
    public class PageNavigationServiceTests
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

        private void Init(out PageNavigationService pageNavigationService)
        {
            pageNavigationService = new PageNavigationService(_navigationMock.Object,
                _viewModelViewsProviderMock.Object, _viewModelViewSettingsServiceMock.Object);
        }

#region Navigate

        [Test]
        public async Task When_Navigate_With_Parameters_ViewModel_Gets_This_Parameters()
        {
            //Arrange
            TestView testView = new TestView();
            DefaultNavigationParameters navigationParameters = new DefaultNavigationParameters();

            Mock<INavigationParameterizedViewModel<DefaultNavigationParameters>> navigationParameterizedViewModel
                = new Mock<INavigationParameterizedViewModel<DefaultNavigationParameters>>();

            Init(out PageNavigationService pageNavigationService);

            _viewModelViewSettingsServiceMock.Setup(service =>
                    service.GetByViewModel(typeof(INavigationParameterizedViewModel<DefaultNavigationParameters>)))
                .Returns(() => new ViewModelViewResponse
                {
                    View = typeof(TestView),
                    ViewModel = typeof(INavigationParameterizedViewModel<DefaultNavigationParameters>)
                });

            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(INavigationParameterizedViewModel<DefaultNavigationParameters>)))
                .ReturnsAsync(() => navigationParameterizedViewModel.Object);
            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
                .ReturnsAsync(() => testView);
            //Act
            await pageNavigationService.Navigate<INavigationParameterizedViewModel<DefaultNavigationParameters>, DefaultNavigationParameters>(
                navigationParameters);
            //Assert
            navigationParameterizedViewModel.VerifySet(model => model.NavigationParameters = navigationParameters);
        }

        [Test]
        public async Task When_Navigate_ViewModel_PreNavigated()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<IPreNavigableViewModel> navigatableViewModel
                = new Mock<IPreNavigableViewModel>();

            Init(out PageNavigationService pageNavigationService);

            _viewModelViewSettingsServiceMock.Setup(service =>
                    service.GetByViewModel(typeof(IPreNavigableViewModel)))
                .Returns(() => new ViewModelViewResponse
                {
                    View = typeof(TestView),
                    ViewModel = typeof(IPreNavigableViewModel)
                });

            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(IPreNavigableViewModel)))
                .ReturnsAsync(() => navigatableViewModel.Object);
            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
                .ReturnsAsync(() => testView);
            //Act
            await pageNavigationService.Navigate<IPreNavigableViewModel>();
            //Assert
            navigatableViewModel.Verify(model => model.PreNavigate(), Times.Once);
        }
        
        [Test]
        public async Task When_Navigate_ViewModel_PostNavigated()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<IPostNavigableViewModel> navigatableViewModel
                = new Mock<IPostNavigableViewModel>();

            Init(out PageNavigationService pageNavigationService);

            _viewModelViewSettingsServiceMock.Setup(service =>
                    service.GetByViewModel(typeof(IPostNavigableViewModel)))
                .Returns(() => new ViewModelViewResponse
                {
                    View = typeof(TestView),
                    ViewModel = typeof(IPostNavigableViewModel)
                });

            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(IPostNavigableViewModel)))
                .ReturnsAsync(() => navigatableViewModel.Object);
            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
                .ReturnsAsync(() => testView);
            //Act
            await pageNavigationService.Navigate<IPostNavigableViewModel>();
            //Assert
            navigatableViewModel.Verify(model => model.PostNavigate(), Times.Once);
        }

        [Test]
        public async Task When_Navigate_ViewModel_LastViewModel_Leaved()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<ILeaveableViewModel> leavableViewModel
                = new Mock<ILeaveableViewModel>();
            testView.BindingContext = leavableViewModel.Object;

            Init(out PageNavigationService pageNavigationService);

            _viewModelViewSettingsServiceMock.Setup(service =>
                    service.GetByViewModel(typeof(TestViewModel)))
                .Returns(() => new ViewModelViewResponse
                {
                    View = typeof(TestView),
                    ViewModel = typeof(TestViewModel)
                });

            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestViewModel)))
                .ReturnsAsync(() => new TestViewModel());
            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
                .ReturnsAsync(() => new TestView());

            _navigationMock.SetupGet(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView});

            //Act
            await pageNavigationService.Navigate<TestViewModel>();
            //Assert
            leavableViewModel.Verify(model => model.Leave(), Times.Once);
        }

        [Test]
        public async Task When_Navigate_View_Pushed()
        {
            //Arrange
            bool animate = true;
            TestView testView = new TestView();
            TestViewModel testViewModel = new TestViewModel();
            Init(out PageNavigationService pageNavigationService);

            _viewModelViewSettingsServiceMock.Setup(service =>
                    service.GetByViewModel(typeof(TestViewModel)))
                .Returns(() => new ViewModelViewResponse
                {
                    View = typeof(TestView),
                    ViewModel = typeof(TestViewModel)
                });

            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestViewModel)))
                .ReturnsAsync(() => testViewModel);
            _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
                .ReturnsAsync(() => testView);

            //Act
            await pageNavigationService.Navigate<TestViewModel>(animate);
            //Assert
            _navigationMock.VerifyGet(navigation => navigation.NavigationStack);
            _navigationMock.Verify(navigation => navigation.PushAsync(testView, animate));
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

#endregion

#region NavigateBack

        [Test]
        public async Task When_NavigateBack_NavigationStack_Has_One_View_DoesNothing()
        {
            //Arrange
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView()}).Verifiable();
            //Act
            await pageNavigationService.NavigateBack();
            //Assert
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_NavigateBack_NavigationStack_Has_Several_Views_Pop()
        {
            //Arrange
            bool animate = true;
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView(), new TestView()}).Verifiable();
            //Act
            await pageNavigationService.NavigateBack(animate);
            //Assert
            _navigationMock.Verify(navigation => navigation.PopAsync(animate), Times.Once);
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_NavigateBack_Cleaned()
        {
            //Arrange
            bool animate = true;
            TestView testView = new TestView();
            Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
            testView.BindingContext = cleanableViewModelMock.Object;
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView(), testView});
            //Act
            await pageNavigationService.NavigateBack(animate);
            //Assert
            cleanableViewModelMock.Verify(model => model.Clean(), Times.Once);
        }

        [Test]
        public async Task When_NavigateBack_Restored()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<IRestorableViewModel> restorableViewModelMock = new Mock<IRestorableViewModel>();
            testView.BindingContext = restorableViewModelMock.Object;
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView, new TestView()});
            _navigationMock.Setup(navigation => navigation.PopAsync(It.IsAny<bool>()))
                .Returns(() =>
                {
                    _navigationMock.Setup(navigation => navigation.NavigationStack)
                        .Returns(new List<Page> {testView});
                    return Task.FromResult<Page>(new TestView());
                });
            //Act
            await pageNavigationService.NavigateBack();
            //Assert
            restorableViewModelMock.Verify(model => model.Restore(), Times.Once);
        }

        [Test]
        public async Task When_NavigateBackTo_WithViewModelThatIsNotInStack_ThrowsException()
        {
            //Arrange
            TestView testView = new TestView();
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView, new TestView(), new TestView(), new TestView()})
                .Verifiable();
            //Act
            //Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await pageNavigationService.NavigateBackTo<TestViewModel>();
            });
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_NavigateBackTo_WithViewModelThatIsInStack_RemovesAllPagesInBetweenAndPops()
        {
            //Arrange
            TestView testView = new TestView();
            TestView testView1 = new TestView();
            TestView testView2 = new TestView();
            bool animated = true;
            testView.BindingContext = new TestViewModel();
            List<Page> navigationStack = new List<Page>
            {
                testView,
                testView1,
                testView2,
                new TestView()
            };
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(navigationStack)
                .Verifiable();
            _navigationMock.Setup(navigation => navigation.RemovePage(testView1))
                .Callback(() =>
                {
                    navigationStack.Remove(testView1);
                })
                .Verifiable();
            _navigationMock.Setup(navigation => navigation.RemovePage(testView2))
                .Callback(() =>
                {
                    navigationStack.Remove(testView2);
                })
                .Verifiable();
            _navigationMock.Setup(navigation => navigation.PopAsync(animated))
                .Returns(Task.FromResult(new Page()))
                .Verifiable();
            //Act
            await pageNavigationService.NavigateBackTo<TestViewModel>(animated);
            //Assert
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

#endregion

#region CurrentViewModel

        [Test]
        public void When_CurrentViewModel_NavigationStack_Has_Any_Returns_ViewModel()
        {
            //Arrange
            TestView testView = new TestView();
            TestViewModel testViewModel = new TestViewModel();
            testView.BindingContext = testViewModel;
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView});
            //Act
            object? returnedCurrentViewModel = pageNavigationService.CurrentViewModel;
            //Assert
            Assert.That(Equals(testViewModel, returnedCurrentViewModel));
        }

#endregion


#region Remove

        [Test]
        public async Task When_RemovePreviousPage_OnePage_InStack_DoesNothing()
        {
            //Arrange
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView()}).Verifiable();
            //Act
            await pageNavigationService.RemovePreviousPage();
            //Assert
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_RemovePreviousPage_Cleaned()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
            testView.BindingContext = cleanableViewModelMock.Object;

            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView, new TestView()});
            //Act
            await pageNavigationService.RemovePreviousPage();
            //Assert
            cleanableViewModelMock.Verify(model => model.Clean(), Times.Once);
        }

        [Test]
        public async Task When_RemovePreviousPage_Several_Pages_InStack_RemovesPrevious()
        {
            //Arrange
            TestView testView = new TestView();
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView(), testView, new TestView()}).Verifiable();
            //Act
            await pageNavigationService.RemovePreviousPage();
            //Assert
            _navigationMock.Verify(navigation => navigation.RemovePage(testView), Times.Once);
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_RemoveAllPreviousPages_Cleaned()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
            testView.BindingContext = cleanableViewModelMock.Object;

            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView, new TestView()});
            //Act
            await pageNavigationService.RemoveAllPreviousPages();
            //Assert
            cleanableViewModelMock.Verify(model => model.Clean(), Times.Once);
        }

        [Test]
        public async Task When_RemoveAllPreviousPages_OnePage_InStack_DoesNothing()
        {
            //Arrange
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView()}).Verifiable();
            //Act
            await pageNavigationService.RemoveAllPreviousPages();
            //Assert
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_RemoveAllPreviousPages_Several_Pages_InStack_RemovesAllPrevious()
        {
            //Arrange
            TestView testView1 = new TestView();
            TestView testView2 = new TestView();
            TestView testView3 = new TestView();
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView1, testView2, testView3, new TestView()}).Verifiable();
            //Act
            await pageNavigationService.RemoveAllPreviousPages();
            //Assert
            _navigationMock.Verify();
            _navigationMock.Verify(navigation => navigation.RemovePage(testView1), Times.Once);
            _navigationMock.Verify(navigation => navigation.RemovePage(testView2), Times.Once);
            _navigationMock.Verify(navigation => navigation.RemovePage(testView3), Times.Once);
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_RemovePreviousPagesByViewModel_No_Such_Pages_InStack_DoesNothing()
        {
            //Arrange
            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {new TestView(), new TestView(), new TestView()}).Verifiable();
            //Act
            await pageNavigationService.RemovePreviousPagesByViewModel<OtherTestViewModel>();
            //Assert
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task When_RemovePreviousPagesByViewModel_Cleaned()
        {
            //Arrange
            TestView testView = new TestView();
            Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
            testView.BindingContext = cleanableViewModelMock.Object;

            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView, new TestView()});
            //Act
            await pageNavigationService.RemovePreviousPagesByViewModel(cleanableViewModelMock.Object.GetType());
            //Assert
            cleanableViewModelMock.Verify(model => model.Clean(), Times.Once);
        }

        [Test]
        public async Task When_RemovePreviousPagesByViewModel_Several_Such_Pages_InStack_RemovesThem()
        {
            //Arrange
            TestView testView1 = new TestView();
            TestView testView2 = new TestView();
            TestView testView3 = new TestView();
            TestView testView4 = new TestView();

            TestViewModel testViewModel1 = new TestViewModel();
            TestViewModel testViewModel2 = new TestViewModel();
            TestViewModel testViewModel3 = new TestViewModel();

            OtherTestViewModel otherTestViewModel3 = new OtherTestViewModel();

            testView1.BindingContext = testViewModel1;
            testView2.BindingContext = testViewModel2;
            testView3.BindingContext = testViewModel3;
            testView4.BindingContext = otherTestViewModel3;

            Init(out PageNavigationService pageNavigationService);
            _navigationMock.Setup(navigation => navigation.NavigationStack)
                .Returns(new List<Page> {testView1, testView2, testView4,  new TestView(), testView3}).Verifiable();
            //Act
            await pageNavigationService.RemovePreviousPagesByViewModel<TestViewModel>();
            //Assert
            _navigationMock.Verify(navigation => navigation.RemovePage(testView1), Times.Once);
            _navigationMock.Verify(navigation => navigation.RemovePage(testView2), Times.Once);
            _navigationMock.Verify();
            _navigationMock.VerifyNoOtherCalls();
        }

#endregion
        
        private class TestViewModel { }

        private class OtherTestViewModel { }

        private class TestView : Page { }
    }
}
