namespace ImageLyre.UI.Contracts.Keybinding
{
    /// <summary>
    /// 表示一个键盘按键。
    /// 覆盖常用字符键、数字键、功能键和导航键。
    /// 不包含小键盘专用键（如 NumPad0），如有需要可后续扩展。
    /// </summary>
    public enum KeyCode
    {
        /// <summary>无按键（用于默认值或无效状态）。</summary>
        None = 0,

        /// <summary>字母键 A。</summary>
        A, /// <summary>字母键 B。</summary>
        B, /// <summary>字母键 C。</summary>
        C, /// <summary>字母键 D。</summary>
        D, /// <summary>字母键 E。</summary>
        E, /// <summary>字母键 F。</summary>
        F, /// <summary>字母键 G。</summary>
        G, /// <summary>字母键 H。</summary>
        H, /// <summary>字母键 I。</summary>
        I, /// <summary>字母键 J。</summary>
        J,
        /// <summary>字母键 K。</summary>
        K, /// <summary>字母键 L。</summary>
        L, /// <summary>字母键 M。</summary>
        M, /// <summary>字母键 N。</summary>
        N, /// <summary>字母键 O。</summary>
        O, /// <summary>字母键 P。</summary>
        P, /// <summary>字母键 Q。</summary>
        Q, /// <summary>字母键 R。</summary>
        R, /// <summary>字母键 S。</summary>
        S, /// <summary>字母键 T。</summary>
        T,
        /// <summary>字母键 U。</summary>
        U, /// <summary>字母键 V。</summary>
        V, /// <summary>字母键 W。</summary>
        W, /// <summary>字母键 X。</summary>
        X, /// <summary>字母键 Y。</summary>
        Y, /// <summary>字母键 Z。</summary>
        Z,

        /// <summary>主键盘数字键 0。</summary>
        D0, /// <summary>主键盘数字键 1。</summary>
        D1, /// <summary>主键盘数字键 2。</summary>
        D2, /// <summary>主键盘数字键 3。</summary>
        D3, /// <summary>主键盘数字键 4。</summary>
        D4, /// <summary>主键盘数字键 5。</summary>
        D5, /// <summary>主键盘数字键 6。</summary>
        D6, /// <summary>主键盘数字键 7。</summary>
        D7, /// <summary>主键盘数字键 8。</summary>
        D8, /// <summary>主键盘数字键 9。</summary>
        D9,

        /// <summary>功能键 F1。</summary>
        F1, /// <summary>功能键 F2。</summary>
        F2, /// <summary>功能键 F3。</summary>
        F3, /// <summary>功能键 F4。</summary>
        F4, /// <summary>功能键 F5。</summary>
        F5, /// <summary>功能键 F6。</summary>
        F6, /// <summary>功能键 F7。</summary>
        F7, /// <summary>功能键 F8。</summary>
        F8, /// <summary>功能键 F9。</summary>
        F9, /// <summary>功能键 F10。</summary>
        F10, /// <summary>功能键 F11。</summary>
        F11, /// <summary>功能键 F12。</summary>
        F12,

        /// <summary>回车键。</summary>
        Enter,
        /// <summary>Esc 键。</summary>
        Escape,
        /// <summary>空格键。</summary>
        Space,
        /// <summary>Tab 键。</summary>
        Tab,
        /// <summary>退格键。</summary>
        Backspace,
        /// <summary>Delete 键。</summary>
        Delete,
        /// <summary>Insert 键。</summary>
        Insert,
        /// <summary>Home 键。</summary>
        Home,
        /// <summary>End 键。</summary>
        End,
        /// <summary>PageUp 键。</summary>
        PageUp,
        /// <summary>PageDown 键。</summary>
        PageDown,

        /// <summary>方向键：上。</summary>
        Up,
        /// <summary>方向键：下。</summary>
        Down,
        /// <summary>方向键：左。</summary>
        Left,
        /// <summary>方向键：右。</summary>
        Right,

        /// <summary>逗号键（,）。</summary>
        Comma,
        /// <summary>句号键（.）。</summary>
        Period,
        /// <summary>斜杠键（/）。</summary>
        Slash,
        /// <summary>分号键（;）。</summary>
        Semicolon,
        /// <summary>单引号键（'）。</summary>
        Quote,
        /// <summary>反引号键（`）。</summary>
        Backquote,
        /// <summary>左方括号键（[）。</summary>
        BracketLeft,
        /// <summary>右方括号键（]）。</summary>
        BracketRight,
        /// <summary>反斜杠键（\）。</summary>
        Backslash,
        /// <summary>减号键（-）。</summary>
        Minus,
        /// <summary>等号键（=）。</summary>
        Equal,
        /// <summary>加号键（+，通常为 Shift+=，保留语义）。</summary>
        Plus,
        /// <summary>下划线键（_，通常为 Shift+-）。</summary>
        Underscore,

        /// <summary>小键盘数字键 0。</summary>
        NumPad0, /// <summary>小键盘数字键 1。</summary>
        NumPad1, /// <summary>小键盘数字键 2。</summary>
        NumPad2, /// <summary>小键盘数字键 3。</summary>
        NumPad3, /// <summary>小键盘数字键 4。</summary>
        NumPad4,
        /// <summary>小键盘数字键 5。</summary>
        NumPad5, /// <summary>小键盘数字键 6。</summary>
        NumPad6, /// <summary>小键盘数字键 7。</summary>
        NumPad7, /// <summary>小键盘数字键 8。</summary>
        NumPad8, /// <summary>小键盘数字键 9。</summary>
        NumPad9,
        /// <summary>小键盘回车键。</summary>
        NumPadEnter,
        /// <summary>小键盘加号键（+）。</summary>
        NumPadAdd,
        /// <summary>小键盘减号键（-）。</summary>
        NumPadSubtract,
        /// <summary>小键盘乘号键（*）。</summary>
        NumPadMultiply,
        /// <summary>小键盘除号键（/）。</summary>
        NumPadDivide,
        /// <summary>小键盘小数点键（.）。</summary>
        NumPadDecimal
    }

    /// <summary>
    /// 表示键盘修饰键（组合键中的控制键）。
    /// 使用 Flags 特性支持多选（如 Ctrl+Shift+A）。
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        /// <summary>
        /// 无修饰键。
        /// </summary>
        None = 0,

        /// <summary>
        /// Control 键（Windows/Linux 中为 Ctrl，macOS 中通常映射为 Cmd，由实现层处理，允许差异）。
        /// </summary>
        Control = 1 << 0,

        /// <summary>
        /// Shift 键。
        /// </summary>
        Shift = 1 << 1,

        /// <summary>
        /// Alt 键（macOS 中通常为 Option）。
        /// </summary>
        Alt = 1 << 2,

        /// <summary>
        /// Windows 键（或 macOS 的 Command 键）。
        /// 注意：某些系统组合（如 Win+L）被操作系统保留，无法拦截。
        /// </summary>
        Windows = 1 << 3,

        /// <summary>
        /// 所有修饰键的组合（用于掩码或全匹配）。
        /// </summary>
        All = Control | Shift | Alt | Windows
    }

    /// <summary>
    /// 表示一个完整的键盘快捷键组合（如 Ctrl+Shift+S）。
    /// 该结构是不可变的（immutable），线程安全，且支持相等性比较和哈希。
    /// </summary>
    public readonly struct KeyCombination : IEquatable<KeyCombination>
    {
        /// <summary>
        /// 获取组合中的主按键（必须为非 None）。
        /// </summary>
        public KeyCode Key { get; }

        /// <summary>
        /// 获取组合中激活的修饰键（可为 None）。
        /// </summary>
        public ModifierKeys Modifiers { get; }

        /// <summary>
        /// 初始化一个新的快捷键组合。
        /// </summary>
        /// <param name="key">主按键，不能为 <see cref="KeyCode.None"/>。</param>
        /// <param name="modifiers">修饰键组合，默认为 <see cref="ModifierKeys.None"/>。</param>
        /// <exception cref="ArgumentException">当 key 为 None 时抛出。</exception>
        public KeyCombination(KeyCode key, ModifierKeys modifiers = ModifierKeys.None)
        {
            if (key == KeyCode.None)
                throw new ArgumentException("主按键不能为 None。", nameof(key));

            Key = key;
            Modifiers = modifiers;
        }

        /// <summary>
        /// 判断两个快捷键组合是否完全相同（按键 + 修饰键均一致）。
        /// </summary>
        /// <param name="other">用于比较的另一实例</param>
        public bool Equals(KeyCombination other)
        {
            return Key == other.Key && Modifiers == other.Modifiers;
        }

        /// <summary>
        /// 判断此快捷键组合是否等于指定对象。
        /// </summary>
        /// <param name="obj">用于比较的对象</param>
        public override bool Equals(object? obj)
        {
            return obj is KeyCombination other && Equals(other);
        }

        /// <summary>
        /// 获取此快捷键组合的哈希码。
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Key * 397) ^ (int)Modifiers;
            }
        }

        /// <summary>
        /// 返回此快捷键组合的人类可读字符串表示（如 "Ctrl+Shift+S"）。
        /// 本地化显示逻辑可由 UI 层处理，此处使用英文通用缩写。
        /// </summary>
        public override string ToString()
        {
            var parts = new System.Text.StringBuilder();

            // 按照常见顺序：Ctrl → Alt → Shift → Win
            if (Modifiers.HasFlag(ModifierKeys.Control))
                parts.Append("Ctrl+");
            if (Modifiers.HasFlag(ModifierKeys.Alt))
                parts.Append("Alt+");
            if (Modifiers.HasFlag(ModifierKeys.Shift))
                parts.Append("Shift+");
            if (Modifiers.HasFlag(ModifierKeys.Windows))
                parts.Append("Win+");

            // 主按键名称
            parts.Append(Key switch
            {
                KeyCode.D0 => "0",
                KeyCode.D1 => "1",
                KeyCode.D2 => "2",
                KeyCode.D3 => "3",
                KeyCode.D4 => "4",
                KeyCode.D5 => "5",
                KeyCode.D6 => "6",
                KeyCode.D7 => "7",
                KeyCode.D8 => "8",
                KeyCode.D9 => "9",

                KeyCode.A => "A",
                KeyCode.B => "B",
                KeyCode.C => "C",
                KeyCode.D => "D",
                KeyCode.E => "E",
                KeyCode.F => "F",
                KeyCode.G => "G",
                KeyCode.H => "H",
                KeyCode.I => "I",
                KeyCode.J => "J",
                KeyCode.K => "K",
                KeyCode.L => "L",
                KeyCode.M => "M",
                KeyCode.N => "N",
                KeyCode.O => "O",
                KeyCode.P => "P",
                KeyCode.Q => "Q",
                KeyCode.R => "R",
                KeyCode.S => "S",
                KeyCode.T => "T",
                KeyCode.U => "U",
                KeyCode.V => "V",
                KeyCode.W => "W",
                KeyCode.X => "X",
                KeyCode.Y => "Y",
                KeyCode.Z => "Z",

                KeyCode.F1 => "F1",
                KeyCode.F2 => "F2",
                KeyCode.F3 => "F3",
                KeyCode.F4 => "F4",
                KeyCode.F5 => "F5",
                KeyCode.F6 => "F6",
                KeyCode.F7 => "F7",
                KeyCode.F8 => "F8",
                KeyCode.F9 => "F9",
                KeyCode.F10 => "F10",
                KeyCode.F11 => "F11",
                KeyCode.F12 => "F12",

                KeyCode.Enter => "Enter",
                KeyCode.Escape => "Esc",
                KeyCode.Space => "Space",
                KeyCode.Tab => "Tab",
                KeyCode.Backspace => "Backspace",
                KeyCode.Delete => "Del",
                KeyCode.Insert => "Ins",
                KeyCode.Home => "Home",
                KeyCode.End => "End",
                KeyCode.PageUp => "PgUp",
                KeyCode.PageDown => "PgDn",

                KeyCode.Up => "↑",
                KeyCode.Down => "↓",
                KeyCode.Left => "←",
                KeyCode.Right => "→",

                KeyCode.Comma => ",",
                KeyCode.Period => ".",
                KeyCode.Slash => "/",
                KeyCode.Semicolon => ";",
                KeyCode.Quote => "'",
                KeyCode.Backquote => "`",
                KeyCode.BracketLeft => "[",
                KeyCode.BracketRight => "]",
                KeyCode.Backslash => "\\",
                KeyCode.Minus => "-",
                KeyCode.Equal => "=",
                KeyCode.Plus => "+",
                KeyCode.Underscore => "_",

                KeyCode.NumPad0 => "Num0",
                KeyCode.NumPad1 => "Num1",
                KeyCode.NumPad2 => "Num2",
                KeyCode.NumPad3 => "Num3",
                KeyCode.NumPad4 => "Num4",
                KeyCode.NumPad5 => "Num5",
                KeyCode.NumPad6 => "Num6",
                KeyCode.NumPad7 => "Num7",
                KeyCode.NumPad8 => "Num8",
                KeyCode.NumPad9 => "Num9",
                KeyCode.NumPadEnter => "NumEnter",
                KeyCode.NumPadAdd => "+",
                KeyCode.NumPadSubtract => "-",
                KeyCode.NumPadMultiply => "*",
                KeyCode.NumPadDivide => "/",
                KeyCode.NumPadDecimal => ".",

                _ => Key.ToString()
            });

            return parts.ToString();
        }

        /// <summary>
        /// ToString 输出的全部键名 → KeyCode 映射（不区分大小写）。
        /// 注意两点歧义：
        /// 1) "+" 同时是分隔符，任何段内都不会出现 "+"，故 KeyCode.Plus / NumPadAdd 不在此表，
        ///    只能经枚举名（"Plus" / "NumPadAdd"）解析，其 ToString 输出 "+" 无法往返（结构性歧义）；
        /// 2) 符号键与小键盘符号键输出相同（"-" 来自 Minus 或 NumPadSubtract 等），
        ///    解析按主键盘版本优先，小键盘版本经枚举名可达。
        /// </summary>
        private static readonly Dictionary<string, KeyCode> KeyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            // 数字键（主键盘）
            ["0"] = KeyCode.D0, ["1"] = KeyCode.D1, ["2"] = KeyCode.D2, ["3"] = KeyCode.D3,
            ["4"] = KeyCode.D4, ["5"] = KeyCode.D5, ["6"] = KeyCode.D6, ["7"] = KeyCode.D7,
            ["8"] = KeyCode.D8, ["9"] = KeyCode.D9,
            // 字母键
            ["A"] = KeyCode.A, ["B"] = KeyCode.B, ["C"] = KeyCode.C, ["D"] = KeyCode.D,
            ["E"] = KeyCode.E, ["F"] = KeyCode.F, ["G"] = KeyCode.G, ["H"] = KeyCode.H,
            ["I"] = KeyCode.I, ["J"] = KeyCode.J, ["K"] = KeyCode.K, ["L"] = KeyCode.L,
            ["M"] = KeyCode.M, ["N"] = KeyCode.N, ["O"] = KeyCode.O, ["P"] = KeyCode.P,
            ["Q"] = KeyCode.Q, ["R"] = KeyCode.R, ["S"] = KeyCode.S, ["T"] = KeyCode.T,
            ["U"] = KeyCode.U, ["V"] = KeyCode.V, ["W"] = KeyCode.W, ["X"] = KeyCode.X,
            ["Y"] = KeyCode.Y, ["Z"] = KeyCode.Z,
            // 功能键
            ["F1"] = KeyCode.F1, ["F2"] = KeyCode.F2, ["F3"] = KeyCode.F3, ["F4"] = KeyCode.F4,
            ["F5"] = KeyCode.F5, ["F6"] = KeyCode.F6, ["F7"] = KeyCode.F7, ["F8"] = KeyCode.F8,
            ["F9"] = KeyCode.F9, ["F10"] = KeyCode.F10, ["F11"] = KeyCode.F11, ["F12"] = KeyCode.F12,
            // 导航与编辑键
            ["Enter"] = KeyCode.Enter, ["Esc"] = KeyCode.Escape, ["Space"] = KeyCode.Space,
            ["Tab"] = KeyCode.Tab, ["Backspace"] = KeyCode.Backspace, ["Del"] = KeyCode.Delete,
            ["Ins"] = KeyCode.Insert, ["Home"] = KeyCode.Home, ["End"] = KeyCode.End,
            ["PgUp"] = KeyCode.PageUp, ["PgDn"] = KeyCode.PageDown,
            // 方向键
            ["↑"] = KeyCode.Up, ["↓"] = KeyCode.Down, ["←"] = KeyCode.Left, ["→"] = KeyCode.Right,
            // 符号键
            [","] = KeyCode.Comma, ["."] = KeyCode.Period, ["/"] = KeyCode.Slash,
            [";"] = KeyCode.Semicolon, ["'"] = KeyCode.Quote, ["`"] = KeyCode.Backquote,
            ["["] = KeyCode.BracketLeft, ["]"] = KeyCode.BracketRight, ["\\"] = KeyCode.Backslash,
            ["-"] = KeyCode.Minus, ["="] = KeyCode.Equal, ["_"] = KeyCode.Underscore,
            // 小键盘（ToString 输出名）
            ["Num0"] = KeyCode.NumPad0, ["Num1"] = KeyCode.NumPad1, ["Num2"] = KeyCode.NumPad2,
            ["Num3"] = KeyCode.NumPad3, ["Num4"] = KeyCode.NumPad4, ["Num5"] = KeyCode.NumPad5,
            ["Num6"] = KeyCode.NumPad6, ["Num7"] = KeyCode.NumPad7, ["Num8"] = KeyCode.NumPad8,
            ["Num9"] = KeyCode.NumPad9, ["NumEnter"] = KeyCode.NumPadEnter,
            ["*"] = KeyCode.NumPadMultiply,
        };

        /// <summary>
        /// 解析快捷键字符串（如 "Ctrl+Shift+S"、"F5"）。修饰键顺序任意、不区分大小写；
        /// 无法解析返回 false。与 <see cref="ToString"/> 互为逆操作。
        /// </summary>
        /// <param name="text">快捷键文本；null、空白、未知修饰键、未知主键名均视为无法解析。</param>
        /// <param name="result">解析成功时输出对应的快捷键组合；失败时为 default。</param>
        /// <returns>解析成功返回 true，否则 false。</returns>
        public static bool TryParse(string? text, out KeyCombination result)
        {
            result = default;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            var segments = text.Split('+', StringSplitOptions.TrimEntries);
            if (segments.Length == 0)
                return false;

            // 除最后一段（主键）外，其余段按修饰键解析，顺序任意
            var modifiers = ModifierKeys.None;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (!TryParseModifier(segments[i], out var modifier))
                    return false;
                modifiers |= modifier;
            }

            if (!TryParseKey(segments[^1], out var key))
                return false;

            try
            {
                result = new KeyCombination(key, modifiers);
                return true;
            }
            catch (ArgumentException)
            {
                return false; // 非法主键（如 KeyCode.None）
            }
        }

        private static bool TryParseModifier(string segment, out ModifierKeys modifier)
        {
            modifier = ModifierKeys.None;
            switch (segment.ToUpperInvariant())
            {
                case "CTRL":
                    modifier = ModifierKeys.Control;
                    return true;
                case "ALT":
                    modifier = ModifierKeys.Alt;
                    return true;
                case "SHIFT":
                    modifier = ModifierKeys.Shift;
                    return true;
                case "WIN":
                    modifier = ModifierKeys.Windows;
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryParseKey(string segment, out KeyCode key)
        {
            // 1) ToString 输出的键名（精确映射，含 "Esc"/"Del"/"↑" 等）
            if (KeyNames.TryGetValue(segment, out key))
                return true;

            // 2) 组合键按枚举名解析（如 "Ctrl+Shift+Plus" 的 "Plus"、"Ctrl+Alt+Delete" 的 "Delete"）
            //    拒绝纯数字段，防止 Enum.TryParse 的数值回退把 "67" 解析成枚举成员
            if (!int.TryParse(segment, out _)
                && Enum.TryParse<KeyCode>(segment, ignoreCase: true, out var parsed)
                && parsed != KeyCode.None)
            {
                key = parsed;
                return true;
            }

            key = default;
            return false;
        }

        /// <summary>
        /// 比较两个快捷键组合是否相等。
        /// </summary>
        public static bool operator ==(KeyCombination left, KeyCombination right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// 比较两个快捷键组合是否不相等。
        /// </summary>
        public static bool operator !=(KeyCombination left, KeyCombination right)
        {
            return !(left == right);
        }
    }
}