using System;
using System.Text;

namespace Findx.Utils
{
    /// <summary>
    /// 随机工具
    /// </summary>
    public class RandomUtil
    {
        private static readonly string[] _cnTextArr = new string[] { "的", "一", "国", "在", "人", "了", "有", "中", "是", "年", "和", "大", "业", "不", "为", "发", "会", "工", "经", "上", "地", "市", "要", "个", "产", "这", "出", "行", "作", "生", "家", "以", "成", "到", "日", "民", "来", "我", "部", "对", "进", "多", "全", "建", "他", "公", "开", "们", "场", "展", "时", "理", "新", "方", "主", "企", "资", "实", "学", "报", "制", "政", "济", "用", "同", "于", "法", "高", "长", "现", "本", "月", "定", "化", "加", "动", "合", "品", "重", "关", "机", "分", "力", "自", "外", "者", "区", "能", "设", "后", "就", "等", "体", "下", "万", "元", "社", "过", "前", "面", "农", "也", "得", "与", "说", "之", "员", "而", "务", "利", "电", "文", "事", "可", "种", "总", "改", "三", "各", "好", "金", "第", "司", "其", "从", "平", "代", "当", "天", "水", "省", "提", "商", "十", "管", "内", "小", "技", "位", "目", "起", "海", "所", "立", "已", "通", "入", "量", "子", "问", "度", "北", "保", "心", "还", "科", "委", "都", "术", "使", "明", "着", "次", "将", "增", "基", "名", "向", "门", "应", "里", "美", "由", "规", "今", "题", "记", "点", "计", "去", "强", "两", "些", "表", "系", "办", "教 正", "条", "最", "达", "特", "革", "收", "二", "期", "并", "程", "厂", "如", "道", "际 及", "西", "口", "京", "华", "任", "调", "性", "导", "组", "东", "路", "活", "广", "意", "比", "投", "决", "交", "统", "党", "南", "安", "此", "领", "结", "营", "项", "情", "解", "议", "义", "山", "先", "车", "然", "价", "放", "世", "间", "因", "共", "院", "步", "物", "界", "集", "把", "持", "无", "但", "城", "相", "书", "村", "求", "治", "取", "原", "处", "府", "研", "质", "信", "四", "运", "县", "军", "件", "育", "局", "干", "队", "团", "又", "造", "形", "级", "标", "联", "专", "少", "费", "效", "据", "手", "施", "权", "江", "近", "深", "更", "认", "果", "格", "几", "看", "没", "职", "服", "台", "式", "益", "想", "数", "单", "样", "只", "被", "亿", "老", "受", "优", "常", "销", "志", "战", "流", "很", "接", "乡", "头", "给", "至", "难", "观", "指", "创", "证", "织", "论", "别", "五", "协", "变", "风", "批", "见", "究", "支", "那", "查", "张", "精", "每", "林", "转", "划", "准", "做", "需", "传", "争", "税", "构", "具", "百", "或", "才", "积", "势", "举", "必", "型", "易", "视", "快", "李", "参", "回", "引", "镇", "首", "推", "思", "完", "消", "值", "该", "走", "装", "众", "责", "备", "州", "供", "包", "副", "极", "整", "确", "知", "贸", "己", "环", "话", "反", "身", "选", "亚", "么", "带", "采", "王", "策", "真", "女", "谈", "严", "斯", "况", "色", "打", "德", "告", "仅", "它", "气", "料", "神", "率", "识", "劳", "境", "源", "青", "护", "列", "兴", "许", "户", "马", "港", "则", "节", "款", "拉", "直", "案", "股", "光", "较", "河", "花", "根", "布", "线", "土", "克", "再", "群", "医", "清", "速", "律", "她", "族", "历", "非", "感", "占", "续", "师", "何", "影", "功", "负", "验", "望", "财", "类", "货", "约", "艺", "售", "连", "纪", "按", "讯", "史", "示", "象", "养", "获", "石", "食", "抓", "富", "模", "始", "住", "赛", "客", "越", "闻", "央", "席", "坚" };
        private static readonly string[] _enTextArr = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "N", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private static Random _random = new Random();

        /// <summary>
        /// 获得随机数int值
        /// </summary>
        /// <returns></returns>
        public static int RandomInt()
        {
            return _random.Next();
        }

        /// <summary>
        /// 获得指定范围内的随机数 [0,limit)
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public static int RandomInt(int limit)
        {
            return _random.Next(limit);
        }

        /// <summary>
        /// 获得指定范围内的随机数
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int RandomInt(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// 用随机数填充指定字节数组的元素
        /// </summary>
        /// <param name="bytes"></param>
        public static void RandomBytes(byte[] bytes)
        {
            _random.NextBytes(bytes);
        }

        /// <summary>
        /// 获得指定长度的随机字节数组
        /// </summary>
        /// <param name="length"></param>
        public static byte[] RandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            _random.NextBytes(bytes);
            return bytes;
        }

        /// <summary>
        /// 返回一个介于 0.0 和 1.0 之间的随机数
        /// </summary>
        /// <returns></returns>
        public static double RandomDouble()
        {
            return _random.NextDouble();
        }

        /// <summary>
        /// 获得一个随机的字符串（只包含中文）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomCnString(int length)
        {
            StringBuilder sb = new StringBuilder();
            if (length > 0)
            {
                do
                {
                    sb.Append(_cnTextArr[RandomInt(0, _cnTextArr.Length)]);
                }
                while (--length > 0);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获得一个随机的字符串（只包含数字和字符）
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            StringBuilder sb = new StringBuilder();
            if (length > 0)
            {
                do
                {
                    if (RandomInt(0, 2) > 0)
                    {
                        sb.Append(RandomInt(2, 10));
                    }
                    else
                    {
                        sb.Append(_enTextArr[RandomInt(0, _enTextArr.Length)]);
                    }
                }
                while (--length > 0);
            }
            return sb.ToString();
        }
    }
}
