using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace WebFormular.Components.Pages.Auth
{
    public partial class LoginPage(IJSRuntime JS, NavigationManager Navigation)
    {
        private MudForm? _form;

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _loginError = string.Empty;

        private bool _isLoading;

        private bool _isShow;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        private async Task Login()
        {
            await _form!.Validate();

            if (!_form.IsValid)
                return;

            _isLoading = true;

            try
            {
                var module = await JS.InvokeAsync<IJSObjectReference>("import", "./js/auth.js");

                bool success;

                try
                {
                    success = await module.InvokeAsync<bool>(
                        "login", _username, _password);
                }
                finally
                {
                    await module.DisposeAsync();
                }

                if (!success)
                {
                    _loginError = "Benutzername oder Passwort falsch.";
                    return;
                }

                Navigation.NavigateTo("/", forceLoad: true);
            }
            catch (JSException)
            {
                _loginError = "Beim Login ist ein Fehler aufgetreten.";
            }
            finally
            {
                _isLoading = false;
            }
        }

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