using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;

namespace Grocery.App.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly GlobalViewModel _global;

        [ObservableProperty]
        private string email = "user3@mail.com";

        [ObservableProperty]
        private string password = "user3";

        [ObservableProperty]
        private string loginMessage = "";
        
        [ObservableProperty]
        private Color loginMessageColor = Colors.Red;
        
        [ObservableProperty]
        private bool rememberMe = false;
        
        [ObservableProperty]
        private bool isLoading = false;
        
        public bool HasLoginMessage => !string.IsNullOrEmpty(LoginMessage);

        public LoginViewModel(IAuthService authService, GlobalViewModel global)
        {
            _authService = authService;
            _global = global;
            
            // Load remembered credentials if available
            LoadRememberedCredentials();
        }
        
        private void LoadRememberedCredentials()
        {
            try
            {
                if (Preferences.Get("RememberMe", false))
                {
                    Email = Preferences.Get("SavedEmail", "");
                    RememberMe = true;
                }
            }
            catch (Exception ex)
            {
                // Handle preferences loading error silently
                System.Diagnostics.Debug.WriteLine($"Error loading preferences: {ex.Message}");
            }
        }
        
        private void SaveCredentials()
        {
            try
            {
                if (RememberMe)
                {
                    Preferences.Set("RememberMe", true);
                    Preferences.Set("SavedEmail", Email);
                }
                else
                {
                    Preferences.Remove("RememberMe");
                    Preferences.Remove("SavedEmail");
                }
            }
            catch (Exception ex)
            {
                // Handle preferences saving error silently
                System.Diagnostics.Debug.WriteLine($"Error saving preferences: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ShowErrorMessage("Vul zowel e-mailadres als wachtwoord in.");
                return;
            }
            
            if (!IsValidEmail(Email))
            {
                ShowErrorMessage("Voer een geldig e-mailadres in.");
                return;
            }
            
            IsLoading = true;
            LoginMessage = "";
            
            try
            {
                // Simulate some loading time for better UX
                await Task.Delay(500);
                
                Client? authenticatedClient = _authService.Login(Email, Password);
                
                if (authenticatedClient != null)
                {
                    // Save credentials if remember me is checked
                    SaveCredentials();
                    
                    ShowSuccessMessage($"Welkom terug, {authenticatedClient.Name}!");
                    _global.Client = authenticatedClient;
                    
                    // Navigate to main app after short delay to show success message
                    await Task.Delay(1000);
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    ShowErrorMessage("Ongeldige inloggegevens. Controleer je e-mailadres en wachtwoord.");
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Er is een fout opgetreden tijdens het inloggen: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        [RelayCommand]
        private async Task NavigateToRegister()
        {
            await Toast.Make("Registratiefunctie komt binnenkort beschikbaar!").Show();
            // TODO: Implement navigation to registration page
            // await Shell.Current.GoToAsync(nameof(RegisterView));
        }
        
        [RelayCommand]
        private async Task ForgotPassword()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "E-mailadres vereist", 
                    "Voer eerst je e-mailadres in om je wachtwoord te resetten.", 
                    "OK");
                return;
            }
            
            bool result = await Application.Current.MainPage.DisplayAlert(
                "Wachtwoord resetten", 
                $"Wil je een reset-link naar {Email} versturen?", 
                "Ja", "Annuleren");
                
            if (result)
            {
                await Toast.Make($"Reset-link verzonden naar {Email}").Show();
                // TODO: Implement actual password reset functionality
            }
        }
        
        private void ShowErrorMessage(string message)
        {
            LoginMessage = message;
            LoginMessageColor = Colors.Red;
            OnPropertyChanged(nameof(HasLoginMessage));
        }
        
        private void ShowSuccessMessage(string message)
        {
            LoginMessage = message;
            LoginMessageColor = Colors.Green;
            OnPropertyChanged(nameof(HasLoginMessage));
        }
        
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}