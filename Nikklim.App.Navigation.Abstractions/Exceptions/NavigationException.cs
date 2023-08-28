using System;

namespace Nikklim.App.Navigation.Abstractions.Exceptions
{
    public class NavigationException : Exception
    {
        public NavigationException(string message) : base(message)
        {
        }
    }
}