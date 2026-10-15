using System.IO;
using System.Windows;
using System.Windows.Controls;
namespace ServiceLens;
public sealed class ProfileDialog : Window
{
    public Profile? Result { get; private set; }
    public ProfileDialog(Profile? original = null)
    {
        Title = original is null ? "Cadastrar aplicação" : "Editar aplicação";
        Width = 560; Height = 440; ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var panel = new StackPanel { Margin = new Thickness(20) }; Content = panel;
        TextBox Field(string label, string value)
        {
            panel.Children.Add(new TextBlock { Text = label, Margin = new Thickness(3, 9, 3, 0) });
            var box = new TextBox { Text = value }; panel.Children.Add(box); return box;
        }
        var name = Field("Nome de exibição", original?.Label ?? "");
        var service = Field("Nome interno do serviço (ex.: PrinterWebApp)", original?.ServiceName ?? "");
        var folder = Field("Pasta dos logs", original?.Folder ?? "");
        var pattern = Field("Arquivo fixo ou padrão (ex.: app-*.log)", original?.Pattern ?? "*.log");
        var save = new Button { Content = "Salvar", IsDefault = true, Margin = new Thickness(3, 18, 3, 3) };
        panel.Children.Add(save);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(service.Text) || !Path.IsPathFullyQualified(folder.Text) || string.IsNullOrWhiteSpace(pattern.Text) || pattern.Text.IndexOfAny(['/', '\\', ':']) >= 0)
            { MessageBox.Show(this, "Preencha nome, serviço, pasta absoluta e um padrão sem diretórios."); return; }
            Result = new Profile { Id = original?.Id ?? Guid.NewGuid(), Label = name.Text.Trim(), ServiceName = service.Text.Trim(), Folder = folder.Text.Trim(), Pattern = pattern.Text.Trim() };
            DialogResult = true;
        };
    }
}
