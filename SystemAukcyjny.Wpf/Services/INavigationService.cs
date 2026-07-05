using System;
using SystemAukcyjny.Wpf.ViewModels;

namespace SystemAukcyjny.Wpf.Services
{
    public interface INavigationService
    {
        ViewModelBase? CurrentViewModel { get; }
        event Action? CurrentViewModelChanged;
        void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    }
}
