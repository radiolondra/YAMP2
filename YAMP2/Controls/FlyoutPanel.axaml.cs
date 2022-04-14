using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace YAMP2.Controls
{
    public class FlyoutPanel : Panel
    {
        public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<FlyoutPanel, bool>(name: "IsOpen");
        public bool IsOpen
        { get => GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }

        public FlyoutPanel()
        {
            
        }
    }
}
