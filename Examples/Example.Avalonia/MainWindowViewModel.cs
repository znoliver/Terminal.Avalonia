using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Example.Avalonia;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _text = string.Empty;

    private readonly ShellStream _shellStream;

    public MainWindowViewModel()
    {
        var sshClient = new SshClient("192.168.110.191", 2224, "zksd", "1");
        sshClient.Connect();
        _shellStream = sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);

        _shellStream.DataReceived += ShellStreamOnDataReceived;
    }

    [RelayCommand]
    private void OnInput(string input)
    {
        _shellStream.Write(input);
    }

    private void ShellStreamOnDataReceived(object? sender, ShellDataEventArgs e)
    {
        this.Text += Encoding.Default.GetString(e.Data);
    }
}