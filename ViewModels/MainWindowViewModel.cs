using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using Trakto.ViewModels.Layout;

namespace Trakto.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private LayoutNode _rootLayout;

    public MainWindowViewModel()
    {
        var initialLeaf = new LeafNode();
        var initialTab = new PanelContent { Title = "Main Workspace", Content = "Welcome to the Video Editor", Parent = initialLeaf };
        initialLeaf.Tabs.Add(initialTab);
        initialLeaf.SelectedTab = initialTab;
        
        WireUpLeaf(initialLeaf);
        _rootLayout = initialLeaf;
    }

    private void WireUpLeaf(LeafNode leaf)
    {
        leaf.SplitRequested = (node, orientation) => SplitLeaf(node, orientation);
        leaf.CloseRequested = (node) => CloseLeaf(node);
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
