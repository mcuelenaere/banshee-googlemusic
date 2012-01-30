using System.Net;
using Banshee.WebBrowser;
using Banshee.WebSource;

namespace Banshee.GoogleMusic
{
	public class LoginWidget : WebView
	{
		private static readonly Cookie[] REQUIRED_COOKIES = new Cookie[] {
			new Cookie("SID", "", "/", ".google.com"),
			new Cookie("HSID", "", "/", ".google.com"),
			new Cookie("SSID", "", "/", ".google.com"),
			new Cookie("APISID", "", "/", ".google.com"),
			new Cookie("SAPISID", "", "/", ".google.com"),
			new Cookie("sjsaid", "", "/", "music.google.com"),
			new Cookie("sjpref", "", "/music", "music.google.com"),
			new Cookie("xt", "", "/music", "music.google.com"),
		};
		
		public LoginWidget()
		{
			OssiferSession.CookieChanged += (oldCookie, newCookie) => CookiesChanged();
			
			FullReload();
		}
		
		public delegate void UserLoggedInHandler(Cookie[] cookies);
		public event UserLoggedInHandler UserLoggedIn;
		
		private static Cookie fromOssiferCookie(OssiferCookie cookie) {
			Cookie ret = new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain);
			ret.HttpOnly = cookie.HttpOnly;
			ret.Secure = cookie.Secure;
			return ret;
		}
		
		private void CookiesChanged()
		{
			bool allCookiesAreAvailable = true;
			foreach (Cookie cookie in REQUIRED_COOKIES) {
				OssiferCookie ossiferCookie = OssiferSession.GetCookie(cookie.Name, cookie.Domain, cookie.Path);
				if (ossiferCookie == null || string.IsNullOrEmpty(ossiferCookie.Value))
					allCookiesAreAvailable = false;
			}
			
			if (allCookiesAreAvailable && UserLoggedIn != null) {
				Cookie[] cookies = new Cookie[REQUIRED_COOKIES.Length];
				for (int i=0; i < REQUIRED_COOKIES.Length; i++) {
					Cookie requiredCookie = REQUIRED_COOKIES[i];
					cookies[i] = fromOssiferCookie(OssiferSession.GetCookie(requiredCookie.Name, requiredCookie.Domain, requiredCookie.Path));
				}
				UserLoggedIn(cookies);
			}
		}
		
		public override void GoHome()
		{
			LoadUri("http://music.google.com");
		}
	}
}

