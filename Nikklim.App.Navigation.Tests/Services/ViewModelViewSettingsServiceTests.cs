using Nikklim.App.Navigation.Abstractions.Extensions;
using Nikklim.App.Navigation.Abstractions.Models;
using Nikklim.App.Navigation.Abstractions.Services.Implementation;

namespace Nikklim.App.Navigation.Tests.Services
{
    [TestFixture]
    public class ViewModelViewSettingsServiceTests
    {
        private void Init(out ViewModelViewSettingsService viewModelViewSettingsService)
        {
            viewModelViewSettingsService = new ViewModelViewSettingsService();
        }

        [Test]
        public void When_Registered_GetByViewModel_Returns_ThatValue()
        {
            //Arrange
            Init(out ViewModelViewSettingsService viewModelViewSettingsService);
            //Act
            viewModelViewSettingsService.Register<TestView, TestViewModel>();
            ViewModelViewResponse response = viewModelViewSettingsService.GetByViewModel<TestViewModel>();
            //Assert
            Assert.That(response.View, Is.EqualTo(typeof(TestView)));
            Assert.That(response.ViewModel, Is.EqualTo(typeof(TestViewModel)));
        }

        [Test]
        public void When_Registered_GetByView_Returns_ThatValue()
        {
            //Arrange
            Init(out ViewModelViewSettingsService viewModelViewSettingsService);
            //Act
            viewModelViewSettingsService.Register<TestView, TestViewModel>();
            ViewModelViewResponse response = viewModelViewSettingsService.GetByView<TestView>();
            //Assert
            Assert.That(response.View, Is.EqualTo(typeof(TestView)));
            Assert.That(response.ViewModel, Is.EqualTo(typeof(TestViewModel)));
        }

        [Test]
        public void When_NotRegistered_GetByView_ThrowsException()
        {
            //Arrange
            Init(out ViewModelViewSettingsService viewModelViewSettingsService);
            //Act
            //Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                ViewModelViewResponse response = viewModelViewSettingsService.GetByView<TestView>();
            });
        }

        [Test]
        public void When_NotRegistered_GetByViewModel_ThrowsException()
        {
            //Arrange
            Init(out ViewModelViewSettingsService viewModelViewSettingsService);
            //Act
            //Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                ViewModelViewResponse response = viewModelViewSettingsService.GetByViewModel<TestView>();
            });
        }

        private class TestView : Page { }
        private class TestViewModel { }
    }
}
