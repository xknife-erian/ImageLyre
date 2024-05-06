using System.Collections.Generic;
using System.Windows.Input;

namespace ImageLyre.UI.Controls.Win;

/// <summary>
///     菜单项绑定实体
/// </summary>
public class MenuItemModel
{
    public MenuItemModel()
    {
        SubItems = new List<MenuItemModel>();
    }

    public string Header { get; set; }
    public string Key { get; set; }
    public List<MenuItemModel> SubItems { get; set; }
    public ICommand Command { get; set; }
    public bool IsChecked { get; set; }
    public bool IsCheckable { get; set; }
}