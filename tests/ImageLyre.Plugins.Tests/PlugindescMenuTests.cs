using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ImageLyre.Plugins;
using ImageLyre.UI.Contracts.Workbench;
using Xunit;

namespace ImageLyre.Presentation.Tests;

/// <summary>校验 `.plugindesc` 解析与命令绑定：描述文件是菜单唯一真源，命令按名字解析。</summary>
public sealed class PlugindescMenuTests
{
    [Fact]
    public void ReaderParsesCategoriesGroupsAndCommands()
    {
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <PluginDictionary>
                    <Plugin Id="T" Header="测试" IsEnable="true">
                      <Category Header="帮助">
                        <Group Header="">
                          <Focal Header="关于" Command="ShowAboutCommand" Order="3" />
                        </Group>
                        <Group Header="更多">
                          <Focal Header="嵌套" Command="NestedCommand" />
                        </Group>
                      </Category>
                    </Plugin>
                  </PluginDictionary>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var categories = PlugindescMenuReader.Read(stream);

        var category = Assert.Single(categories);
        Assert.Equal("帮助", category.Header);
        Assert.Equal(2, category.Groups.Count);
        Assert.Equal(string.Empty, category.Groups[0].Header);
        var about = Assert.Single(category.Groups[0].Items);
        Assert.Equal("关于", about.Header);
        Assert.Equal("ShowAboutCommand", about.CommandName);
        Assert.Equal(3, about.Order);
        Assert.Equal("更多", category.Groups[1].Header);
    }

    [Fact]
    public void ReaderParsesEnableAndVisibility()
    {
        var xml = """
                  <PluginDictionary>
                    <Plugin Id="T" Header="测试" IsEnable="true">
                      <Category Header="帮助">
                        <Group Header="">
                          <Focal Header="灰显项" Command="A" Enable="false" />
                          <Focal Header="隐藏项" Command="B" Visibility="Collapsed" />
                          <Focal Header="正常项" Command="C" />
                        </Group>
                      </Category>
                    </Plugin>
                  </PluginDictionary>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        var items = PlugindescMenuReader.Read(stream).Single().Groups.Single().Items;

        Assert.False(items[0].IsEnabled);
        Assert.True(items[0].IsVisible);
        Assert.False(items[1].IsVisible);
        Assert.True(items[2].IsEnabled);
        Assert.True(items[2].IsVisible);
    }

    [Fact]
    public void ReaderSkipsDisabledPlugin()
    {
        var xml = """
                  <PluginDictionary>
                    <Plugin Id="T" Header="测试" IsEnable="false">
                      <Category Header="帮助">
                        <Group Header="">
                          <Focal Header="关于" Command="ShowAboutCommand" />
                        </Group>
                      </Category>
                    </Plugin>
                  </PluginDictionary>
                  """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));

        Assert.Empty(PlugindescMenuReader.Read(stream));
    }

    [Fact]
    public void SourceBindsCommandsByNameAndLeavesUnknownDisabled()
    {
        var commands = new Dictionary<string, ICommand>(StringComparer.Ordinal)
        {
            ["ShowAboutCommand"] = new NoopCommand(),
            ["NestedCommand"] = new NoopCommand(),
        };

        var categories = PlugindescMenuSource.Read(typeof(PlugindescMenuTests).Assembly, commands);

        var category = Assert.Single(categories);
        var items = category.Groups.SelectMany(group => group.Items).ToArray();
        Assert.Equal(3, items.Length);
        Assert.Same(commands["ShowAboutCommand"], items[0].Command);
        Assert.Null(items[1].Command);
    }

    [Fact]
    public void SourceReturnsEmptyWhenAssemblyHasNoDescriptor()
    {
        var categories = PlugindescMenuSource.Read(typeof(object).Assembly, new Dictionary<string, ICommand>());

        Assert.Empty(categories);
    }

    private sealed class NoopCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
        }
    }
}
