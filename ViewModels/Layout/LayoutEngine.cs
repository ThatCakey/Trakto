using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Layout;
using Avalonia.Controls;
using System.Text.Json.Serialization;

namespace Trakto.ViewModels.Layout;

[JsonDerivedType(typeof(SplitNode), typeDiscriminator: "split")]
[JsonDerivedType(typeof(LeafNode), typeDiscriminator: "leaf")]
public abstract partial class LayoutNode : ObservableObject
{
    [JsonIgnore]
    public SplitNode? Parent { get; set; }
}

public partial class SplitNode : LayoutNode
{
    [ObservableProperty]
    private LayoutNode _firstChild;

    [ObservableProperty]
    private LayoutNode _secondChild;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsHorizontal))]
    [NotifyPropertyChangedFor(nameof(IsVertical))]
    private Orientation _orientation;

    [ObservableProperty]
    [property: JsonIgnore]
    private GridLength _firstSize = new GridLength(1, GridUnitType.Star);

    [ObservableProperty]
    [property: JsonIgnore]
    private GridLength _secondSize = new GridLength(1, GridUnitType.Star);

    public string FirstSizeString
    {
        get => FirstSize.ToString();
        set => FirstSize = GridLength.Parse(value);
    }

    public string SecondSizeString
    {
        get => SecondSize.ToString();
        set => SecondSize = GridLength.Parse(value);
    }

    public bool IsHorizontal => Orientation == Orientation.Horizontal;
    public bool IsVertical => Orientation == Orientation.Vertical;

    public SplitNode()
    {
        // Parameterless constructor for JSON deserialization
    }

    public SplitNode(LayoutNode first, LayoutNode second, Orientation orientation)
    {
        _firstChild = first;
        _secondChild = second;
        _orientation = orientation;

        _firstChild.Parent = this;
        _secondChild.Parent = this;
    }

    public void ReplaceChild(LayoutNode oldChild, LayoutNode newChild)
    {
        if (FirstChild == oldChild)
        {
            FirstChild = newChild;
            newChild.Parent = this;
        }
        else if (SecondChild == oldChild)
        {
            SecondChild = newChild;
            newChild.Parent = this;
        }
    }
}

public partial class PanelContent : ObservableObject
{
    [ObservableProperty]
    private string _title = "New Tab";

    [ObservableProperty]
    private string _viewType = "Empty";

    [ObservableProperty]
    [property: JsonIgnore]
    private object? _content;

    public void ResolveView()
    {
        string actualType = ViewType == "Empty" ? Title : ViewType;
        
        if (actualType == "Preview") 
        {
            Content = new Trakto.Views.PreviewView();
        }
        else if (actualType == "Timeline")
        {
            Content = new Trakto.Views.TimelineView();
        }
        else 
        {
            Content = $"This is the {actualType} view.";
        }
    }

    [JsonIgnore]
    public LeafNode? Parent { get; set; }

    [RelayCommand]
    public void Close()
    {
        Parent?.CloseTab(this);
    }
}

public partial class LeafNode : LayoutNode
{
    [ObservableProperty]
    private ObservableCollection<PanelContent> _tabs = new();

    [ObservableProperty]
    private PanelContent? _selectedTab;

    [JsonIgnore]
    public Action<LeafNode, Orientation>? SplitRequested { get; set; }
    
    [JsonIgnore]
    public Action<LeafNode>? CloseRequested { get; set; }

    [RelayCommand]
    public void SplitHorizontal()
    {
        Console.WriteLine($"[SplitHorizontal] clicked");
        SplitRequested?.Invoke(this, Orientation.Horizontal);
    }
    
    [RelayCommand]
    public void SplitVertical()
    {
        Console.WriteLine($"[SplitVertical] clicked");
        SplitRequested?.Invoke(this, Orientation.Vertical);
    }
    
    [RelayCommand]
    public void Close()
    {
        Console.WriteLine($"[Close] clicked");
        CloseRequested?.Invoke(this);
    }

    [RelayCommand]
    public void AddTab()
    {
        var tab = new PanelContent { Title = "New Tab", Content = "Empty Tab", Parent = this };
        Tabs.Add(tab);
        SelectedTab = tab;
    }

    public void CloseTab(PanelContent tab)
    {
        Tabs.Remove(tab);
        if (Tabs.Count == 0)
        {
            Close();
        }
    }

    [RelayCommand]
    public void SetView(string viewType)
    {
        if (SelectedTab != null)
        {
            SelectedTab.Title = viewType;
            SelectedTab.ViewType = viewType;
            SelectedTab.ResolveView();
        }
    }
}
