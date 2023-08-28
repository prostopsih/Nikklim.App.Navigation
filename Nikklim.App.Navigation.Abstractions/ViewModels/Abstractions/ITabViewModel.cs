namespace Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions
{
    public interface ITabViewModel
    {
        ITabbedViewModel ParentViewModel { get; set; }
    }
}