
using Microsoft.JSInterop;
using MudBlazor;
namespace WebFormular.Components.Layout
{
    public partial class MainLayout(IJSRuntime JS)
    {
        private bool _drawerOpen = true;
        private bool _isDarkMode = true;
        private MudTheme? _theme;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _theme = new MudTheme
            {
                PaletteLight = _lightPalette,
                PaletteDark = _darkPalette,

                LayoutProperties = new LayoutProperties
                {
                    DefaultBorderRadius = "12px",
                    DrawerWidthLeft = "260px",
                    AppbarHeight = "64px"
                },

                Typography = new Typography
                {
                    Default = new DefaultTypography
                    {
                        FontFamily = new[]
                        {
                            "-apple-system",
                            "BlinkMacSystemFont",
                            "SF Pro Display",
                            "SF Pro Text",
                            "Segoe UI",
                            "Helvetica Neue",
                            "Arial",
                            "sans-serif"
                        },
                        FontSize = "0.9375rem",
                        LineHeight = "1.5"
                    }
                }
            };
        }

        // Restores dark mode setting
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            var savedMode = await JS.InvokeAsync<string?>(
                "localStorage.getItem",
                "CoMiMeRa.DarkMode");

            if (bool.TryParse(savedMode, out bool darkMode))
            {
                if (_isDarkMode != darkMode)
                {
                    _isDarkMode = darkMode;
                    StateHasChanged();
                }
            }
        }

        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        private async Task DarkModeToggle()
        {
            _isDarkMode = !_isDarkMode;

            await JS.InvokeVoidAsync(
                "localStorage.setItem",
                "CoMiMeRa.DarkMode",
                _isDarkMode.ToString());
        }

        // Apple-inspired Light Mode
        private readonly PaletteLight _lightPalette = new()
        {
            Primary = "#007AFF",
            Secondary = "#5856D6",
            Tertiary = "#5AC8FA",

            Background = "#F5F5F7",
            BackgroundGray = "#F2F2F7",
            Surface = "#FFFFFF",

            AppbarBackground = "#FFFFFF",
            AppbarText = "#1D1D1F",

            DrawerBackground = "#FFFFFF",
            DrawerText = "#1D1D1F",
            DrawerIcon = "#8E8E93",

            TextPrimary = "#1D1D1F",
            TextSecondary = "#6E6E73",
            TextDisabled = "#AEAEB2",

            ActionDefault = "#636366",
            ActionDisabled = "#C7C7CC",
            ActionDisabledBackground = "#F2F2F7",

            Info = "#007AFF",
            Success = "#34C759",
            Warning = "#FF9500",
            Error = "#FF3B30",

            LinesDefault = "#E5E5EA",
            TableLines = "#E5E5EA",
            Divider = "#E5E5EA",

            GrayLight = "#E5E5EA",
            GrayLighter = "#F5F5F7",

            OverlayLight = "#FFFFFF80",
            Black = "#000000",
            White = "#FFFFFF"
        };

        // Apple-inspired Dark Mode
        private readonly PaletteDark _darkPalette = new()
        {
            Primary = "#0A84FF",
            Secondary = "#5E5CE6",
            Tertiary = "#64D2FF",

            Background = "#1C1C1E",
            BackgroundGray = "#000000",
            Surface = "#2C2C2E",

            AppbarBackground = "#1C1C1E",
            AppbarText = "#FFFFFF",

            DrawerBackground = "#1C1C1E",
            DrawerText = "#F5F5F7",
            DrawerIcon = "#AEAEB2",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#AEAEB2",
            TextDisabled = "#636366",

            ActionDefault = "#AEAEB2",
            ActionDisabled = "#636366",
            ActionDisabledBackground = "#3A3A3C",

            Info = "#0A84FF",
            Success = "#30D158",
            Warning = "#FF9F0A",
            Error = "#FF453A",

            LinesDefault = "#38383A",
            TableLines = "#38383A",
            Divider = "#38383A",

            GrayLight = "#3A3A3C",
            GrayLighter = "#2C2C2E",

            OverlayLight = "#2C2C2E80",
            Black = "#000000",
            White = "#FFFFFF"
        };

        public string DarkLightModeButtonIcon => _isDarkMode switch
        {
            true => Icons.Material.Rounded.LightMode,
            false => Icons.Material.Rounded.DarkMode
        };
    }
}
