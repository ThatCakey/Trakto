using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Trakto.ViewModels.Layout;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;

namespace Trakto.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private LayoutNode _rootLayout;

    public RecentLayoutsManager RecentManager { get; } = new();
    
    public ObservableCollection<string> RecentLayouts { get; } = new();

    public MainWindowViewModel()
    {
        var initialLeaf = new LeafNode();
        var initialTab = new PanelContent { Title = "Main Workspace", Content = "Welcome to the Video Editor", Parent = initialLeaf };
        initialLeaf.Tabs.Add(initialTab);
        initialLeaf.SelectedTab = initialTab;
        
        WireUpLeaf(initialLeaf);
        _rootLayout = initialLeaf;
        
        UpdateRecentLayouts();
    }
    
    private void UpdateRecentLayouts()
    {
        RecentLayouts.Clear();
        foreach (var path in RecentManager.RecentPaths)
        {
            RecentLayouts.Add(path);
        }
    }

    private void WireUpLeaf(LeafNode leaf)
    {
        leaf.SplitRequested = (node, orientation) => SplitLeaf(node, orientation);
        leaf.CloseRequested = (node) => CloseLeaf(node);
        
        foreach (var tab in leaf.Tabs)
        {
            tab.Parent = leaf;
            tab.ResolveView();
        }
    }

    private void WireUpTree(LayoutNode node, SplitNode? parent)
    {
        node.Parent = parent;
        if (node is SplitNode splitNode)
        {
            WireUpTree(splitNode.FirstChild, splitNode);
            WireUpTree(splitNode.SecondChild, splitNode);
        }
        else if (node is LeafNode leafNode)
        {
            WireUpLeaf(leafNode);
        }
    }

    private IStorageProvider? GetStorageProvider()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.StorageProvider;
        }
        return null;
    }

    [RelayCommand]
    public async Task SaveLayout()
    {
        var storage = GetStorageProvider();
        if (storage == null) return;

        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Layout",
            DefaultExtension = "json",
            FileTypeChoices = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
        });

        if (file != null)
        {
            await LayoutSerializer.SaveAsync(RootLayout, file.Path.LocalPath);
            RecentManager.Add(file.Path.LocalPath);
            UpdateRecentLayouts();
        }
    }

    [RelayCommand]
    public async Task LoadLayout()
    {
        var storage = GetStorageProvider();
        if (storage == null) return;

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Layout",
            AllowMultiple = false,
            FileTypeFilter = new[] { new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } } }
        });

        if (files.Count > 0)
        {
            await LoadRecentLayout(files[0].Path.LocalPath);
        }
    }

    [RelayCommand]
    public async Task LoadRecentLayout(string path)
    {
        var loadedTree = await LayoutSerializer.LoadAsync(path);
        if (loadedTree != null)
        {
            WireUpTree(loadedTree, null);
            RootLayout = loadedTree;
            RecentManager.Add(path);
            UpdateRecentLayouts();
        }
    }

    private void SplitLeaf(LeafNode target, Orientation orientation)
    {
        var oldParent = target.Parent;
        
        var newLeaf = new LeafNode();
        var newTab = new PanelContent { Title = "New Panel", Content = "Empty", Parent = newLeaf };
        newLeaf.Tabs.Add(newTab);
        newLeaf.SelectedTab = newTab;
        WireUpLeaf(newLeaf);

        var splitNode = new SplitNode(target, newLeaf, orientation);

        if (oldParent == null)
        {
            RootLayout = splitNode;
        }
        else
        {
            oldParent.ReplaceChild(target, splitNode);
        }
    }

    private void CloseLeaf(LeafNode target)
    {
        if (target.Parent == null)
        {
            // Cannot close the last remaining node
            return;
        }

        var parent = target.Parent;
        var sibling = parent.FirstChild == target ? parent.SecondChild : parent.FirstChild;

        if (parent.Parent == null)
        {
            RootLayout = sibling;
            sibling.Parent = null;
        }
        else
        {
            parent.Parent.ReplaceChild(parent, sibling);
        }
    }
}
