"""由设计基线生成主题令牌字典。设计值来自 color-tokens.md 1.3、typography.md 2.0、spacing.md 1.0。"""
import io
import os

OUT = r'src\Libs\ImageLyre.Themes'

LIGHT = [
    ('Lyre.Surface.Window', '#F5F6F8'),
    ('Lyre.Surface.Panel', '#FFFFFF'),
    ('Lyre.Surface.Canvas', '#E8EAED'),
    ('Lyre.Surface.Input', '#FFFFFF'),
    ('Lyre.Surface.Hover', '#EAEDF0'),
    ('Lyre.Line.Divider', '#DFE2E6'),
    ('Lyre.Line.Border', '#C2C7CD'),
    ('Lyre.Line.Focus', '#B83A24'),
    ('Lyre.Text.T1', '#141619'),
    ('Lyre.Text.T2', '#4C5259'),
    ('Lyre.Text.T3', '#6A717A'),
    ('Lyre.Text.Disabled', '#7B828A'),
    ('Lyre.Signal.Active', '#B83A24'),
    ('Lyre.Signal.Running', '#B83A24'),
    ('Lyre.State.Selected.Bg', '#E2E6EA'),
    ('Lyre.State.Disabled.Bg', '#F1F3F5'),
    ('Lyre.Signal.Warning.Background', '#F6E7E5'),
    ('Lyre.Signal.Warning.Line', '#B83A24'),
]

DARK = [
    ('Lyre.Surface.Window', '#15171A'),
    ('Lyre.Surface.Panel', '#1C1F23'),
    ('Lyre.Surface.Canvas', '#0D0F11'),
    ('Lyre.Surface.Input', '#121417'),
    ('Lyre.Surface.Hover', '#292E34'),
    ('Lyre.Line.Divider', '#2A2E33'),
    ('Lyre.Line.Border', '#3C4249'),
    ('Lyre.Line.Focus', '#D89A4A'),
    ('Lyre.Text.T1', '#E9ECEF'),
    ('Lyre.Text.T2', '#B5BBC2'),
    ('Lyre.Text.T3', '#8C939B'),
    ('Lyre.Text.Disabled', '#7E858D'),
    ('Lyre.Signal.Active', '#D89A4A'),
    ('Lyre.Signal.Running', '#D89A4A'),
    ('Lyre.State.Selected.Bg', '#2E333A'),
    ('Lyre.State.Disabled.Bg', '#1A1D21'),
    ('Lyre.Signal.Warning.Background', '#332E28'),
    ('Lyre.Signal.Warning.Line', '#D89A4A'),
]

HEAD = '''<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
'''

TAIL = '</ResourceDictionary>\n'


def colors(rows):
    return ''.join('    <Color x:Key="%s">%s</Color>\n' % (k, v) for k, v in rows)


io.open(os.path.join(OUT, 'Tokens.Light.axaml'), 'w', encoding='utf-8', newline='\n').write(
    HEAD + '    <!-- 亮 30：值取自 color-tokens.md 1.3，键名由 ADR-0009 §2.1 规定 -->\n' + colors(LIGHT) + TAIL)

io.open(os.path.join(OUT, 'Tokens.Dark.axaml'), 'w', encoding='utf-8', newline='\n').write(
    HEAD + '    <!-- 暗 31：与亮色一一对应，只换色值 -->\n' + colors(DARK) + TAIL)

SHARED = '''<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- 与主题无关的令牌：画布叠加、排印、间距与线宽 -->
    <Color x:Key="Lyre.Canvas.Ink">#F5F7FA</Color>
    <Color x:Key="Lyre.Canvas.Edge">#0B0D0F</Color>

    <FontFamily x:Key="Lyre.Type.Family.Ui">Segoe UI Variable Text, Segoe UI, Microsoft YaHei UI, PingFang SC, Noto Sans CJK SC</FontFamily>
    <FontFamily x:Key="Lyre.Type.Family.Mono">Cascadia Mono, Consolas, Microsoft YaHei UI</FontFamily>

    <x:Double x:Key="Lyre.Type.Size.S11">11</x:Double>
    <x:Double x:Key="Lyre.Type.Size.S13">13</x:Double>
    <x:Double x:Key="Lyre.Type.Size.S16">16</x:Double>
    <x:Double x:Key="Lyre.Type.Size.S20">20</x:Double>

    <x:Double x:Key="Lyre.Type.LineHeight.L11">16</x:Double>
    <x:Double x:Key="Lyre.Type.LineHeight.L13">20</x:Double>
    <x:Double x:Key="Lyre.Type.LineHeight.L16">24</x:Double>
    <x:Double x:Key="Lyre.Type.LineHeight.L20">28</x:Double>

    <x:Double x:Key="Lyre.Type.LetterSpacing.Tight">-0.2</x:Double>
    <x:Double x:Key="Lyre.Type.LetterSpacing.Normal">0</x:Double>
    <x:Double x:Key="Lyre.Type.LetterSpacing.Caps">1.1</x:Double>

    <x:String x:Key="Lyre.Type.Weight.Regular">Normal</x:String>
    <x:String x:Key="Lyre.Type.Weight.SemiBold">SemiBold</x:String>

    <x:Double x:Key="Lyre.Space.1">4</x:Double>
    <x:Double x:Key="Lyre.Space.2">8</x:Double>
    <x:Double x:Key="Lyre.Space.3">12</x:Double>
    <x:Double x:Key="Lyre.Space.4">16</x:Double>
    <x:Double x:Key="Lyre.Space.5">24</x:Double>
    <x:Double x:Key="Lyre.Space.6">32</x:Double>

    <x:Double x:Key="Lyre.Size.Hairline">1</x:Double>
    <x:Double x:Key="Lyre.Size.Signal">2</x:Double>
</ResourceDictionary>
'''
io.open(os.path.join(OUT, 'Tokens.Shared.axaml'), 'w', encoding='utf-8', newline='\n').write(SHARED)

PARENT = '''<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <!-- 唯一入口：外观模块挂载它，控件只引用 Lyre.* -->
    <ResourceDictionary.MergedDictionaries>
        <ResourceInclude Source="avares://ImageLyre.Themes/Tokens.Shared.axaml" />
    </ResourceDictionary.MergedDictionaries>
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Light">
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="avares://ImageLyre.Themes/Tokens.Light.axaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
        <ResourceDictionary x:Key="Dark">
            <ResourceDictionary.MergedDictionaries>
                <ResourceInclude Source="avares://ImageLyre.Themes/Tokens.Dark.axaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
'''
io.open(os.path.join(OUT, 'Tokens.axaml'), 'w', encoding='utf-8', newline='\n').write(PARENT)
print('generated')
