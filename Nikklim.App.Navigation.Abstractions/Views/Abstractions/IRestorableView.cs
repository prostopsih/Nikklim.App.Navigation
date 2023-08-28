using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.Views.Abstractions
{
    public interface IRestorableView
    {
        Task Restore();
    }
}
