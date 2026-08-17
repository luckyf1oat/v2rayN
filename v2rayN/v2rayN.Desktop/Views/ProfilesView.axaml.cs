using Avalonia.VisualTree;
using DialogHostAvalonia;
using v2rayN.Desktop.Common;

namespace v2rayN.Desktop.Views;

public partial class ProfilesView : ReactiveUserControl<ProfilesViewModel>
{
    private static Config _config;
    private static readonly string _tag = "ProfilesView";
    private int _dragSourceIndex = -1;
    private bool _dropBelow = false;
    private bool _isSelecting = false;
    private int _dragSelectStartIndex = -1;
    private bool _suppressSelectionChanged = false;
    private static readonly DataFormat<object> _dragFormat = DataFormat.CreateInProcessFormat<object>("v2rayN.ProfileIndex");

    public ProfilesView()
    {
        InitializeComponent();

        _config = AppManager.Instance.Config;

        menuSelectAll.Click += menuSelectAll_Click;
        btnAutofitColumnWidth.Click += BtnAutofitColumnWidth_Click;
        txtServerFilter.KeyDown += TxtServerFilter_KeyDown;
        lstProfiles.KeyDown += LstProfiles_KeyDown;
        lstProfiles.SelectionChanged += lstProfiles_SelectionChanged;
        lstProfiles.DoubleTapped += LstProfiles_DoubleTapped;
        lstProfiles.LoadingRow += LstProfiles_LoadingRow;
        lstProfiles.Sorting += LstProfiles_Sorting;
        if (_config.UiItem.EnableDragDropSort)
        {
            DragDrop.SetAllowDrop(lstProfiles, true);
            lstProfiles.AddHandler(PointerPressedEvent, LstProfiles_PointerPressed, RoutingStrategies.Tunnel, true);
            lstProfiles.AddHandler(PointerMovedEvent, LstProfiles_PointerMoved, RoutingStrategies.Bubble, true);
            lstProfiles.AddHandler(PointerReleasedEvent, LstProfiles_PointerReleased, RoutingStrategies.Bubble, true);
            lstProfiles.AddHandler(DragDrop.DragOverEvent, LstProfiles_DragOver);
            lstProfiles.AddHandler(DragDrop.DropEvent, LstProfiles_Drop);
        }

        this.WhenActivated(disposables =>
        {
            this.OneWayBind(ViewModel, vm => vm.ProfileItems, v => v.lstProfiles.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedProfile, v => v.lstProfiles.SelectedItem).DisposeWith(disposables);

            // this.OneWayBind(ViewModel, vm => vm.SubItems, v => v.lstGroup.ItemsSource).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.SelectedSub, v => v.lstGroup.SelectedItem).DisposeWith(disposables);
            this.Bind(ViewModel, vm => vm.ServerFilter, v => v.txtServerFilter.Text).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSubCmd, v => v.btnAddSub).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.EditSubCmd, v => v.btnEditSub).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.EditSubCmd, v => v.menuSubEdit).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.AddSubCmd, v => v.menuSubAdd).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.DeleteSubCmd, v => v.menuSubDelete).DisposeWith(disposables);

            //servers delete
            this.BindCommand(ViewModel, vm => vm.EditServerCmd, v => v.menuEditServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveServerCmd, v => v.menuRemoveServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveDuplicateServerCmd, v => v.menuRemoveDuplicateServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.CopyServerCmd, v => v.menuCopyServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SetDefaultServerCmd, v => v.menuSetDefaultServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.ShareServerCmd, v => v.menuShareServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.GenGroupAllServerCmd, v => v.menuGenGroupAllServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.GenGroupRegionServerCmd, v => v.menuGenGroupRegionServer).DisposeWith(disposables);

            //servers move
            //this.OneWayBind(ViewModel, vm => vm.SubItems, v => v.cmbMoveToGroup.ItemsSource).DisposeWith(disposables);
            //this.Bind(ViewModel, vm => vm.SelectedMoveToGroup, v => v.cmbMoveToGroup.SelectedItem).DisposeWith(disposables);

            this.BindCommand(ViewModel, vm => vm.MoveTopCmd, v => v.menuMoveTop).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveUpCmd, v => v.menuMoveUp).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveDownCmd, v => v.menuMoveDown).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.MoveBottomCmd, v => v.menuMoveBottom).DisposeWith(disposables);

            //servers ping
            this.BindCommand(ViewModel, vm => vm.MixedTestServerCmd, v => v.menuMixedTestServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.TcpingServerCmd, v => v.menuTcpingServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RealPingServerCmd, v => v.menuRealPingServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.UdpTestServerCmd, v => v.menuUdpTestServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SpeedServerCmd, v => v.menuSpeedServer).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.SortServerResultCmd, v => v.menuSortServerResult).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.RemoveInvalidServerResultCmd, v => v.menuRemoveInvalidServerResult).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.FastRealPingCmd, v => v.btnFastRealPing).DisposeWith(disposables);

            //servers export
            this.BindCommand(ViewModel, vm => vm.Export2ClientConfigCmd, v => v.menuExport2ClientConfig).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ClientConfigClipboardCmd, v => v.menuExport2ClientConfigClipboard).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ShareUrlCmd, v => v.menuExport2ShareUrl).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2ShareUrlBase64Cmd, v => v.menuExport2ShareUrlBase64).DisposeWith(disposables);
            this.BindCommand(ViewModel, vm => vm.Export2InnerUriCmd, v => v.menuExport2InnerUri).DisposeWith(disposables);

            ViewModel.ShowYesNoInteraction.RegisterHandler(async interaction =>
            {
                var message = interaction.Input;
                var result = await UI.ShowYesNo(message);
                interaction.SetOutput(result == ButtonResult.Yes);
            }).DisposeWith(disposables);

            ViewModel.SaveFileDialogInteraction.RegisterHandler(async interaction =>
            {
                var viewModel = ViewModel;
                if (viewModel is null)
                {
                    interaction.SetOutput(false);
                    return;
                }
                var profileItem = interaction.Input;
                var fileName = await UI.SaveFileDialog("");
                if (fileName.IsNullOrEmpty())
                {
                    interaction.SetOutput(false);
                    return;
                }
                await viewModel.Export2ClientConfigResult(fileName, profileItem);
                interaction.SetOutput(true);
            }).DisposeWith(disposables);

            ViewModel.SetClipboardDataInteraction.RegisterHandler(async interaction =>
            {
                var strData = interaction.Input;
                await AvaUtils.SetClipboardData(this, strData);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.ProfilesFocusInteraction.RegisterHandler(interaction =>
            {
                lstProfiles.Focus();
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.ShareServerInteraction.RegisterHandler(async interaction =>
            {
                var url = interaction.Input;
                if (url.IsNullOrEmpty())
                {
                    interaction.SetOutput(RxVoid.Default);
                    return;
                }
                await ShareServer(url);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.DispatcherRefreshServersBizInteraction.RegisterHandler(interaction =>
            {
                Dispatcher.UIThread.Post(RefreshServersBiz, DispatcherPriority.Default);
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            ViewModel.AdjustMainLvColWidthInteraction.RegisterHandler(interaction =>
            {
                //AutofitColumnWidth();
                interaction.SetOutput(RxVoid.Default);
            }).DisposeWith(disposables);

            AppEvents.AppExitRequested
              .AsObservable()
              .ObserveOn(RxSchedulers.MainThreadScheduler)
              .Subscribe(_ => StorageUI())
              .DisposeWith(disposables);

        });

        RestoreUI();
    }

    private async void LstProfiles_Sorting(object? sender, DataGridColumnEventArgs e)
    {
        e.Handled = true;

        if (ViewModel != null && e.Column?.Tag?.ToString() != null)
        {
            await ViewModel.SortServer(e.Column.Tag.ToString());
        }

        e.Handled = false;
    }

    #region Event

    public async Task ShareServer(string url)
    {
        if (url.IsNullOrEmpty())
        {
            return;
        }

        var dialog = new QrcodeView(url);
        await DialogHost.Show(dialog);
    }

    public void RefreshServersBiz()
    {
        if (lstProfiles.SelectedIndex >= 0)
        {
            lstProfiles.ScrollIntoView(lstProfiles.SelectedItem, null);
        }
    }

    private void lstProfiles_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionChanged)
        {
            return;
        }
        if (ViewModel != null)
        {
            ViewModel.SelectedProfiles = lstProfiles.SelectedItems.Cast<ProfileItemModel>().ToList();
        }
    }

    private void LstProfiles_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        ResetDragState();
        var source = e.Source as Border;
        if (source?.Name == "HeaderBackground")
        {
            return;
        }

        if (_config.UiItem.DoubleClick2Activate)
        {
            ViewModel?.SetDefaultServer();
        }
        else
        {
            ViewModel?.EditServerAsync();
        }
    }

    private void LstProfiles_LoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Header = $" {e.Row.Index + 1}";
    }

    //private void LstProfiles_ColumnHeader_Click(object? sender, RoutedEventArgs e)
    //{
    //    var colHeader = sender as DataGridColumnHeader;
    //    if (colHeader == null || colHeader.TabIndex < 0 || colHeader.Column == null)
    //    {
    //        return;
    //    }

    //    var colName = ((MyDGTextColumn)colHeader.Column).ExName;
    //    ViewModel?.SortServer(colName);
    //}

    private void menuSelectAll_Click(object? sender, RoutedEventArgs e)
    {
        lstProfiles.SelectAll();
    }

    private void LstProfiles_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta)
        {
            switch (e.Key)
            {
                case Key.A:
                    menuSelectAll_Click(null, null);
                    break;

                case Key.C:
                    ViewModel?.Export2ShareUrlAsync(false);
                    break;

                case Key.D:
                    ViewModel?.EditServerAsync();
                    break;

                case Key.F:
                    ViewModel?.ShareServerAsync();
                    break;

                case Key.O:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Tcping);
                    break;

                case Key.R:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Realping);
                    break;

                case Key.T:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Speedtest);
                    break;

                case Key.E:
                    ViewModel?.ServerSpeedtest(ESpeedActionType.Mixedtest);
                    break;
            }
        }
        else
        {
            switch (e.Key)
            {
                case Key.Enter:
                    //case Key.Return:
                    ViewModel?.SetDefaultServer();
                    break;

                case Key.Delete:
                case Key.Back:
                    ViewModel?.RemoveServerAsync();
                    break;

                case Key.T:
                    ViewModel?.MoveServer(EMove.Top);
                    break;

                case Key.U:
                    ViewModel?.MoveServer(EMove.Up);
                    break;

                case Key.D:
                    ViewModel?.MoveServer(EMove.Down);
                    break;

                case Key.B:
                    ViewModel?.MoveServer(EMove.Bottom);
                    break;

                case Key.Escape:
                    ViewModel?.ServerSpeedtestStop();
                    break;
            }
        }
    }

    private void BtnAutofitColumnWidth_Click(object? sender, RoutedEventArgs e)
    {
        AutofitColumnWidth();
    }

    private void AutofitColumnWidth()
    {
        try
        {
            //First scroll horizontally to the initial position to avoid the control crash bug
            if (lstProfiles.SelectedIndex >= 0)
            {
                lstProfiles.ScrollIntoView(lstProfiles.SelectedItem, lstProfiles.Columns[0]);
            }
            else
            {
                var model = lstProfiles.ItemsSource.Cast<ProfileItemModel>();
                if (model.Any())
                {
                    lstProfiles.ScrollIntoView(model.First(), lstProfiles.Columns[0]);
                }
                else
                {
                    return;
                }
            }

            foreach (var it in lstProfiles.Columns)
            {
                it.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void TxtServerFilter_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Return)
        {
            ViewModel?.RefreshServers();
        }
    }

    #endregion Event

    #region UI

    private void RestoreUI()
    {
        try
        {
            var lvColumnItem = _config.UiItem.MainColumnItem.OrderBy(t => t.Index).ToList();
            var displayIndex = 0;
            foreach (var item in lvColumnItem)
            {
                foreach (var item2 in lstProfiles.Columns)
                {
                    if (item2.Tag == null)
                    {
                        continue;
                    }
                    if (item2.Tag.Equals(item.Name))
                    {
                        if (item.Width < 0)
                        {
                            item2.IsVisible = false;
                        }
                        else
                        {
                            item2.Width = new DataGridLength(item.Width, DataGridLengthUnitType.Pixel);
                            item2.DisplayIndex = displayIndex++;
                        }
                        if (item.Name.StartsWith("to", StringComparison.CurrentCultureIgnoreCase))
                        {
                            item2.IsVisible = _config.GuiItem.EnableStatistics;
                        }
                        if (item.Name.Equals("IpInfo", StringComparison.CurrentCultureIgnoreCase))
                        {
                            item2.IsVisible = _config.SpeedTestItem.IPAPIUrl.IsNotEmpty() && !_config.UiItem.HideColumnIpInfo;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void StorageUI()
    {
        try
        {
            List<ColumnItem> lvColumnItem = [];
            foreach (var item2 in lstProfiles.Columns)
            {
                if (item2.Tag == null)
                {
                    continue;
                }
                lvColumnItem.Add(new()
                {
                    Name = (string)item2.Tag,
                    Width = (int)(item2.IsVisible == true ? item2.ActualWidth : -1),
                    Index = item2.DisplayIndex
                });
            }
            _config.UiItem.MainColumnItem = lvColumnItem;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    #endregion UI

    #region Drag and Drop

#pragma warning disable CS0618 // 拖放使用旧 API（DataObject/DoDragDrop）以兼容当前 Avalonia 版本

    private async void LstProfiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            if (!e.GetCurrentPoint(lstProfiles).Properties.IsLeftButtonPressed)
            {
                return;
            }
            var isSortModifier = e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.KeyModifiers.HasFlag(KeyModifiers.Alt);
            if (isSortModifier)
            {
                // Ctrl+Alt + 左键：拖拽排序
                var row = FindAncestor<DataGridRow>(e.Source as Control);
                if (row == null)
                {
                    return;
                }
                _dragSourceIndex = row.Index;
                // 阻止 DataGrid 进入选择逻辑，避免其状态被拖拽打断后卡死
                e.Handled = true;
                e.Pointer.Capture(lstProfiles);
                var dragData = new DataTransfer();
                var dragItem = DataTransferItem.Create(_dragFormat, _dragSourceIndex);
                dragData.Add(dragItem);
                try
                {
                    await DragDrop.DoDragDropAsync(e, dragData, DragDropEffects.Move);
                }
                finally
                {
                    _dragSourceIndex = -1;
                    e.Pointer.Capture(null);
                    dropIndicator.IsVisible = false;
                }
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                // Ctrl + 左键：DataGrid 原生多选（切换选中），不拦截
                return;
            }
            else
            {
                // 双击：不启动范围选择，避免状态残留导致编辑取消后误触发多选
                if (e.ClickCount > 1)
                {
                    ResetDragState();
                    return;
                }
                // 左键：按住拖动 = 范围多选（不捕获指针，避免干扰双击检测）
                var row = FindAncestor<DataGridRow>(e.Source as Control);
                if (row == null)
                {
                    return;
                }
                _dragSelectStartIndex = row.Index;
                _isSelecting = true;
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void LstProfiles_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                _dragSelectStartIndex = -1;
                return;
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void ResetDragState()
    {
        _dragSourceIndex = -1;
        _isSelecting = false;
        _dragSelectStartIndex = -1;
        dropIndicator.IsVisible = false;
    }

    private void LstProfiles_PointerMoved(object? sender, PointerEventArgs e)
    {
        try
        {
            if (_isSelecting)
            {
                // 按住左键拖动：范围多选
                var row = FindRowAtY(e.GetPosition(lstProfiles).Y);
                if (row != null && row.Index >= 0)
                {
                    UpdateDragSelection(row.Index);
                }
                return;
            }
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private DataGridRow? FindRowAtY(double y)
    {
        foreach (var row in lstProfiles.GetVisualDescendants().OfType<DataGridRow>())
        {
            var top = row.TranslatePoint(new Point(0, 0), lstProfiles);
            if (top.HasValue && y >= top.Value.Y && y <= top.Value.Y + row.Bounds.Height)
            {
                return row;
            }
        }
        return null;
    }

    private void UpdateDragSelection(int currentIndex)
    {
        var start = Math.Min(_dragSelectStartIndex, currentIndex);
        var end = Math.Max(_dragSelectStartIndex, currentIndex);
        _suppressSelectionChanged = true;
        lstProfiles.SelectedItems.Clear();
        for (var i = start; i <= end; i++)
        {
            var item = ViewModel?.ProfileItems.ElementAtOrDefault(i);
            if (item != null)
            {
                lstProfiles.SelectedItems.Add(item);
            }
        }
        _suppressSelectionChanged = false;
        if (ViewModel != null)
        {
            ViewModel.SelectedProfiles = lstProfiles.SelectedItems.Cast<ProfileItemModel>().ToList();
        }
    }

    private void LstProfiles_DragOver(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(_dragFormat))
        {
            e.DragEffects = DragDropEffects.None;
            dropIndicator.IsVisible = false;
            return;
        }
        e.DragEffects = DragDropEffects.Move;
        e.Handled = true;
        UpdateDropIndicator(e);
    }

    private void UpdateDropIndicator(DragEventArgs e)
    {
        try
        {
            var row = FindAncestor<DataGridRow>(e.Source as Control);
            if (row == null)
            {
                dropIndicator.IsVisible = false;
                return;
            }

            var rowTop = row.TranslatePoint(new Point(0, 0), lstProfiles) ?? new Point(0, 0);
            var mouseY = e.GetPosition(lstProfiles).Y;
            var rowHeight = row.Bounds.Height;
            var isBelow = (mouseY - rowTop.Y) > rowHeight / 2;
            var dropY = isBelow ? rowTop.Y + rowHeight : rowTop.Y;

            _dropBelow = isBelow;
            dropIndicator.Margin = new Thickness(0, dropY - 1.5, 0, 0);
            dropIndicator.IsVisible = true;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private void LstProfiles_Drop(object? sender, DragEventArgs e)
    {
        try
        {
            if (!e.DataTransfer.Contains(_dragFormat))
            {
                return;
            }

            var startIndex = -1;
            foreach (var item in e.DataTransfer.Items)
            {
                if (!item.Formats.Contains(_dragFormat))
                {
                    continue;
                }
                if (item.TryGetRaw(_dragFormat) is int idx)
                {
                    startIndex = idx;
                    break;
                }
            }
            var row = FindAncestor<DataGridRow>(e.Source as Control);
            var targetItem = row?.DataContext as ProfileItemModel;
            if (startIndex < 0 || targetItem == null || ViewModel == null)
            {
                return;
            }

            var sourceItem = ViewModel.ProfileItems.ElementAtOrDefault(startIndex);
            if (sourceItem == null || sourceItem.IndexId == targetItem.IndexId)
            {
                dropIndicator.IsVisible = false;
                return;
            }

            dropIndicator.IsVisible = false;
            _ = MoveServerAsync(startIndex, targetItem, _dropBelow);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private async Task MoveServerAsync(int startIndex, ProfileItemModel targetItem, bool insertBelow)
    {
        try
        {
            await ViewModel.MoveServerTo(startIndex, targetItem, insertBelow);
        }
        catch (Exception ex)
        {
            Logging.SaveLog(_tag, ex);
        }
    }

    private static T? FindAncestor<T>(Control? current) where T : Control
    {
        while (current != null)
        {
            if (current is T ancestor)
            {
                return ancestor;
            }
            current = current.Parent as Control;
        }
        return null;
    }

#pragma warning restore CS0618

    #endregion Drag and Drop
}
