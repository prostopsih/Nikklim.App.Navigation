namespace Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions
{
    public interface ITabbedViewModel
    {
        ITabViewModel CurrentTabViewModel { get; set; }
    }
}