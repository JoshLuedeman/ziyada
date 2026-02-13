using System.Text;
using Terminal.Gui;
using Ziyada.Helpers;
using Ziyada.Models;
using Ziyada.Services;

namespace Ziyada.Views;

public class PackageDetailsDialog : Dialog
{
    private readonly WingetService _winget;
    private readonly string _packageId;
    private readonly TextView _detailsView;
    private readonly ProgressBar _progressBar;
    private readonly Label _statusLabel;

    public PackageDetailsDialog(WingetService winget, string packageId, string packageName)
    {
        _winget = winget;
        _packageId = packageId;

        Title = $"Package Details - {packageName}";
        Width = Dim.Percent(80);
        Height = Dim.Percent(80);
        ColorScheme = Theme.Base;

        _statusLabel = new Label
        {
            Text = "Loading package details...",
            X = 1,
            Y = 0,
            Width = Dim.Fill(1),
            ColorScheme = Theme.Status,
        };

        _progressBar = new ProgressBar
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Height = 1,
            ProgressBarStyle = ProgressBarStyle.MarqueeContinuous,
            ColorScheme = Theme.Accent,
        };

        _detailsView = new TextView
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
            Height = Dim.Fill(2),
            ReadOnly = true,
            WordWrap = true,
            ColorScheme = Theme.Base,
        };

        var closeBtn = new Button
        {
            Text = "Close",
            ColorScheme = Theme.Button,
        };
        closeBtn.Accepting += (s, e) => Application.RequestStop();
        AddButton(closeBtn);

        Add(_statusLabel, _progressBar, _detailsView);

        // Start loading details asynchronously
        LoadDetailsAsync();
    }

    private void LoadDetailsAsync()
    {
        // Pulse the marquee animation
        var pulseTimer = Application.AddTimeout(TimeSpan.FromMilliseconds(100), () =>
        {
            _progressBar.Pulse();
            return true;
        });

        Task.Run(async () =>
        {
            try
            {
                var details = await _winget.ShowAsync(_packageId);
                Application.Invoke(() =>
                {
                    Application.RemoveTimeout(pulseTimer);
                    _progressBar.Visible = false;

                    if (details != null)
                    {
                        _statusLabel.Text = $"Package: {details.Name} ({details.Id})";
                        _detailsView.Text = FormatDetails(details);
                    }
                    else
                    {
                        _statusLabel.Text = "Failed to load package details";
                        _detailsView.Text = "Unable to retrieve package information. The package may not exist or there was an error communicating with winget.";
                    }

                    _statusLabel.SetNeedsDraw();
                    _detailsView.SetNeedsDraw();
                });
                Application.Wakeup();
            }
            catch (Exception ex)
            {
                Application.Invoke(() =>
                {
                    Application.RemoveTimeout(pulseTimer);
                    _progressBar.Visible = false;
                    _statusLabel.Text = "Error loading package details";
                    _detailsView.Text = $"An error occurred: {ex.Message}";
                    _statusLabel.SetNeedsDraw();
                    _detailsView.SetNeedsDraw();
                });
                Application.Wakeup();
            }
        });
    }

    private static string FormatDetails(PackageDetails details)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Name: {details.Name}");
        sb.AppendLine($"ID: {details.Id}");
        
        if (!string.IsNullOrEmpty(details.Version))
            sb.AppendLine($"Version: {details.Version}");
        
        if (!string.IsNullOrEmpty(details.Publisher))
            sb.AppendLine($"Publisher: {details.Publisher}");
        
        if (!string.IsNullOrEmpty(details.Source))
            sb.AppendLine($"Source: {details.Source}");
        
        sb.AppendLine();

        if (!string.IsNullOrEmpty(details.Description))
        {
            sb.AppendLine("Description:");
            sb.AppendLine(details.Description);
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(details.Homepage))
        {
            sb.AppendLine($"Homepage: {details.Homepage}");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(details.License))
        {
            sb.AppendLine($"License: {details.License}");
            if (!string.IsNullOrEmpty(details.LicenseUrl))
                sb.AppendLine($"License URL: {details.LicenseUrl}");
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(details.ReleaseNotes))
        {
            sb.AppendLine("Release Notes:");
            sb.AppendLine(details.ReleaseNotes);
            sb.AppendLine();
        }
        else if (!string.IsNullOrEmpty(details.ReleaseNotesUrl))
        {
            sb.AppendLine($"Release Notes URL: {details.ReleaseNotesUrl}");
            sb.AppendLine();
        }

        if (details.Dependencies.Count > 0)
        {
            sb.AppendLine("Dependencies:");
            foreach (var dep in details.Dependencies)
            {
                sb.AppendLine($"  • {dep}");
            }
        }

        return sb.ToString();
    }
}
