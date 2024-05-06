using ImageLyre.Contracts.Documents;
using System.ComponentModel;

namespace ImageLyre.UI.Contracts.Documents;
/// <summary>
/// 描述一个可被窗体管理的承载主业务内容的可停靠子窗体（如实验记录、数据表、图像视图等）
/// </summary>
public interface IDocumentViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// 标题栏显示的标题文本
    /// </summary>
    string Title { get; set; }
}
