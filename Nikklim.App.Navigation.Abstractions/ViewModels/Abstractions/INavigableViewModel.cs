﻿using System.Threading.Tasks;

namespace Nikklim.App.Navigation.Abstractions.ViewModels.Abstractions
{
    public interface IPreNavigableViewModel
    {
        Task PreNavigate();
    }
}
