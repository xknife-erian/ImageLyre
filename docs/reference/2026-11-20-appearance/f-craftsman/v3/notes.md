# 卡尺 Caliper · v3

**这条道怎么落地**：右边一条命令台。等宽输入框里是 `lyre measure roi-1 --stats area,mean,stddev,min,max,perimeter --unit µm --decimals 1`；下面是参数补全表（类型、当前值、Tab 应用），改 flag 回车就重跑；再下面是这次运行的记录 `✓ measure → Results`：exit 0、12 ms、六项读数，可切 JSON；下面是可复制的脚本片段与一行 Agent 调用；底部是历史、撤销、导出、重跑。左边一整张图，`roi-1` 标签与命令目标同色。

**Lyre 自己的东西**：命令、记录、脚本、历史是同一份事实的四种看法——六个数不是一块面板，是这次命令的输出；参数不藏在对话框，就是命令里的 flag；`roi-1` 在图上和命令行里是同一个东西。

**ImageJ 老用户**：Results、Area/Mean/StdDev/Min/Max/Perimeter、黄线 ROI 与顶点把手都在；`measure` 就是 Analyze → Measure，补全提示里写着等价关系；先圈、再回车，五分钟完成第一次测量。

**最大风险**：命令台占掉右侧 416 px，画布不再是全幅；不熟命令行的人第一眼可能找不到「打开 / 画 ROI」的按钮——它们只出现在历史里。
