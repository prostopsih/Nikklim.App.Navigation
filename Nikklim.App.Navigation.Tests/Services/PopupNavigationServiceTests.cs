using Nikklim.App.Navigation.Extensions;
using Nikklim.App.Navigation.Services.Implementation;
using Mopups.Interfaces;
using Mopups.Pages;
using Moq;
using Nikklim.App.Navigation.Abstractions.Extensions;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services;
using Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions;

namespace Nikklim.App.Navigation.Tests.Services;

[TestFixture]
public class PopupNavigationServiceTests
{
    private Mock<IPopupNavigation> _popupNavigationMock;
    private Mock<IViewModelViewsProvider> _viewModelViewsProviderMock;
    private Mock<IViewModelViewSettingsService> _viewModelViewSettingsServiceMock;

    [SetUp]
    public void SetUp()
    {
        MainThreadExtensions.IsBeingRunFromTest = true;
        _popupNavigationMock = new Mock<IPopupNavigation>();
        _viewModelViewsProviderMock = new Mock<IViewModelViewsProvider>();
        _viewModelViewSettingsServiceMock = new Mock<IViewModelViewSettingsService>();
    }

    private void Init(out PopupNavigationService popupNavigationService)
    {
        popupNavigationService = new PopupNavigationService(_popupNavigationMock.Object,
            _viewModelViewsProviderMock.Object, _viewModelViewSettingsServiceMock.Object);
    }
    
    [Test]
    public async Task When_Navigate_ViewPushed()
    {
        //Arrange
        TestView testView = new TestView();
        bool animate = false;

        Init(out PopupNavigationService popupNavigationService);

        _viewModelViewSettingsServiceMock.Setup(service =>
                service.GetByViewModel(typeof(TestViewModel)))
            .Returns(() => new ViewModelViewResponse
            {
                View = typeof(TestView),
                ViewModel = typeof(TestViewModel)
            });

        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestView)))
            .ReturnsAsync(() => testView);
        _viewModelViewsProviderMock.Setup(provider => provider.GetServiceAsync(typeof(TestViewModel)))
            .ReturnsAsync(() => testView);
        _popupNavigationMock.Setup(navigation => navigation.PushAsync(testView, animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PushPopup<TestViewModel>(animate);
        //Assert
        _popupNavigationMock.Verify();
    }
    
    [Test]
    public async Task When_NavigateWithParameter_ViewPushed()
    {
        //Arrange
        TestView testView = new TestView();
        bool animate = false;
        DefaultNavigationParameters navigationParameters = new DefaultNavigationParameters();

        Mock<INavigationParameterizedViewModel<DefaultNavigationParameters>> navigationParameterizedViewModel
            = new Mock<INavigationParameterizedViewModel<DefaultNavigationParameters>>();

        Init(out PopupNavigationService popupNavigationService);

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
        _popupNavigationMock.Setup(navigation => navigation.PushAsync(testView, animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PushPopup<INavigationParameterizedViewModel<DefaultNavigationParameters>,
            DefaultNavigationParameters>(navigationParameters, animate);
        //Assert
        _popupNavigationMock.Verify();
    }

    [Test]
    public async Task When_PopPopup_StackEmpty_DoesNothing()
    {
        //Arrange
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>())
            .Verifiable();
        //Act
        await popupNavigationService.PopPopup();
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task When_PopPopup_StackNotEmpty_Pop()
    {
        //Arrange
        bool animate = false;
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>
            {
                new TestView()
            })
            .Verifiable();
        _popupNavigationMock.Setup(navigation => navigation.PopAsync(animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PopPopup(animate);
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task When_PopPopup_ViewModelCleanable_Clean()
    {
        //Arrange
        bool animate = false;
        TestView testView = new TestView();
        Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
        testView.BindingContext = cleanableViewModelMock.Object;
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>
            {
                testView
            })
            .Verifiable();
        _popupNavigationMock.Setup(navigation => navigation.PopAsync(animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        cleanableViewModelMock.Setup(model => model.Clean())
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PopPopup(animate);
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
        cleanableViewModelMock.Verify();
        cleanableViewModelMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task When_PopPopup_ViewModelRestorable_Restore()
    {
        //Arrange
        bool animate = false;
        TestView testView = new TestView();
        Mock<IRestorableViewModel> restorableViewModelMock = new Mock<IRestorableViewModel>();
        testView.BindingContext = restorableViewModelMock.Object;
        Init(out PopupNavigationService popupNavigationService);
        List<PopupPage> stackList = new List<PopupPage>
        {
            testView,
            new TestView()
        };
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(stackList)
            .Verifiable();
        _popupNavigationMock.Setup(navigation => navigation.PopAsync(animate))
            .Returns(Task.CompletedTask)
            .Callback(() => stackList.RemoveAt(stackList.Count - 1))
            .Verifiable();
        restorableViewModelMock.Setup(model => model.Restore())
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PopPopup(animate);
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
        restorableViewModelMock.Verify();
        restorableViewModelMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task When_PopAllPopups_StackEmpty_DoesNothing()
    {
        //Arrange
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>())
            .Verifiable();
        //Act
        await popupNavigationService.PopAllPopups();
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task When_PopAllPopups_StackNotEmpty_Pop()
    {
        //Arrange
        bool animate = false;
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>
            {
                new TestView()
            })
            .Verifiable();
        _popupNavigationMock.Setup(navigation => navigation.PopAllAsync(animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PopAllPopups(animate);
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
    }
    
    [Test]
    public async Task When_PopAllPopups_ViewModelCleanable_Clean()
    {
        //Arrange
        bool animate = false;
        TestView testView = new TestView();
        Mock<ICleanableViewModel> cleanableViewModelMock = new Mock<ICleanableViewModel>();
        testView.BindingContext = cleanableViewModelMock.Object;
        Init(out PopupNavigationService popupNavigationService);
        _popupNavigationMock.Setup(navigation => navigation.PopupStack)
            .Returns(new List<PopupPage>
            {
                testView
            })
            .Verifiable();
        _popupNavigationMock.Setup(navigation => navigation.PopAllAsync(animate))
            .Returns(Task.CompletedTask)
            .Verifiable();
        cleanableViewModelMock.Setup(model => model.Clean())
            .Returns(Task.CompletedTask)
            .Verifiable();
        //Act
        await popupNavigationService.PopAllPopups(animate);
        //Assert
        _popupNavigationMock.Verify();
        _popupNavigationMock.VerifyNoOtherCalls();
        cleanableViewModelMock.Verify();
        cleanableViewModelMock.VerifyNoOtherCalls();
    }

    private class TestView : PopupPage { }
    private class TestViewModel { }
}