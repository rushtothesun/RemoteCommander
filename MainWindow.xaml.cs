using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RemoteCommander
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private CommanderSettings _settings;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool AlwaysOnTop
        {
            get => _settings.AlwaysOnTop;
            set
            {
                if (_settings.AlwaysOnTop != value)
                {
                    _settings.AlwaysOnTop = value;
                    OnPropertyChanged(nameof(AlwaysOnTop));
                }
            }
        }

        public double WindowOpacity
        {
            get => _settings.WindowOpacity;
            set
            {
                if (_settings.WindowOpacity != value)
                {
                    _settings.WindowOpacity = value;
                    OnPropertyChanged(nameof(WindowOpacity));
                }
            }
        }

        public bool MutedColors
        {
            get => _settings.MutedColors;
            set
            {
                if (_settings.MutedColors != value)
                {
                    _settings.MutedColors = value;
                    OnPropertyChanged(nameof(MutedColors));
                }
            }
        }

        public MainWindow()
        {
            _settings = CommanderSettings.Load();
            InitializeComponent();
            this.DataContext = this;

            BotListBox.ItemsSource = _settings.RemoteBots;
            
            _settings.RemoteBots.CollectionChanged += (s, e) =>
            {
                _settings.Save();
                RebuildBotPanels();
            };

            Loaded += (s, e) =>
            {
                RebuildBotPanels();
                // Enforce initial states
                this.Topmost = AlwaysOnTop;
                this.Opacity = WindowOpacity;
            };
            
            Closing += (s, e) =>
            {
                _settings.Save();
            };
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RebuildBotPanels()
        {
            BotPanelsContainer.Children.Clear();

            if (_settings.RemoteBots != null)
            {
                foreach (var entry in _settings.RemoteBots)
                {
                    if (string.IsNullOrWhiteSpace(entry)) continue;
                    // Entry format: "IP:Port" or "IP:Port|Label"
                    var parts = entry.Split('|');
                    var address = parts[0].Trim();
                    var label = parts.Length > 1 ? parts[1].Trim() : address;
                    AddBotPanel(label, address);
                }
            }
        }

        private void AddBotPanel(string label, string address)
        {
            var headerText = new TextBlock
            {
                Text = label.ToUpper(),
                Foreground = Brushes.White,
                FontSize = 11
            };

            var group = new GroupBox
            {
                Header = headerText,
                Margin = new Thickness(3),
                MinWidth = 180
            };
            group.SetResourceReference(FrameworkElement.StyleProperty, "DPBGroupBox");
            
            // Adjust border colors internally for LOCAL vs REMOTE
            if (address == null) 
            {
                // Unused in standalone, but kept if needed
                group.BorderBrush = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }

            var stack = new StackPanel { Margin = new Thickness(4) };

            // Bot Lifecycle (Starts BotManager externally via BotStart command)
            var lifecycleRow = new WrapPanel { Margin = new Thickness(0, 2, 0, 8) };
            lifecycleRow.Children.Add(MakeButton("Start Bot", "BotStart", address,
                Color.FromRgb(76, 175, 80), Brushes.White));
            lifecycleRow.Children.Add(MakeButton("Stop Bot", "BotStop", address,
                Color.FromRgb(244, 67, 54), Brushes.White));
            stack.Children.Add(lifecycleRow);

            stack.Children.Add(MakeToggleRow("Follow", "StartFollow", "StopFollow", address));

            var followLocRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 4) };
            followLocRow.Children.Add(MakeSmallLabel("Town"));
            followLocRow.Children.Add(MakeButton("✓", "FollowTownOn", address, Color.FromRgb(76, 175, 80), Brushes.White, 20, 10));
            followLocRow.Children.Add(MakeButton("✗", "FollowTownOff", address, Color.FromRgb(244, 67, 54), Brushes.White, 20, 10));
            followLocRow.Children.Add(MakeSmallLabel("HO"));
            followLocRow.Children.Add(MakeButton("✓", "FollowHideoutOn", address, Color.FromRgb(76, 175, 80), Brushes.White, 20, 10));
            followLocRow.Children.Add(MakeButton("✗", "FollowHideoutOff", address, Color.FromRgb(244, 67, 54), Brushes.White, 20, 10));
            followLocRow.Children.Add(MakeSmallLabel("Heist"));
            followLocRow.Children.Add(MakeButton("✓", "FollowHeistOn", address, Color.FromRgb(76, 175, 80), Brushes.White, 20, 10));
            followLocRow.Children.Add(MakeButton("✗", "FollowHeistOff", address, Color.FromRgb(244, 67, 54), Brushes.White, 20, 10));
            stack.Children.Add(followLocRow);

            stack.Children.Add(MakeToggleRow("Attack", "StartAttack", "StopAttack", address));
            stack.Children.Add(MakeToggleRow("Loot", "StartLoot", "StopLoot", address));
            stack.Children.Add(MakeToggleRow("Auto-TP", "StartPortal", "StopPortal", address));

            var actionsRow = new WrapPanel { Margin = new Thickness(0, 6, 0, 2) };
            actionsRow.Children.Add(MakeButton("Teleport", "Teleport", address));
            actionsRow.Children.Add(MakeButton("Open Portal", "OpenPortal", address));
            stack.Children.Add(actionsRow);

            var portalRow2 = new WrapPanel { Margin = new Thickness(0, 0, 0, 4) };
            portalRow2.Children.Add(MakeButton("Enter Portal", "EnterPortal", address));
            portalRow2.Children.Add(MakeButton("New Instance", "NewInstance", address));
            stack.Children.Add(portalRow2);

            var otherRow = new WrapPanel { Margin = new Thickness(0, 2, 0, 6) };
            otherRow.Children.Add(MakeButton("Stash", "Stash", address));
            otherRow.Children.Add(MakeButton("Stash $", "StashCurrency", address));
            stack.Children.Add(otherRow);

            stack.Children.Add(MakeToggleRow("AutoDep", "AutoDepositOn", "AutoDepositOff", address));

            var stashTypeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 1, 0, 4) };
            stashTypeRow.Children.Add(new TextBlock { Text = "Stash", Width = 60, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            stashTypeRow.Children.Add(MakeButton("REGULAR", "UseRegularStash", address, Color.FromRgb(66, 133, 244), Brushes.White, 0, 10));
            stashTypeRow.Children.Add(MakeButton("GUILD", "UseGuildStash", address, Color.FromRgb(255, 152, 0), Brushes.White, 0, 10));
            stack.Children.Add(stashTypeRow);

            stack.Children.Add(MakeToggleRow("Ult TP", "UltPortalOn", "UltPortalOff", address));

            var timerRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 4) };
            timerRow.Children.Add(new TextBlock { Text = "Ult Timer", Width = 60, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            var timerSlider = new Slider
            {
                Minimum = 1, Maximum = 60, Value = 15, Width = 100,
                VerticalAlignment = VerticalAlignment.Center,
                TickFrequency = 1, IsSnapToTickEnabled = true,
                Tag = address
            };
            var timerLabel = new TextBlock
            {
                Text = "15s", VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0), Width = 30, Foreground = Brushes.White
            };
            timerSlider.ValueChanged += (s, args) =>
            {
                var val = (int)args.NewValue;
                timerLabel.Text = val + "s";
                var t = (string)timerSlider.Tag;
                var cmd = "SetUltTimer:" + val;

                if (SyncAllCheckbox?.IsChecked == true)
                    SendToAllRemotes(cmd);
                else
                    ThreadPool.QueueUserWorkItem(_ => HttpCommandSender.Send(t, cmd));
            };
            timerRow.Children.Add(timerSlider);
            timerRow.Children.Add(timerLabel);
            stack.Children.Add(timerRow);

            var unloaderRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            unloaderRow.Children.Add(new TextBlock { Text = "Unloader", Width = 60, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            unloaderRow.Children.Add(MakeButton("✓", "Unloader", address, Color.FromRgb(76, 175, 80), Brushes.White, 28, 12));
            stack.Children.Add(unloaderRow);

            var delayRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 4) };
            delayRow.Children.Add(new TextBlock { Text = "Unload Delay", Width = 72, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            var delaySlider = new Slider
            {
                Minimum = 1500, Maximum = 10000, Value = 2000, Width = 100,
                VerticalAlignment = VerticalAlignment.Center,
                TickFrequency = 100, IsSnapToTickEnabled = true,
                Tag = address
            };
            var delayLabel = new TextBlock
            {
                Text = "2.0s", VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(6, 0, 0, 0), Width = 36, Foreground = Brushes.White
            };
            delaySlider.ValueChanged += (s, args) =>
            {
                var val = (int)args.NewValue;
                delayLabel.Text = (val / 1000.0).ToString("0.0") + "s";
                var t = (string)delaySlider.Tag;
                var cmd = "SetUnloaderDelay:" + val;

                if (SyncAllCheckbox?.IsChecked == true)
                    SendToAllRemotes(cmd);
                else
                    ThreadPool.QueueUserWorkItem(_ => HttpCommandSender.Send(t, cmd));
            };
            delayRow.Children.Add(delaySlider);
            delayRow.Children.Add(delayLabel);
            stack.Children.Add(delayRow);

            group.Content = stack;
            BotPanelsContainer.Children.Add(group);
        }

        private StackPanel MakeToggleRow(string label, string startCmd, string stopCmd, string target)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            row.Children.Add(new TextBlock { Text = label, Width = 60, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White });
            row.Children.Add(MakeButton("✓", startCmd, target, Color.FromRgb(76, 175, 80), Brushes.White, 28, 12));
            row.Children.Add(MakeButton("✗", stopCmd, target, Color.FromRgb(244, 67, 54), Brushes.White, 28, 12));
            return row;
        }

        private TextBlock MakeSmallLabel(string text)
        {
            return new TextBlock
            {
                Text = text, FontSize = 10,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 2, 0), Foreground = Brushes.White
            };
        }

        private Button MakeButton(string text, string command, string target,
            Color? bg = null, Brush fg = null, int minWidth = 0, int fontSize = 11)
        {
            var btn = new Button
            {
                Content = text.ToUpper(), Tag = command + "|" + target,
                Margin = new Thickness(2), Padding = new Thickness(8, 4, 8, 4),
                FontSize = fontSize, FontWeight = FontWeights.Bold
            };
            if (bg.HasValue) 
            {
                if (MutedColors)
                {
                    // Map vibrant colors to muted shades
                    if (bg.Value == Color.FromRgb(76, 175, 80)) // Green -> Dark Gray
                        btn.Background = new SolidColorBrush(Color.FromRgb(64, 64, 64));
                    else if (bg.Value == Color.FromRgb(244, 67, 54)) // Red -> Darker Gray
                        btn.Background = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                    else if (bg.Value == Color.FromRgb(66, 133, 244)) // Blue -> Charcoal
                        btn.Background = new SolidColorBrush(Color.FromRgb(56, 56, 56));
                    else if (bg.Value == Color.FromRgb(255, 152, 0)) // Orange -> Dim Gray
                        btn.Background = new SolidColorBrush(Color.FromRgb(72, 72, 72));
                    else
                        btn.Background = new SolidColorBrush(bg.Value);
                }
                else
                {
                    btn.Background = new SolidColorBrush(bg.Value);
                }
                
                // For DPB style flat buttons
                btn.BorderThickness = new Thickness(0);
            }
            else
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(51, 51, 51));
                btn.BorderThickness = new Thickness(1);
                btn.BorderBrush = Brushes.Gray;
                btn.Foreground = Brushes.White;
            }
            if (fg != null) btn.Foreground = fg;
            if (minWidth > 0) btn.MinWidth = minWidth;

            btn.Click += BotCommand_Click;
            return btn;
        }

        private void BotCommand_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var tagParts = (button.Tag as string)?.Split('|');
            if (tagParts == null || tagParts.Length < 2) return;

            var command = tagParts[0];
            var target = tagParts[1];
            var syncAll = SyncAllCheckbox.IsChecked == true;

            if (syncAll)
            {
                SendToAllRemotes(command);
                UpdateStatus($"ALL: {command}");
            }
            else
            {
                ThreadPool.QueueUserWorkItem(_ => HttpCommandSender.Send(target, command));
                UpdateStatus($"{target}: {command}");
            }
        }

        private void SendToAllRemotes(string command)
        {
            if (_settings.RemoteBots == null || _settings.RemoteBots.Count == 0) return;

            var addresses = new string[_settings.RemoteBots.Count];
            _settings.RemoteBots.CopyTo(addresses, 0);

            foreach (var entry in addresses)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;
                var address = entry.Split('|')[0].Trim();
                ThreadPool.QueueUserWorkItem(_ => HttpCommandSender.Send(address, command));
            }
        }

        private void AddBot_Click(object sender, RoutedEventArgs e)
        {
            var address = NewBotAddress.Text?.Trim();
            if (string.IsNullOrEmpty(address)) return;

            var label = NewBotLabel.Text?.Trim();
            var entry = string.IsNullOrEmpty(label) ? address : $"{address}|{label}";

            if (!_settings.RemoteBots.Contains(entry))
            {
                _settings.RemoteBots.Add(entry);
                UpdateStatus($"Added: {entry}");
            }
            
            NewBotAddress.Text = "";
            NewBotLabel.Text = "";
        }

        private void BotListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BotListBox.SelectedItem is string selected)
            {
                var parts = selected.Split('|');
                NewBotAddress.Text = parts[0].Trim();
                NewBotLabel.Text = parts.Length > 1 ? parts[1].Trim() : "";
            }
        }

        private void UpdateBot_Click(object sender, RoutedEventArgs e)
        {
            var oldEntry = BotListBox.SelectedItem as string;
            if (string.IsNullOrEmpty(oldEntry)) return;

            var address = NewBotAddress.Text?.Trim();
            if (string.IsNullOrEmpty(address)) return;

            var label = NewBotLabel.Text?.Trim();
            var newEntry = string.IsNullOrEmpty(label) ? address : $"{address}|{label}";

            int index = _settings.RemoteBots.IndexOf(oldEntry);
            if (index >= 0)
            {
                _settings.RemoteBots[index] = newEntry;
                BotListBox.SelectedItem = newEntry;
                UpdateStatus($"Updated: {newEntry}");
            }
        }

        private void RemoveBot_Click(object sender, RoutedEventArgs e)
        {
            if (BotListBox.SelectedItem is string selected)
            {
                _settings.RemoteBots.Remove(selected);
                UpdateStatus($"Removed: {selected}");
            }
        }

        private void SyncAllCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            UpdateStatus(SyncAllCheckbox.IsChecked == true ? "Sync mode: ALL bots" : "Sync mode: Individual");
        }

        private void AlwaysOnTop_Changed(object sender, RoutedEventArgs e)
        {
            if (_settings == null) return;
            this.Topmost = AlwaysOnTop;
            _settings.Save();
        }

        private void MutedColors_Changed(object sender, RoutedEventArgs e)
        {
            if (_settings == null) return;
            _settings.Save();
            RebuildBotPanels(); // Redraw with new colors
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_settings == null) return;
            this.Opacity = WindowOpacity;
            _settings.Save();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateStatus(string text)
        {
            if (StatusText == null) return;
            // Dispatch UI update to main thread just in case it's called async
            Dispatcher.InvokeAsync(() => 
            {
                StatusText.Text = $"{DateTime.Now:HH:mm:ss} — {text}";
            });
        }
    }
}