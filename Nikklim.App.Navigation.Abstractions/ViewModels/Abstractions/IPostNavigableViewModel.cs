using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions
{
    public interface IPostNavigableViewModel
    {
        Task PostNavigate();
    }
}