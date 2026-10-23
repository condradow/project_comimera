using MudBlazor;

namespace WebFormular.Components.Pages.Auth
{
    public partial class LoginPage
    {
        private MudForm? _form;

        private string _email = string.Empty;
        private string _password = string.Empty;

        private bool _showPassword;
        private bool _isLoading;

        private void TogglePassword()
        {
            _showPassword = !_showPassword;
        }

        private async Task Login()
        {
            await _form!.Validate();

            if (!_form.IsValid)
                return;

            _isLoading = true;

            try
            {
                // Hier deine Authentifizierung einbauen
                await Task.Delay(1000);

                // Beispiel:
                // NavigationManager.NavigateTo("/");
            }
            finally
            {
                _isLoading = false;
            }
        }
        private bool _isShow;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        private void TogglePasswordVisibility()
        {
            if (_isShow)
            {
                _isShow = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                _isShow = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }
    }
}