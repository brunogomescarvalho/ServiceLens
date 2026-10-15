using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
namespace ServiceLens;
public partial class MainWindow : Window
{
    private readonly ObservableCollection<Profile> profiles = [];
    private readonly ObservableCollection<LogLine> lines = [];
    private readonly LogFollower follower = new();
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private ICollectionView view = null!;
    private bool reading;
    private int generation;
    private DateTime nextStatus;
    public MainWindow()
    {
        InitializeComponent();
        ProfilesList.ItemsSource = profiles; Logs.ItemsSource = lines;
        view = CollectionViewSource.GetDefaultView(lines); view.Filter = Matches;
        try { foreach (var profile in ProfileStore.Load()) profiles.Add(profile); }
        catch (Exception ex) { MessageBox.Show(this, "Não foi possível ler os cadastros. O arquivo será preservado. " + ex.Message); IsEnabled = false; return; }
        timer.Tick += async (_, _) => await RefreshAsync(); timer.Start();
        Closed += (_, _) => { timer.Stop(); follower.Dispose(); };
        if (profiles.Count > 0) ProfilesList.SelectedIndex = 0;
    }
    private bool Matches(object item)
    {
        if (item is not LogLine line) return false;
        var level = (Level.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Todos";
        return line.Text.Contains(Search.Text, StringComparison.OrdinalIgnoreCase) && (level == "Todos" || line.Severity == level);
    }
    private void Filter_Changed(object sender, RoutedEventArgs e) { if (view is not null) view.Refresh(); }
    private async Task RefreshAsync()
    {
        if (reading || ProfilesList.SelectedItem is not Profile profile) return;
        reading = true;
        var revision = generation;
        try
        {
            // File reads are bounded; keeping them on this dispatcher serializes selection/disposal.
            foreach (var text in follower.Read(profile)) lines.Add(LogLine.Parse(text));
            while (lines.Count > 10000) lines.RemoveAt(0);
            FileStatus.Text = follower.Current ?? "Aguardando arquivo compatível na pasta configurada…";
            if (Follow.IsChecked == true && view.Cast<object>().LastOrDefault() is { } last) Logs.ScrollIntoView(last);
            Footer.Text = $"{lines.Count:N0} linhas em memória • UTF-8 • atualização a cada segundo";
            if (DateTime.UtcNow >= nextStatus)
            {
                nextStatus = DateTime.UtcNow.AddSeconds(5);
                var status = await WindowsServices.QueryAsync(profile.ServiceName);
                if (revision == generation) ServiceStatus.Text = status.Split('\n').FirstOrDefault(s => s.Contains("STATE") || s.Contains("ESTADO"))?.Trim() ?? status.Trim();
            }
        }
        catch (Exception ex) { if (revision == generation) FileStatus.Text = "Não foi possível atualizar: " + ex.Message; }
        finally { reading = false; }
    }
    private async void Selection_Changed(object sender, SelectionChangedEventArgs e)
    {
        generation++; follower.Dispose(); lines.Clear(); nextStatus = DateTime.MinValue;
        ProfileTitle.Text = (ProfilesList.SelectedItem as Profile)?.Label ?? "Selecione uma aplicação";
        ServiceStatus.Text = ""; FileStatus.Text = ""; await RefreshAsync();
    }
    private bool Save()
    {
        try { ProfileStore.Save(profiles); return true; }
        catch (Exception ex) { MessageBox.Show(this, "Falha ao salvar: " + ex.Message); return false; }
    }
    private void New_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new ProfileDialog { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is not { } profile) return;
        profiles.Add(profile);
        if (!Save()) { profiles.Remove(profile); return; }
        ProfilesList.SelectedItem = profile;
    }
    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesList.SelectedItem is not Profile original) return;
        var dialog = new ProfileDialog(original) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.Result is not { } replacement) return;
        var index = profiles.IndexOf(original); profiles[index] = replacement;
        if (!Save()) { profiles[index] = original; ProfilesList.SelectedItem = original; return; }
        ProfilesList.SelectedItem = replacement;
    }
    private void Remove_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesList.SelectedItem is not Profile profile || MessageBox.Show(this, "Remover este cadastro?", "ServiceLens", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        var index = profiles.IndexOf(profile); profiles.Remove(profile);
        if (!Save()) profiles.Insert(index, profile);
    }
    private void Clear_Click(object sender, RoutedEventArgs e) => lines.Clear();
    private void Folder_Click(object sender, RoutedEventArgs e)
    {
        if (ProfilesList.SelectedItem is not Profile profile) return;
        try { Process.Start(new ProcessStartInfo("explorer.exe") { Arguments = "\"" + profile.Folder + "\"", UseShellExecute = true }); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message); }
    }
    private async void Services_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var text = await WindowsServices.QueryAsync();
            new Window { Owner = this, Title = "Serviços — copie o SERVICE_NAME para o cadastro", Width = 750, Height = 600, Content = new TextBox { Text = text, IsReadOnly = true, AcceptsReturn = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, FontFamily = new System.Windows.Media.FontFamily("Consolas") } }.Show();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message); }
    }
}
