namespace ImageLyre.UI.Contracts.Keybinding;

/// <summary>
/// 多段组合键序列（如 VS Code 的 "Ctrl+K Ctrl+S"）。
/// 单段序列（Count=1）等价于普通 <see cref="KeyCombination"/> 快捷键。
///
/// ⚠️ 语义定死：
/// - 构造校验：Chords 必须非空，且各段主键不得为 <see cref="KeyCode.None"/>；
/// - 相等性：逐段结构相等（可安全用作字典键，如 <see cref="IKeybindingMap.FindByChord"/>）；
/// - 字符串表示：段间以单个空格分隔（如 "Ctrl+K Ctrl+S"），
///   <see cref="ToString"/> 与 <see cref="TryParse(string?, out KeyChord)"/> 互为逆操作。
/// </summary>
public record KeyChord
{
    /// <summary>
    /// 创建多段组合键序列。
    /// </summary>
    /// <param name="chords">组合键序列，必须非空且各段主键不得为 <see cref="KeyCode.None"/>。
    /// 内容被防御性复制，构造后与传入集合互不影响（保证记录不可变）。</param>
    /// <exception cref="ArgumentNullException">chords 为 null。</exception>
    /// <exception cref="ArgumentException">chords 为空，或某段主键为 <see cref="KeyCode.None"/>。</exception>
    public KeyChord(IReadOnlyList<KeyCombination> chords)
    {
        ArgumentNullException.ThrowIfNull(chords);

        if (chords.Count == 0)
            throw new ArgumentException("快捷键序列不能为空。", nameof(chords));

        foreach (var chord in chords)
        {
            if (chord.Key == KeyCode.None)
                throw new ArgumentException("快捷键序列各段主键不得为 KeyCode.None。", nameof(chords));
        }

        // 防御性复制：调用方后续修改传入集合不影响本实例
        Chords = chords.ToArray();
    }

    /// <summary>
    /// 组合键序列（至少一段，各段主键均非 <see cref="KeyCode.None"/>）。
    /// </summary>
    public IReadOnlyList<KeyCombination> Chords { get; init; }

    /// <summary>
    /// 返回人类可读的规范字符串表示（如 "Ctrl+K Ctrl+S"），段间以单个空格分隔。
    /// 与 <see cref="TryParse(string?, out KeyChord)"/> 互为逆操作。
    /// </summary>
    public override string ToString()
    {
        return string.Join(" ", Chords.Select(c => c.ToString()));
    }

    /// <summary>
    /// 解析快捷键序列字符串（如 "Ctrl+K Ctrl+S"、"F5"）。
    /// 段间以单个空格分隔，每段按 <see cref="KeyCombination.TryParse(string?, out KeyCombination)"/> 解析。
    /// 与 <see cref="ToString"/> 互为逆操作。
    /// </summary>
    /// <param name="text">快捷键序列文本；null、空白、空段（段间连续空格）、段内非法键名均视为无法解析。</param>
    /// <param name="result">解析成功时输出对应的 <see cref="KeyChord"/>；失败时为 null。</param>
    /// <returns>解析成功返回 true，否则 false。</returns>
    public static bool TryParse(string? text, out KeyChord result)
    {
        result = null!;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var segments = text.Trim().Split(' ', StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            return false;

        var chords = new KeyCombination[segments.Length];
        for (var i = 0; i < segments.Length; i++)
        {
            // 空段（如段间连续空格）视为非法
            if (segments[i].Length == 0)
                return false;

            if (!KeyCombination.TryParse(segments[i], out var chord))
                return false;

            chords[i] = chord;
        }

        try
        {
            result = new KeyChord(chords);
            return true;
        }
        catch (ArgumentException)
        {
            // 防御：构造校验在段均解析成功后理论上不会触发，但保证 TryParse 永不抛异常
            result = null!;
            return false;
        }
    }

    /// <summary>
    /// 逐段结构相等（顺序敏感）。
    /// </summary>
    /// <param name="other">用于比较的另一实例</param>
    public virtual bool Equals(KeyChord? other)
    {
        return other is not null
            && other.Chords.Count == Chords.Count
            && Chords.SequenceEqual(other.Chords);
    }

    /// <summary>
    /// 与 <see cref="Equals(KeyChord?)"/> 一致的哈希码。
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var chord in Chords)
        {
            hash.Add(chord);
        }

        return hash.ToHashCode();
    }
}
