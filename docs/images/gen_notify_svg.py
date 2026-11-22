# -*- coding: utf-8 -*-
"""生成 Findx.Extensions.Notify 架构图 SVG（1600 宽画布、手工坐标）。"""

FONT = "PingFang SC, Hiragino Sans GB, Microsoft YaHei, sans-serif"
TEAL, BLUE, AMBER, PURPLE, SLATE, INK, MUTED = (
    "#0f766e", "#1d4ed8", "#b45309", "#6d28d9", "#475569", "#0f172a", "#64748b")
TINTS = {TEAL: "#f0fdfa", BLUE: "#eff6ff", AMBER: "#fffbeb",
         PURPLE: "#f5f3ff", SLATE: "#f8fafc"}


def esc(s):
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def tw(s, size):
    """粗略文字宽度：汉字 1em，ASCII 0.55em。"""
    return sum(size if ord(c) > 127 else size * 0.56 for c in s)


def wrap(s, size, max_w):
    """按 token 换行：ASCII 单词/路径不拆，中文按字断行。"""
    import re
    toks = re.findall(r"[A-Za-z0-9_@./:+=()\-]+|\s+|.", s)
    lines, cur, cur_w = [], "", 0.0
    for t in toks:
        tw_ = tw(t, size)
        if cur and cur_w + tw_ > max_w:
            lines.append(cur.rstrip())
            cur, cur_w = "", 0.0
            if t.isspace():
                continue
        if tw_ > max_w:  # 单个 token 比整行还宽，退化为逐字断
            for ch in t:
                cw = tw(ch, size)
                if cur and cur_w + cw > max_w:
                    lines.append(cur)
                    cur, cur_w = "", 0.0
                cur += ch
                cur_w += cw
            continue
        cur += t
        cur_w += tw_
    if cur.strip():
        lines.append(cur.rstrip())
    return lines


def text(x, y, s, size=12.5, fill="#334155", weight="normal", anchor="start"):
    return (f'<text x="{x}" y="{y}" font-size="{size}" fill="{fill}" font-weight="{weight}" '
            f'text-anchor="{anchor}" font-family="{FONT}">{esc(s)}</text>')


def box(x, y, w, h, title, lines, color=TEAL, title_size=13.5, line_size=12,
        line_h=20, pad=16, dashed=False):
    fill = TINTS.get(color, "#f8fafc")
    dash = ' stroke-dasharray="6 5"' if dashed else ""
    out = [f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="10" fill="{fill}" '
           f'stroke="{color}" stroke-width="1.3"{dash}/>',
           f'<rect x="{x}" y="{y}" width="4" height="{h}" rx="2" fill="{color}"/>']
    out.append(text(x + pad, y + 24, title, title_size, color, "700"))
    ly = y + 24 + title_size + 10
    for ln in lines:
        if not ln:
            ly += line_h * 0.5
            continue
        weight, fc = "normal", "#334155"
        if ln.startswith("!"):
            ln, fc = ln[1:], AMBER
        elif ln.startswith("*"):
            ln, fc, weight = ln[1:], "#1e293b", "600"
        for i, sub in enumerate(wrap(ln, line_size, w - pad * 2 - 4)):
            out.append(text(x + pad, ly, sub, line_size, fc, weight if i == 0 else "normal"))
            ly += line_h
    return "\n".join(out)


def arrow(x1, y1, x2, y2, color=TEAL, label=None, dash=False, lx=None, ly=None,
          anchor="middle", lsize=10.5):
    dash_s = ' stroke-dasharray="7 5"' if dash else ""
    marker = "arrowhead" if color == TEAL else "arrowheadgray"
    out = [f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{color}" stroke-width="1.9"'
           f'{dash_s} marker-end="url(#{marker})"/>']
    if label:
        out.append(text(lx if lx is not None else (x1 + x2) // 2,
                        ly if ly is not None else min(y1, y2) - 8,
                        label, lsize, MUTED, "normal", anchor))
    return "\n".join(out)


def head(title, subtitle, w, h, legend2=False):
    out = [f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" '
           f'viewBox="0 0 {w} {h}" font-family="{FONT}">',
           '<rect width="100%" height="100%" fill="#FFFFFF"/>',
           '<defs><marker id="arrowhead" markerWidth="9" markerHeight="9" refX="8" refY="4.5" orient="auto">'
           '<path d="M0,0 L9,4.5 L0,9 z" fill="#0f766e"/></marker>'
           '<marker id="arrowheadgray" markerWidth="9" markerHeight="9" refX="8" refY="4.5" orient="auto">'
           '<path d="M0,0 L9,4.5 L0,9 z" fill="#64748b"/></marker></defs>',
           text(60, 54, title, 29, INK, "700"),
           text(60, 86, subtitle, 13, MUTED)]
    out += [f'<line x1="{w-320}" y1="48" x2="{w-278}" y2="48" stroke="{TEAL}" stroke-width="2" marker-end="url(#arrowhead)"/>',
            text(w - 268, 52, "调用 / 发送", 11.5, SLATE)]
    if legend2:
        out += [f'<line x1="{w-320}" y1="74" x2="{w-278}" y2="74" stroke="{SLATE}" stroke-width="2" stroke-dasharray="7 5" marker-end="url(#arrowheadgray)"/>',
                text(w - 268, 78, "返回 / 落库 / 后台", 11.5, SLATE)]
    return "\n".join(out)


# ================================================================ 图 1
H = 950
parts = [head("Findx 消息通知组件架构（单出口 · 五通道 · 业务线程零 IO）",
              "Findx.Extensions.Notify：一个 INotifySender 出口 + IUserNotifier 通用契约；通道按配置注册，同步 / 异步两条路径共用同一套路由、超时与日志", 1600, H, True)]

# ① 装配带
parts.append(f'<rect x="44" y="110" width="1512" height="150" rx="12" fill="#f8fafc" stroke="{SLATE}" stroke-width="1.1"/>')
parts.append(text(60, 136, "① 装配：NotifyModule（Level=Application，Order=130）按配置节 Findx:Notify 决定注册哪些服务", 13, SLATE, "700"))
chips1 = [("INotifySender → NotifySender", 60, 290), ("IUserNotifier（Replace 同一实例）", 366, 300),
          ("NotifyChannelDispatcher", 682, 240), ("NotifyOptions（Findx:Notify）", 938, 260)]
for label, cx, w in chips1:
    parts.append(f'<rect x="{cx}" y="150" width="{w}" height="38" rx="8" fill="#ffffff" stroke="{TEAL}" stroke-width="1.1"/>')
    parts.append(text(cx + 12, 174, label, 11.5, TEAL, "600"))
chips2 = [("NotifyDispatchQueue（BackgroundService）", 60, 300), ("NotifyLogStore", 376, 150),
          ("INotifyChannel × N（仅 Enabled=true 注册）", 542, 330), ("AddHttpClient（IHttpClientFactory）", 888, 310)]
for label, cx, w in chips2:
    parts.append(f'<rect x="{cx}" y="198" width="{w}" height="38" rx="8" fill="#ffffff" stroke="{BLUE}" stroke-width="1.1"/>')
    parts.append(text(cx + 12, 222, label, 11.5, BLUE, "600"))

# ② 调用方
parts.append(box(44, 286, 240, 176, "② 调用方",
                 ["业务代码：Send / Queue", "框架契约：IUserNotifier", "广播：DefaultChannels", "  默认 WeCom，可配多个"], PURPLE, line_size=11.5))

# ③ 门面
parts.append(box(364, 286, 300, 282, "③ NotifySender（门面）",
                 ["两个契约指向同一实例", "*SendAsync：发送 + 落库", "*QueueAsync：构 log + TryWrite",
                  "*Broadcast：逐通道 Clone 并行", "*IUserNotifier：映射契约字段", "★ 广播：单通道失败不影响其它",
                  "★ 队列满 → 降级同步发送", "★ LogEnabled=false 不落库", "Id 由 IKeyGenerator 先给"], TEAL, line_size=11.5, line_h=20))

# ④ dispatcher
parts.append(box(744, 286, 260, 160, "④ Dispatcher",
                 ["通道路由 + 超时收敛", "总开关关闭 → Fail", "通道未注册 → Fail", "linked CTS 到点取消", "异常收敛为 Fail"], BLUE, line_size=11.5))

# ⑤ queue
parts.append(box(744, 486, 260, 150, "⑤ 异步队列",
                 ["有界 Channel 2048", "FullMode=Wait，满即 false", "消费线程 × 2", "停机：残留随进程丢弃"], AMBER, line_size=11.5))

# ⑥ 五个通道卡片
ch_y, ch_h, ch_gap = 286, 58, 14
chans = ["WeCom 企业微信", "DingTalk 钉钉（可加签）", "Email 邮件（SMTP）", "WeChat 公众号模板消息", "WeChatMiniProgram 订阅"]
for i, c in enumerate(chans):
    y = ch_y + i * (ch_h + ch_gap)
    parts.append(f'<rect x="1084" y="{y}" width="170" height="{ch_h}" rx="9" fill="{TINTS[BLUE]}" stroke="{BLUE}" stroke-width="1.2"/>')
    parts.append(f'<rect x="1084" y="{y}" width="4" height="{ch_h}" rx="2" fill="{BLUE}"/>')
    for j, sub in enumerate(wrap(c, 11.5, 146)):
        parts.append(text(1098, y + 24 + j * 16, sub, 11.5, "#1e293b", "600"))
parts.append(f'<rect x="1084" y="286" width="170" height="346" rx="10" fill="none" stroke="{BLUE}" stroke-width="1.2" stroke-dasharray="6 5"/>')
parts.append(text(1092, 660, "新增通道：实现 INotifyChannel", 11, MUTED))
parts.append(text(1092, 678, "并在 Module 中按配置注册", 11, MUTED))

# ⑦ 外部平台
parts.append(box(1334, 286, 222, 346, "⑦ 外部平台",
                 ["qyapi.weixin.qq.com", "  webhook/send", "  message/send", "oapi.dingtalk.com/robot",
                  "SMTP 服务器", "api.weixin.qq.com", "  template/send", "  subscribe/send", "",
                  "token：AccessTokenProvider", "进程内缓存，过期重取"], PURPLE, line_size=11, line_h=19))

# 箭头
parts.append(arrow(284, 356, 362, 356, TEAL, "调用", ly=348, lsize=10))
parts.append(arrow(664, 330, 742, 330, TEAL, "同步发送", ly=322, lsize=10))
parts.append(arrow(664, 570, 742, 570, TEAL, "TryWrite", ly=562, lsize=10))
parts.append(arrow(874, 486, 874, 448, SLATE, "消费线程也调用它", lx=866, ly=470, anchor="end", lsize=10))
parts.append(text(1088, 280, "通道组：按 message.Channel 选一（未注册即 Fail）", 10.5, BLUE, "600"))
parts.append(arrow(1008, 330, 1070, 330, TEAL, "按通道路由", lx=1039, ly=320, lsize=9.5))
parts.append(arrow(1254, 330, 1332, 330, TEAL, "HTTP/SMTP", ly=322, lsize=10))

# ⑧ 存储带
parts.append(arrow(514, 568, 514, 696, SLATE, "PersistAsync", lx=522, ly=636, anchor="start", lsize=10, dash=True))
parts.append(arrow(874, 636, 874, 696, SLATE, "PersistAsync", lx=882, ly=666, anchor="start", lsize=10, dash=True))
parts.append(f'<rect x="44" y="700" width="1512" height="230" rx="12" fill="{TINTS[AMBER]}" stroke="{AMBER}" stroke-width="1.2"/>')
parts.append(text(60, 728, "⑧ 存储：NotifyLogStore → 表 notify_logs（EntityExtension DataSource=Notify）", 13, AMBER, "700"))
parts.append(text(60, 756, "字段：Channel / Title / Content(2000) / Receivers(500) / TemplateId / Status / Success / ErrorMessage(500) / BusinessKey / NodeName / Payload(JSON 全长) / SendTime / ElapsedMs",
                  11, "#334155"))
parts.append(text(60, 782, "★ 落库失败只记 Warning：通知已发出，日志写失败不反噬主流程；★ Payload 是完整消息快照（可重放），但当前没有自动重试", 11, "#334155"))
parts.append(text(60, 808, "★ 只有一次 INSERT：同步路径 = 发送返回后写；异步路径 = 消费线程 Apply 之后写。没有「发送中」行，队列在途消息无法从库观测", 11, "#334155"))
parts.append(text(60, 838, "★ 业务拿到的是 Id：QueueAsync 先把主键生成好并返回，日志行随后落库，可用于按业务键反查", 11, "#334155"))
parts.append(text(60, 872, "边界：内存队列 + 单次发送 → 进程崩溃时未发送的消息丢失；通道失败只记日志不重试。若要「不丢 + 可重试」，需要在业务侧落库补偿或换持久化队列", 11, AMBER, "600"))

parts.append("</svg>")
open("notify-architecture.svg", "w", encoding="utf-8").write("\n".join(parts))

# ================================================================ 图 2
H = 960
parts = [head("两条发送路径与全部分支（同步 / 异步 / 降级）",
              "同一份内容、同一套路由与日志；差别只在「谁承担网络耗时」与「结果何时可知」", 1600, H, True)]

parts.append(text(60, 138, "A 同步路径 SendAsync —— 调用方承担一次网络往返，能立刻知道成败", 12.5, TEAL, "700"))
sw, gap, by = 208, 30, 156
for i, (t, ls) in enumerate([
    ("① 业务线程", ["SendAsync(message)", "阻塞等待结果"]),
    ("② Dispatcher", ["总开关 / 通道查表", "TimeoutSeconds 取消"]),
    ("③ 通道实现", ["HTTP 或 SMTP", "异常 → Fail 结果"]),
    ("④ 回填结果", ["NotifyLog.Apply", "Success / Failed"]),
    ("⑤ 落库", ["INSERT notify_logs", "一次写入（终态）"]),
    ("⑥ 返回", ["NotifyResult 给业务", "Success + Message"]),
]):
    parts.append(box(60 + i * (sw + gap), by, sw, 118, t, ls, TEAL if i < 3 else SLATE, title_size=12.5, line_size=11.5, line_h=19))
for i in range(5):
    x1 = 60 + i * (sw + gap) + sw
    parts.append(arrow(x1, by + 57, x1 + gap - 2, by + 57, TEAL))
parts.append(text(60, 298, "调用线程耗时 = 通道往返（≤ TimeoutSeconds）+ 一次 INSERT；通道抛异常已被 dispatcher 收敛为 NotifyResult.Fail，不向业务抛出", 11, MUTED))

parts.append(text(60, 348, "B 异步路径 QueueAsync —— 业务线程只做对象构造 + 一次 TryWrite，发送与落库在后台", 12.5, BLUE, "700"))
by1 = 362
for i, (t, ls) in enumerate([
    ("① 业务线程", ["QueueAsync(message)", "NotifyLog.From + Payload"]),
    ("② TryEnqueue", ["Channel.TryWrite", "非阻塞，返回 bool"]),
    ("③ 立即返回", ["return log.Id", "主键已生成，零 IO"]),
]):
    parts.append(box(60 + i * (sw + gap), by1, sw, 118, t, ls, BLUE, title_size=12.5, line_size=11.5, line_h=19))
for i in range(2):
    x1 = 60 + i * (sw + gap) + sw
    parts.append(arrow(x1, by1 + 57, x1 + gap - 2, by1 + 57, BLUE))

parts.append(f'<line x1="60" y1="522" x2="1540" y2="522" stroke="{SLATE}" stroke-width="1" stroke-dasharray="9 6"/>')
parts.append(text(62, 512, "线程边界：以上在业务线程（毫秒级以内）／以下在后台消费线程（网络与数据库耗时都在这里）", 11, SLATE, "600"))

# 后台消费行（与 ③ 对齐，虚线箭头直达）
by2 = 540
bw, bg = 184, 20
for i, (t, ls) in enumerate([
    ("④ 消费线程", ["ReadAllAsync", "× DispatchWorkerCount"]),
    ("⑤ 还原消息", ["log.ToMessage()", "优先 Payload 快照"]),
    ("⑥ 通道路由", ["dispatcher.SendAsync", "与同步路径同一份"]),
    ("⑦ 回填 + 落库", ["Apply → INSERT", "LogEnabled 才写"]),
    ("⑧ 失败仅告警", ["LogWarning", "不重试、不回写"]),
]):
    parts.append(box(536 + i * (bw + bg), by2, bw, 118, t, ls, AMBER, title_size=12, line_size=11, line_h=19))
for i in range(4):
    x1 = 536 + i * (bw + bg) + bw
    parts.append(arrow(x1, by2 + 57, x1 + bg - 2, by2 + 57, AMBER))
parts.append(arrow(628, by1 + 118, 628, by2 - 2, SLATE, "队列（内存）", lx=638, ly=506, anchor="start", lsize=10, dash=True))

# 左侧后台说明块
parts.append(box(60, 540, 452, 118, "后台消费段（ServerDefaults 边界）",
                 ["队列 = 进程内有界 Channel（QueueCapacity 2048）", "满了 TryWrite=false，由调用方降级，不阻塞生产者", "消费线程 2 个，发送 + 落库都在这里"], SLATE, title_size=12, line_size=10.8, line_h=18))
parts.append(text(60, 688, "后台发送失败不改变业务已返回的 Id：日志行落成 Failed（ErrorMessage + ElapsedMs），业务可据此反查，但目前没有自动重试或补偿", 11, MUTED))

# C 分支
parts.append(text(60, 734, "C 分支矩阵 —— 每个分支都有确定结果，不存在「静默丢弃」", 12.5, "#1e293b", "700"))
cw, cgap = 288, 18
cards = [
    ("队列满（TryWrite=false）", ["降级为同步发送（回到调用线程）", "仍返回同一个 log.Id", "消息不丢，代价是本次耗时"], AMBER),
    ("组件总开关关闭", ["Fail：通知组件未启用", "提示 Findx:Notify:Enabled", "不抛异常"], SLATE),
    ("通道未启用", ["Fail：通道 X 未启用", "提示配 Findx:Notify:X:Enabled", "未注册即无半可用态"], SLATE),
    ("超时 / 通道异常", ["linked CTS 到点取消 → Fail", "异常也收敛成 Fail", "业务只看到结果，不看到栈"], SLATE),
    ("进程停机", ["队列残留随进程结束丢弃", "已发出的日志行保留", "属日志级数据，可接受"], SLATE),
]
for i, (t, ls, c) in enumerate(cards):
    parts.append(box(60 + i * (cw + cgap), 752, cw, 122, t, ls, c, title_size=11.8, line_size=11, line_h=21))

parts.append(text(60, 910, "★ 唯一「会丢」的窗口：异步路径已入队但未发送 + 进程崩溃（无持久化队列、无重试）。若业务要求不丢，请用同步 SendAsync 或自行落库后补偿",
                  11, AMBER, "600"))
parts.append(text(60, 942, "★ 两条路径日志语义一致（结果 + 耗时），差异在 CreatedTime：异步是入队时刻，可据此算出排队延迟", 11, MUTED))

parts.append("</svg>")
open("notify-paths.svg", "w", encoding="utf-8").write("\n".join(parts))

# ================================================================ 图 3
H = 780
parts = [head("五个通道的差异、notify_logs 数据模型与配置速查",
              "每条通道自身只做「协议组装 + 平台返回码翻译」；路由、超时、日志都在上层收敛", 1600, H)]

tx, ty, tw_ = 60, 132, 950
cols = [175, 248, 272, 255]
rows = [
    ("通道 / 枚举", "发送方式与依赖", "关键配置（Findx:Notify）", "目标账号语义（To）"),
    ("WeCom（0）", "Webhook 群机器人，或应用消息（WebhookUrl 优先）", "WeCom:WebhookUrl；或 CorpId/Secret/AgentId", "群机器人忽略 To；应用消息为成员账号，空则 @all"),
    ("DingTalk（1）", "群机器人 Webhook，可选加签", "DingTalk:WebhookUrl / Secret", "标题正文 + AtMobiles / AtAll"),
    ("Email（2）", "SMTP，复用框架 Findx:Email 配置", "Email:Enabled / IsBodyHtml", "邮箱地址，多个逗号拼接"),
    ("WeChat（3）", "公众号模板消息，按 openid 逐条发", "WeChat:AppId / AppSecret / TemplateId", "openid，须配 TemplateId + Data"),
    ("WeChatMiniProgram（4）", "小程序订阅消息，按 openid 逐条发", "AppId / AppSecret / TemplateId / Page / 版本与语言", "openid，超长字段按规则截断（默认开）"),
]
cell_lines, row_hs = [], []
for r in rows[1:]:
    ls = [wrap(c, 10.4, cols[i] - 26) for i, c in enumerate(r)]
    cell_lines.append(ls)
    row_hs.append(max(len(x) for x in ls) * 16 + 24)
head_h = 44
total_h = head_h + sum(row_hs)
parts.append(f'<rect x="{tx}" y="{ty}" width="{tw_}" height="{total_h}" rx="10" fill="#ffffff" stroke="{SLATE}" stroke-width="1.2"/>')
parts.append(f'<rect x="{tx}" y="{ty}" width="{tw_}" height="{head_h}" fill="#f1f5f9"/>')
xx = tx
for i, hh in enumerate(rows[0]):
    parts.append(text(xx + 12, ty + 27, hh, 11.3, SLATE, "700"))
    xx += cols[i]
ry = ty + head_h
for ri, ls in enumerate(cell_lines):
    parts.append(f'<line x1="{tx}" y1="{ry}" x2="{tx+tw_}" y2="{ry}" stroke="#e2e8f0" stroke-width="1"/>')
    for j, txt in enumerate(rows[ri + 1]):
        xx = tx + sum(cols[:j])
        for k, sub in enumerate(ls[j]):
            parts.append(text(xx + 12, ry + 22 + k * 16, sub, 10.4,
                              INK if j == 0 else "#334155", "600" if j == 0 else "normal"))
    ry += row_hs[ri]

parts.append(text(60, ty + total_h + 26, "★ 逐条发送：公众号 / 小程序不支持批量 touser，To 有 N 个 openid 时是 N 次 HTTP（单条失败只影响该条）", 10.8, MUTED))
parts.append(text(60, ty + total_h + 48, "★ 通道自检：参数缺失时通道直接返回 Fail 可读原因；未启用则不会被注册到 dispatcher 字典（不会发到平台才报错）", 10.8, MUTED))

# 右侧：数据模型 + 状态口径 + 配置速查
rx, rw = 1040, 500
parts.append(box(rx, 132, rw, 182, "notify_logs（四组字段）",
                 ["*身份：Id（雪花，入队即生成）+ Channel",
                  "*内容：Title / Content(2000) / Receivers(500) / TemplateId",
                  "*结果：Status / Success / ErrorMessage(500) / SendTime / ElapsedMs",
                  "*追溯：BusinessKey / NodeName / Payload(JSON 全长)",
                  "审计：FullAuditedBase 且 DisableAuditing（只记发送事实）"], AMBER, line_size=10.8, line_h=22))
parts.append(box(rx, 334, rw, 182, "状态口径（Queued 只存在于进程内）",
                 ["Queued=0 → 入队初值，从未落库", "Success=1 / Failed=2 → 落库即终态",
                  "所以表里只有两种行：成功、失败", "无「发送中」行 → 在途消息不可从库观测"], SLATE, line_size=10.8, line_h=22))
parts.append(box(rx, 536, rw, 196, "配置速查（Findx:Notify）",
                 ["Enabled=true / DefaultChannels=[WeCom]",
                  "TimeoutSeconds=10 → 单次发送上限",
                  "LogEnabled=true → 关闭后两条路径都不写库",
                  "NodeName → 多实例定位发送节点",
                  "QueueCapacity=2048 / DispatchWorkerCount=2",
                  "各通道 Enabled 默认 false（不启用不注册）"], TEAL, line_size=10.8, line_h=22))

# 生命周期
sy = 566
parts.append(text(60, sy - 14, "日志行的生命周期（落库是一次 INSERT，不是状态更新）", 12.5, "#1e293b", "700"))
nodes = [("Queued=0", ["内存中的 NotifyLog", "业务线程零 IO"], SLATE),
         ("Success=1", ["平台返回成功", "Success=true"], TEAL),
         ("Failed=2", ["超时 / 异常 / 平台码", "ErrorMessage 记原因"], AMBER)]
for i, (t, ls, c) in enumerate(nodes):
    parts.append(box(60 + i * 320, sy, 200, 112, t, ls, c, title_size=12.2, line_size=10.8, line_h=20))
parts.append(arrow(260, sy + 52, 376, sy + 52, TEAL, "成功", ly=sy + 42, lsize=10))
parts.append(arrow(580, sy + 52, 696, sy + 52, AMBER, "失败", ly=sy + 42, lsize=10))
parts.append(text(60, sy + 148, "★ 结论：日志表是「发送事实账本」，不是「状态机账本」；要看在途与重试，需要额外的队列 / 执行记录表", 10.8, AMBER, "600"))

parts.append("</svg>")
open("notify-channels.svg", "w", encoding="utf-8").write("\n".join(parts))
print("ok")
