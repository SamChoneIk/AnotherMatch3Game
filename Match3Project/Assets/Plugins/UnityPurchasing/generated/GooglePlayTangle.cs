#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("K/4wDPAJS8ZYUyCoQNiUb5mrmMb0RotISlz4wlkoDp3adFT78ZGy8GDdp9vVgh5Ps1sfpdx98mQfqfw0kCKhgpCtpqmKJugmV62hoaGloKMXPSjCYrcQ2AysZVPfePi74YVBgnBq0/qpcIJbRnkcCphK90WNzytMIqGvoJAioaqiIqGhoATGaulkwmeU43ybM0gjxZuUglzSmBYEUV4w7X/E2eVpexXJgTQFgQDRdCEEWgbVcQEac1FbyLjYpCso55vd/T5WRtul/LimGUi5Zo3KifYfYzHpCO7KfjoARDQhgFVA+1bVoOl2L5srtq4UILzPwr/LPEk6aVUyQPh7TuG3Xkd2vzqsUn7gwE5gP/G/alJ5L3dJy+hODbnDSMajYaKjoaCh");
        private static int[] order = new int[] { 8,4,12,8,6,10,6,9,9,11,11,12,12,13,14 };
        private static int key = 160;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
