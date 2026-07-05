using System;
using SystemAukcyjny.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace SystemAukcyjny.Wpf.Services
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private ViewModelBase? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ViewModelBase? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                CurrentViewModelChanged?.Invoke();
            }
        }

        public event Action? CurrentViewModelChanged;

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        }
    }
}
