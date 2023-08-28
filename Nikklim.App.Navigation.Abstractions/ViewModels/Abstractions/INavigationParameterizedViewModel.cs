namespace Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions
{
    public interface INavigationParameterizedViewModel<TParameters> where TParameters : class
    {
        TParameters NavigationParameters { get; set; }
    }
}
