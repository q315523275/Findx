using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using Findx.Caching;
using Findx.DependencyInjection;
using Findx.Extensions;
namespace Findx.Module.Admin.Captcha
{
    /// <summary>
    /// 文字点击验证码
    /// </summary>
    public class ClickWordCaptcha : IClickWordCaptcha, ITransientDependency
	{
        private readonly IWebHostEnvironment _environment;
        private readonly ICacheProvider _cacheProvider;

        public ClickWordCaptcha(IWebHostEnvironment environment, ICacheProvider cacheProvider)
        {
            _environment = environment;
            _cacheProvider = cacheProvider;
        }

        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="code">文字</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="point">验证点数量，不能超过文字的长度</param>
        /// <returns></returns>
        public async Task<ClickWordCaptchaResult> CreateCaptchaImage(string code, int width, int height, int point = 3)
        {
            var rtnResult = new ClickWordCaptchaResult();

            Random random = new();
            // 背景
            string bgImagesDir = Path.Combine(_environment.WebRootPath, "Captcha/Image"); //背 景图片路径
            string[] bgImagesFiles = Directory.GetFiles(bgImagesDir); // 背景图片列表
            int imgIndex = random.Next(1, bgImagesFiles.Length); // 随机一个背景图片
            string randomImgFilePath = bgImagesFiles[imgIndex]; // 得到背景图片路径

            // 字体来自：https://www.zcool.com.cn/special/zcoolfonts/
            string fontsDir = Path.Combine(_environment.WebRootPath, "Captcha/Font");//字体路径
                                                                                               //所有字体，如果有多个字体文件的话
            string[] fontFiles = new DirectoryInfo(fontsDir)?.GetFiles()
                                                            ?.Where(m => m.Extension.ToLower() == ".ttf")
                                                            ?.Select(m => m.FullName).ToArray();

            using Image image = await Image.LoadAsync(randomImgFilePath);

            if (image.Width != width || image.Height != height)
            {
                image.Mutate(x => x.Resize(width, height));
            }
            // 字体
            var fontPath = fontFiles[random.Next(fontFiles.Length)];
            var collection = new FontCollection();
            var fontFamily = collection.Add(fontPath);
            // collection.Install("path/to/emojiFont.ttf");//可以安装多个
            // collection.InstallCollection("path/to/font.ttc");
            //
            List<string> words = new();
            // 循环所有文字
            for (int i = 0; i < code.Length; i++)
            {
                // 文字
                var word = code[i].ToString();
                var font = fontFamily.CreateFont(random.Next(18, 30));  // 字体
                // 颜色
                Color[] colorList = { Color.Black, Color.DarkBlue, Color.Green, Color.Brown, Color.DarkCyan, Color.Purple };
                var colorIndex = random.Next(colorList.Length);
                var color = colorList[colorIndex];
                // 坐标
                int _x = random.Next(30, width - 30); // 随机一个宽度
                int _y = random.Next(30, height - 30); // 随机一个高度

                // 写入文字
                image.Mutate(x => x.DrawText(
                     word,   //文字内容
                     font,   //字体
                     color,   //颜色
                     new PointF(_x, _y))   //坐标
                );

                if (rtnResult.RepData.Point.Count < point)
                {
                    // 记录坐标
                    rtnResult.RepData.Point.Add(new PointPosModel() { X = _x, Y = _y });
                    // 记录文字
                    words.Add(word);
                }
            }

            //记录文字
            rtnResult.RepData.WordList = words;

            MemoryStream ms = new MemoryStream();
            await image.SaveAsJpegAsync(ms);// 位图保存成jpeg
                                            // 转成base64
            rtnResult.RepData.OriginalImageBase64 = Convert.ToBase64String(ms.GetBuffer());
            ms.Dispose();
            rtnResult.RepData.Token = System.Guid.NewGuid().ToString("N");;

            // 缓存验证码正确位置集合
            var cache = _cacheProvider.Get();
            cache.Add(Const.CommonConst.CACHE_KEY_CODE + rtnResult.RepData.Token, rtnResult.RepData.Point, TimeSpan.FromMinutes(1));

            rtnResult.RepData.Point = null; // 清空位置信息
            return rtnResult;
        }

        /// <summary>
        /// 随机绘制字符串
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public string RandomCode(int number)
        {
            char[] str_char_arrary = { '赵', '钱', '孙', '李', '周', '吴', '郑', '王', '冯', '陈', '褚', '卫', '蒋', '沈', '韩', '杨',
                '朱', '秦', '尤', '许', '何', '吕', '施', '张', '孔', '曹', '严', '华', '金', '魏', '陶', '姜', '戚', '谢', '邹', '喻', '柏', '水',
                '窦', '章', '云', '苏', '潘', '葛', '奚', '范', '彭', '郎', '鲁', '韦', '昌', '马', '苗', '凤', '花', '方', '任', '袁', '柳', '鲍',
                '史', '唐', '费', '薛', '雷', '贺', '倪', '汤', '滕', '殷', '罗', '毕', '郝', '安', '常', '傅', '卞', '齐', '元', '顾', '孟', '平',
                '黄', '穆', '萧', '尹', '姚', '邵', '湛', '汪', '祁', '毛', '狄', '米', '伏', '成', '戴', '谈', '宋', '茅', '庞', '熊', '纪', '舒',
                '屈', '项', '祝', '董', '梁', '杜', '阮', '蓝', '闵', '季', '贾', '路', '娄', '江', '童', '颜', '郭', '梅', '盛', '林', '钟', '徐',
                '邱', '骆', '高', '夏', '蔡', '田', '樊', '胡', '凌', '霍', '虞', '万', '支', '柯', '管', '卢', '莫', '柯', '房', '裘', '缪', '解',
                '应', '宗', '丁', '宣', '邓', '单', '杭', '洪', '包', '诸', '左', '石', '崔', '吉', '龚', '程', '嵇', '邢', '裴', '陆', '荣', '翁',
                '荀', '于', '惠', '甄', '曲', '封', '储', '仲', '伊', '宁', '仇', '甘', '武', '符', '刘', '景', '詹', '龙', '叶', '幸', '司', '黎',
                '溥', '印', '怀', '蒲', '邰', '从', '索', '赖', '卓', '屠', '池', '乔', '胥', '闻', '莘', '党', '翟', '谭', '贡', '劳', '逄', '姬',
                '申', '扶', '堵', '冉', '宰', '雍', '桑', '寿', '通', '燕', '浦', '尚', '农', '温', '别', '庄', '晏', '柴', '瞿', '阎', '连', '习',
                '容', '向', '古', '易', '廖', '庾', '终', '步', '都', '耿', '满', '弘', '匡', '国', '文', '寇', '广', '禄', '阙', '东', '欧', '利',
                '师', '巩', '聂', '关', '荆',
                '伟', '刚', '勇', '毅', '俊', '峰', '强', '军', '平', '保', '东', '文', '辉', '力', '明', '永', '健', '世', '广', '志', '义', '兴',
                '良', '海', '山', '仁', '波', '宁', '贵', '福', '生', '龙', '元', '全', '国', '胜', '学', '祥', '才', '发', '武', '新', '利', '清',
                '飞', '彬', '富', '顺', '信', '子', '杰', '涛', '昌', '成', '康', '星', '光', '天', '达', '安', '岩', '中', '茂', '进', '林', '有',
                '坚', '和', '彪', '博', '诚', '先', '敬', '震', '振', '壮', '会', '思', '群', '豪', '心', '邦', '承', '乐', '绍', '功', '松', '善',
                '厚', '庆', '磊', '民', '友', '裕', '河', '哲', '江', '超', '浩', '亮', '政', '谦', '亨', '奇', '固', '之', '轮', '翰', '朗', '伯',
                '宏', '言', '若', '鸣', '朋', '斌', '梁', '栋', '维', '启', '克', '伦', '翔', '旭', '鹏', '泽', '晨', '辰', '士', '以', '建', '家',
                '致', '树', '炎', '德', '行', '时', '泰', '盛', '雄', '琛', '钧', '冠', '策', '腾', '楠', '榕', '风', '航', '弘', '秀', '娟', '英',
                '华', '慧', '巧', '美', '娜', '静', '淑', '惠', '珠', '翠', '雅', '芝', '玉', '萍', '红', '娥', '玲', '芬', '芳', '燕', '彩', '春',
                '菊', '兰', '凤', '洁', '梅', '琳', '素', '云', '莲', '真', '环', '雪', '荣', '爱', '妹', '霞', '香', '月', '莺', '媛', '艳', '瑞',
                '凡', '佳', '嘉', '琼', '勤', '珍', '贞', '莉', '桂', '娣', '叶', '璧', '璐', '娅', '琦', '晶', '妍', '茜', '秋', '珊', '莎', '锦',
                '黛', '青', '倩', '婷', '姣', '婉', '娴', '瑾', '颖', '露', '瑶', '怡', '婵', '雁', '蓓', '纨', '仪', '荷', '丹', '蓉', '眉', '君',
                '琴', '蕊', '薇', '菁', '梦', '岚', '苑', '婕', '馨', '瑗', '琰', '韵', '融', '园', '艺', '咏', '卿', '聪', '澜', '纯', '毓', '悦',
                '昭', '冰', '爽', '琬', '茗', '羽', '希', '欣', '飘', '育', '滢', '馥', '筠', '柔', '竹', '霭', '凝', '晓', '欢', '霄', '枫', '芸',
                '菲', '寒', '伊', '亚', '宜', '可', '姬', '舒', '影', '荔', '枝', '丽', '阳', '妮', '宝', '贝', '初', '程', '梵', '罡', '恒', '鸿',
                '桦', '骅', '剑', '娇', '纪', '宽', '苛', '灵', '玛', '媚', '琪', '晴', '容', '睿', '烁', '堂', '唯', '威', '韦', '雯', '苇', '萱',
                '阅', '彦', '宇', '雨', '洋', '忠', '宗', '曼', '紫', '逸', '贤', '蝶', '菡', '绿', '蓝', '儿', '翠', '烟' };

            var rand = new Random();
            var hs = new HashSet<char>();
            var randomBool = true;
            while (randomBool)
            {
                if (hs.Count == number)
                    break;
                var rand_number = rand.Next(str_char_arrary.Length);
                hs.Add(str_char_arrary[rand_number]);
            }
            return string.Join("", hs);
        }

        /// <summary>
        /// 验证码验证
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public Task<ClickWordCaptchaResult> CheckCode(ClickWordCaptchaRequest req)
        {
            var cache = _cacheProvider.Get();
            var res = new ClickWordCaptchaResult();

            var rightVCodePos = cache.Get<List<PointPosModel>>(Const.CommonConst.CACHE_KEY_CODE + req.Token);
            if (rightVCodePos == null)
            {
                res.RepCode = "6110";
                res.RepMsg = "验证码已失效，请重新获取";
                return Task.FromResult(res);
            }

            var userVCodePos = req.PointJson.ToObject<List<PointPosModel>>();
            if (userVCodePos == null || userVCodePos.Count < rightVCodePos.Count)
            {
                res.RepCode = "6111";
                res.RepMsg = "验证码无效";
                return Task.FromResult(res);
            }

            int allowOffset = 25; // 允许的偏移量(点触容错)
            for (int i = 0; i < userVCodePos.Count; i++)
            {
                var xOffset = userVCodePos[i].X - rightVCodePos[i].X;
                var yOffset = userVCodePos[i].Y - rightVCodePos[i].Y;
                xOffset = Math.Abs(xOffset); // x轴偏移量
                yOffset = Math.Abs(yOffset); // y轴偏移量
                                             // 只要有一个点的任意一个轴偏移量大于allowOffset，则验证不通过
                if (xOffset > allowOffset || yOffset > allowOffset)
                {
                    res.RepCode = "6112";
                    res.RepMsg = "验证码错误";
                    return Task.FromResult(res);
                }
            }

            cache.Remove(Const.CommonConst.CACHE_KEY_CODE + req.Token);
            res.RepCode = "0000";
            res.RepMsg = "验证成功";
            return Task.FromResult(res);
        }
    }
}

