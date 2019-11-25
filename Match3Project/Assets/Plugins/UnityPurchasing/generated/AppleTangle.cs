#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class AppleTangle
    {
        private static byte[] data = System.Convert.FromBase64String("19eOwdDQzMWOw8/Nj8HQ0MzFw8E1PtqsBOcr+3S2l5NrZK/tbrTJcfkHpanct+D2sb7Ucxcrg5vnA3XPppCvpqP1vbOhoV+kpZCjoaFfkL1pudJV/a513/87UoWjGvUv7f2tUdTJxsnDwdTFgMLZgMHO2YDQwdLUlZKRlJCTlvq3rZOVkJKQmZKRlJAVmg1Ur66gMqsRgbaO1HWcrXvCtgsD0TLn8/VhD4/hE1hbQ9BtRgPskCKkG5AiowMAo6KhoqKhopCtpqkRkPhM+qSSLMgTL71+xdNfx/7FHOl41j+TtMUB1zRpjaKjoaChAyKhgOPhkCKhgpCtpqmKJugmV62hoaGtpqmKJugmV62hoaWloKMioaGg/J2Gx4Aqk8pXrSJvfksDj1nzyvvE0MzFgPLPz9SA4+GQvretkJaQlJLSwcPUycPFgNPUwdTFzcXO1NOOkNTIz9LJ1NmRtpC0pqP1pKOzreHQpqP1va6ktqS0i3DJ5zTWqV5Uyy2Qsaaj9aSqs6rh0NDMxYDpzsOOkczFgOnOw46RhpCEpqP1pKuzveHQpKazovXzkbOQsaaj9aSqs6rh0NAioaCmqYom6CZXw8SloZAhUpCKps7EgMPPzsTJ1MnPztOAz8aA1dPFxJWDteu1+b0TNFdWPD5v8Bph+PC/MXu+5/BLpU3+2SSNS5YC9+z1TKiLpqGlpaeioba+yNTU0NOaj4/XwszFgNPUwc7EwdLEgNTF0s3TgMHQzMWA48XS1MnGycPB1MnPzoDh1aWgoyKhr6CQIqGqoiKhoaBEMQmphpCEpqP1pKuzveHQ0MzFgOPF0tQeVNM7TnLEr2vZ75R4Ap5Z2F/LaK89nVOL6Yi6aF5uFRmuef68dmudgMHOxIDDxdLUycbJw8HUyc/OgNCMgMPF0tTJxsnDwdTFgNDPzMnD2dmAwdPT1c3F04DBw8PF0NTBzsPFYMOT11eap4z2S3qvga56GtO57xUXux0z4oSyimevvRbtPP7DaOsgt47gBlfn7d+o/pC/pqP1vYOkuJC25d6/7MvwNuEpZNTCq7Aj4SeTKiHJxsnDwdTJz86A4dXUyM/SydTZkSu5KX5Z68xVpwuCkKJIuJ5Y8Klz3+EIOFlxasY8hMuxcAMbRLuKY7+2kLSmo/Wko7Ot4dDQzMWA8s/P1JY57I3YF00sO3xT1ztS1nLXkO9hgM/GgNTIxYDUyMXOgMHQ0MzJw8GEQktxF9B/r+VBh2pRzdhNRxW3t/LFzMnBzsPFgM/OgNTIydOAw8XSiiboJletoaGlpaCQwpGrkKmmo/WnTN2ZIyvzgHOYZBEfOu+qy1+LXNqQIqHWkK6mo/W9r6GhX6Sko6KhL9MhwGa7+6mPMhJY5OhQwJg+tVUgtItwyec01qleVMstjuAGV+ft35OW+pDCkauQqaaj9aSms6L185GzqP6QIqGxpqP1vYCkIqGokCKhpJB5lt9hJ/V5BzkZkuJbeHXRPt4B8o+QIWOmqIumoaWlp6KikCEWuiETxy+oFIBXawyMgM/QFp+hkCwX42+/JSMluzmd55dSCTvgLox0ETCyeAh83oKVaoV1ea92y3QChIOxVwEM8AoqdXpEXHCpp5cQ1dWB");
        private static int[] order = new int[] { 36,18,52,29,19,39,7,23,24,36,26,27,13,47,56,23,46,39,30,36,29,52,34,43,29,43,35,42,36,56,35,50,34,45,44,40,57,59,50,43,42,46,43,47,48,46,53,50,50,51,57,55,57,56,56,58,59,57,58,59,60 };
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
