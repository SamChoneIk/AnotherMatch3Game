#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("8IcY/1csR6H/8OY4tvxyYDU6VImQIu8sLjicpj1Mavm+EDCflfXWlF5kIFBF5DEknzKxxI0SS/9P0spwwZjcwn0s3QLpru2SewdVjWyKrhpzWUymBtN0vGjIATe7HJzfheEl5k+aVGiUbS+iPDdEzCS88Av9z/yiRsXLxPRGxc7GRsXFxGCiDo0ApgMUDreezRTmPyIdeG78LpMh6atPKETYq6bbr1gtXg0xViScHyqF0zojBLnDv7HmeivXP3vBuBmWAHvNmFAS217INhqEpCoEW5XbDjYdSxMtrxVlfhc1P6zcvMBPTIP/uZlaMiK/G6C9gQ0fca3lUGHlZLUQRWA+YrH0RsXm9MnCze5CjEIzycXFxcHEx4wqad2nLKLHBcbHxcTF");
        private static int[] order = new int[] { 9,4,2,3,6,8,6,10,8,12,13,11,13,13,14 };
        private static int key = 196;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
