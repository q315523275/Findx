namespace Findx.UA
{
    /// <summary>
    /// 平台对象
    /// </summary>
    public class Platform : UserAgentInfo
	{
        /// <summary>
        /// 未知
        /// </summary>
        public static Platform Unknown = new Platform(NameUnknown, null);

        /// <summary>
        /// Iphone
        /// </summary>
        public static readonly Platform Iphone = new Platform("iPhone", "iphone");

        /// <summary>
        /// ipod
        /// </summary>
        public static readonly Platform Ipod = new Platform("iPod", "ipod");

        /// <summary>
        /// ipad
        /// </summary>
        public static readonly Platform Ipad = new Platform("iPad", "ipad");

        /// <summary>
        /// android
        /// </summary>
        public static readonly Platform Android = new Platform("Android", "android");

        /// <summary>
        /// GOOGLE_TV
        /// </summary>
        public static readonly Platform GoogleTv = new Platform("GoogleTV", "googletv");

        /// <summary>
        /// Windows Phone
        /// </summary>
        public static readonly Platform WindowsPhone = new Platform("Windows Phone", "windows (ce|phone|mobile)( os)?");

        /// <summary>
        /// 支持的移动平台类型
        /// </summary>
        public static List<Platform> MobilePlatforms = new List<Platform> { //
        		WindowsPhone, //
        		Ipad, //
        		Ipod, //
        		Iphone, //
        		Android, //
        		GoogleTv, //
        		new Platform("htcFlyer", "htc_flyer"), //
        		new Platform("Symbian", "symbian(os)?"), //
        		new Platform("Blackberry", "blackberry") //
        };

        /// <summary>
        /// 支持的桌面平台类型
        /// </summary>
        public static List<Platform> DesktopPlatforms = new List<Platform> { //
        		new Platform("Windows", "windows"), //
        		new Platform("Mac", "(macintosh|darwin)"), //
        		new Platform("Linux", "linux"), //
        		new Platform("Wii", "wii"), //
        		new Platform("Playstation", "playstation"), //
        		new Platform("Java", "java") //
        };

        /// <summary>
        /// 支持的平台类型
        /// </summary>
        public static List<Platform> Platforms = new List<Platform> {
                WindowsPhone, //
				Ipad, //
				Ipod, //
				Iphone, //
				Android, //
				GoogleTv, //
				new Platform("htcFlyer", "htc_flyer"), //
				new Platform("Symbian", "symbian(os)?"), //
				new Platform("Blackberry", "blackberry"), //
				new Platform("Windows", "windows"), //
				new Platform("Mac", "(macintosh|darwin)"), //
				new Platform("Linux", "linux"), //
				new Platform("Wii", "wii"), //
				new Platform("Playstation", "playstation"), //
				new Platform("Java", "java") //
		};

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="name"></param>
		/// <param name="regex"></param>
		public Platform(string name, string regex) : base(name, regex)
		{
		}

        /// <summary>
        /// 是否为移动平台
        /// </summary>
        /// <returns></returns>
        public bool IsMobile()
        {
            return MobilePlatforms.Any(x => x.Name == this.Name);
        }

        /// <summary>
        /// 是否为Iphone或者iPod设备
        /// </summary>
        /// <returns></returns>
        public bool IsIPhoneOrIPod()
        {
            return this.Name == Iphone.Name || this.Name == Ipod.Name;
        }

        /// <summary>
        /// 是否为Iphone或者iPod设备
        /// </summary>
        /// <returns></returns>
        public bool IsIPad()
        {
            return this.Name == Ipad.Name;
        }

        /// <summary>
        /// 是否为IOS平台，包括IPhone、IPod、IPad
        /// </summary>
        /// <returns></returns>
        public bool IsIos()
        {
            return IsIPhoneOrIPod() || IsIPad();
        }

        /// <summary>
        /// 是否为Android平台，包括Android和Google TV
        /// </summary>
        /// <returns></returns>
        public bool IsAndroid()
        {
            return this.Name == Android.Name || this.Name == GoogleTv.Name;
        }

        /// <summary>
        /// 是否桌面平台
        /// </summary>
        /// <returns></returns>
        public bool IsDesktop()
        {
            return DesktopPlatforms.Any(x => x.Name == this.Name);
        }
    }
}

