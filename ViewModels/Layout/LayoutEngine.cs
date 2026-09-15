using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Layout;

namespace Trakto.ViewModels.Layout;

public abstract partial class LayoutNode : ObservableObject
{
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

    public bool IsHorizontal => Orientation == Orientation.Horizontal;
    public bool IsVertical => Orientation == Orientation.Vertical;

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
    private object? _content;

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

    public Action<LeafNode, Orientation>? SplitRequested { get; set; }
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
            SelectedTab.Content = $"This is the {viewType} view.";
        }
    }
}
