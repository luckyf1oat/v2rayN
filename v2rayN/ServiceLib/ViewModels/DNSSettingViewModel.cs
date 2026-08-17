namespace ServiceLib.ViewModels;

public partial class DNSSettingViewModel : MyReactiveObject, ICloseable
{
    public event EventHandler? RequestClose;

    [Reactive] public partial bool UseSystemHosts { get; set; }
    [Reactive] public partial bool AddCommonHosts { get; set; }
    [Reactive] public partial bool FakeIP { get; set; }
    [Reactive] public partial string FakeIPRange { get; set; }
    [Reactive] public partial bool BlockBindingQuery { get; set; }
    [Reactive] public partial string DirectDNS { get; set; }
    [Reactive] public partial string RemoteDNS { get; set; }
    [Reactive] public partial string BootstrapDNS { get; set; }
    [Reactive] public partial string Strategy4Freedom { get; set; }
    [Reactive] public partial string Strategy4Proxy { get; set; }
    [Reactive] public partial string Strategy4ProxyDial { get; set; }
    [Reactive] public partial string Hosts { get; set; }
    [Reactive] public partial string DirectExpectedIPs { get; set; }
    [Reactive] public partial bool ParallelQuery { get; set; }
    [Reactive] public partial bool ServeStale { get; set; }
    [Reactive] public partial bool EnableHappyEyeballs { get; set; }

    [Reactive] public bool RayCustomDNSEnableCompatible { get; set; }
    [Reactive] public partial bool UseSystemHostsCompatible { get; set; }
    [Reactive] public partial string DomainStrategy4FreedomCompatible { get; set; } = string.Empty;
    [Reactive] public partial string DomainDNSAddressCompatible { get; set; } = string.Empty;
    [Reactive] public partial string NormalDNSCompatible { get; set; } = string.Empty;
    [Reactive] public partial string TunDNSCompatible { get; set; } = string.Empty;

    public bool IsSimpleDNSEnabled => !(RayCustomDNSEnableCompatible);

    public ReactiveCommand<RxVoid, RxVoid> SaveCmd { get; }
    public ReactiveCommand<RxVoid, RxVoid> ImportDefConfig4V2rayCompatibleCmd { get; }

    public DNSSettingViewModel()
    {
        _config = AppManager.Instance.Config;
        SaveCmd = ReactiveCommand.CreateFromTask(SaveSettingAsync);

        ImportDefConfig4V2rayCompatibleCmd = ReactiveCommand.CreateFromTask(async () =>
        {
            NormalDNSCompatible = EmbedUtils.GetEmbedText(Global.DNSV2rayNormalFileName);
            TunDNSCompatible = EmbedUtils.GetEmbedText(Global.DNSV2rayNormalFileName);
            await Task.CompletedTask;
        });

        this.WhenAnyValue(x => x.RayCustomDNSEnableCompatible)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSimpleDNSEnabled)));

        _ = Init();
    }

    private async Task Init()
    {
        _config = AppManager.Instance.Config;
        var item = _config.SimpleDNSItem;
        UseSystemHosts = item.UseSystemHosts ?? false;
        AddCommonHosts = item.AddCommonHosts ?? false;
        FakeIP = item.FakeIP ?? false;
        FakeIPRange = item.FakeIPRange ?? string.Empty;
        BlockBindingQuery = item.BlockBindingQuery ?? false;
        DirectDNS = item.DirectDNS ?? string.Empty;
        RemoteDNS = item.RemoteDNS ?? string.Empty;
        BootstrapDNS = item.BootstrapDNS ?? string.Empty;
        Strategy4Freedom = item.Strategy4Freedom ?? string.Empty;
        Strategy4Proxy = item.Strategy4Proxy ?? string.Empty;
        Strategy4ProxyDial = item.Strategy4ProxyDial ?? string.Empty;
        Hosts = item.Hosts ?? string.Empty;
        DirectExpectedIPs = item.DirectExpectedIPs ?? string.Empty;
        ParallelQuery = item.ParallelQuery ?? false;
        ServeStale = item.ServeStale ?? false;
        EnableHappyEyeballs = item.EnableHappyEyeballs ?? false;
        var item1 = await AppManager.Instance.GetDNSItem(ECoreType.Xray);
        RayCustomDNSEnableCompatible = item1.Enabled;
        UseSystemHostsCompatible = item1.UseSystemHosts;
        DomainStrategy4FreedomCompatible = item1?.DomainStrategy4Freedom ?? string.Empty;
        DomainDNSAddressCompatible = item1?.DomainDNSAddress ?? string.Empty;
        NormalDNSCompatible = item1?.NormalDNS ?? string.Empty;
    }

    private async Task SaveSettingAsync()
    {
        _config.SimpleDNSItem.UseSystemHosts = UseSystemHosts;
        _config.SimpleDNSItem.AddCommonHosts = AddCommonHosts;
        _config.SimpleDNSItem.FakeIP = FakeIP;
        _config.SimpleDNSItem.FakeIPRange = FakeIPRange;
        _config.SimpleDNSItem.BlockBindingQuery = BlockBindingQuery;
        _config.SimpleDNSItem.DirectDNS = DirectDNS;
        _config.SimpleDNSItem.RemoteDNS = RemoteDNS;
        _config.SimpleDNSItem.BootstrapDNS = BootstrapDNS;
        _config.SimpleDNSItem.Strategy4Freedom = Strategy4Freedom;
        _config.SimpleDNSItem.Strategy4Proxy = Strategy4Proxy;
        _config.SimpleDNSItem.Strategy4ProxyDial = Strategy4ProxyDial;
        _config.SimpleDNSItem.Hosts = Hosts;
        _config.SimpleDNSItem.DirectExpectedIPs = DirectExpectedIPs;
        _config.SimpleDNSItem.ParallelQuery = ParallelQuery;
        _config.SimpleDNSItem.ServeStale = ServeStale;
        _config.SimpleDNSItem.EnableHappyEyeballs = EnableHappyEyeballs;
        if (NormalDNSCompatible.IsNotEmpty())
        {
            var obj = JsonUtils.ParseJson(NormalDNSCompatible);
            if (obj != null && obj["servers"] != null)
            {
            }
            else
            {
                if (NormalDNSCompatible.Contains('{') || NormalDNSCompatible.Contains('}'))
                {
                    NoticeManager.Instance.Enqueue(ResUI.FillCorrectDNSText);
                    return;
                }
            }
        }

        var item1 = await AppManager.Instance.GetDNSItem(ECoreType.Xray);
        item1.Enabled = RayCustomDNSEnableCompatible;
        item1.DomainStrategy4Freedom = DomainStrategy4FreedomCompatible;
        item1.DomainDNSAddress = DomainDNSAddressCompatible;
        item1.UseSystemHosts = UseSystemHostsCompatible;
        item1.NormalDNS = NormalDNSCompatible;
        item1.TunDNS = TunDNSCompatible;
        await ConfigHandler.SaveDNSItems(_config, item1);

        await ConfigHandler.SaveConfig(_config);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
