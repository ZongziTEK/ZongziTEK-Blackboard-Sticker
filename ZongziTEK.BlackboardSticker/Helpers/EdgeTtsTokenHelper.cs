using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ZongziTEK.BlackboardSticker.Helpers
{
    internal static class EdgeTtsTokenHelper
    {
        private const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";
        private const long WindowsEpochSeconds = 11644473600L;

        private static readonly uint[] Sha256Initial =
        {
            0x6A09E667, 0xBB67AE85, 0x3C6EF372, 0xA54FF53A,
            0x510E527F, 0x9B05688C, 0x1F83D9AB, 0x5BE0CD19
        };

        private static readonly uint[] Sha256State =
        {
            0xFD9FBEA2, 0x91243CE7, 0xBE499094, 0xCD2E1E0A,
            0x6C006F49, 0x04BD35AB, 0xFEB0A7E4, 0x03EFFBD2
        };

        private static readonly uint[] Sha256Schedule =
        {
            0xEB24BF9E, 0x1558691F, 0xA0C09EE2, 0x0CFA32BA,
            0xA622EE8C, 0xD1DD9491, 0xC0DD25A0, 0xF9E70AB6,
            0xF0C31573, 0x7058CE4E, 0x7188E606, 0x00E5AB33,
            0xD258B060, 0x72766564
        };

        private static readonly uint[] Sha256RoundConstants =
        {
            0x428A2F98, 0x71374491, 0xB5C0FBCF, 0xE9B5DBA5,
            0x3956C25B, 0x59F111F1, 0x923F82A4, 0xAB1C5ED5,
            0xD807AA98, 0x12835B01, 0x243185BE, 0x550C7DC3,
            0x72BE5D74, 0x80DEB1FE, 0x9BDC06A7, 0xC19BF174,
            0xE49B69C1, 0xEFBE4786, 0x0FC19DC6, 0x240CA1CC,
            0x2DE92C6F, 0x4A7484AA, 0x5CB0A9DC, 0x76F988DA,
            0x983E5152, 0xA831C66D, 0xB00327C8, 0xBF597FC7,
            0xC6E00BF3, 0xD5A79147, 0x06CA6351, 0x14292967,
            0x27B70A85, 0x2E1B2138, 0x4D2C6DFC, 0x53380D13,
            0x650A7354, 0x766A0ABB, 0x81C2C92E, 0x92722C85,
            0xA2BFE8A1, 0xA81A664B, 0xC24B8B70, 0xC76C51A3,
            0xD192E819, 0xD6990624, 0xF40E3585, 0x106AA070,
            0x19A4C116, 0x1E376C08, 0x2748774C, 0x34B0BCB5,
            0x391C0CB3, 0x4ED8AA4A, 0x5B9CCA4F, 0x682E6FF3,
            0x748F82EE, 0x78A5636F, 0x84C87814, 0x8CC70208,
            0x90BEFFFA, 0xA4506CEB, 0xBEF9A3F7, 0xC67178F2
        };

        private const uint Sha256Adjustment = 0xC9DC06A7;
        private const uint Sha256Sigma0Adjustment = 0x059C0B80;
        private const uint Sha256Sigma1Adjustment = 0x000B92C0;

        private static readonly uint[] Sha256BlockConstants;

        private static readonly uint[] Cs64Coefficients =
        {
            0xE82E91B1, 0x44DE24C6, 0x3BEB65BC, 0xC89C5933,
            0x8D1EC211, 0x314EF5D5, 0xE92491B1, 0xF0A5B9D5,
            0x06064CF3, 0xDA79FA60, 0xBED20560, 0x89F9C9F8,
            0x35FA38C6, 0xCB49E3C6
        };

        private static readonly uint[] Cs64Offsets =
        {
            0xB32700D0, 0x88DFF51D, 0x29514038, 0xB328CBDF,
            0xD405B4D5, 0x26E55F3C, 0x607801C6, 0x82B2C1FB,
            0xFA51A3DA, 0x207CC464, 0x3C3C74F3, 0xD2D14B61,
            0xBB04A2C1, 0xC5626167
        };

        private const uint Cs64Factor = 0x82893933;
        private const uint Cs64Adjustment0 = 0xF2F90B1A;
        private const uint Cs64Adjustment1 = 0x0B1A0000;

        static EdgeTtsTokenHelper()
        {
            Sha256BlockConstants = new uint[64];
            Array.Copy(Sha256RoundConstants, 0, Sha256BlockConstants, 0, 14);
            Sha256BlockConstants[14] = Sha256Adjustment;
            Array.Copy(Sha256RoundConstants, 15, Sha256BlockConstants, 15, 49);
        }

        public static string Generate(DateTimeOffset? utcNow = null)
        {
            long ticks = (utcNow ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds() + WindowsEpochSeconds;
            ticks -= ticks % 300;
            ticks *= 10_000_000L;

            byte[] data = Encoding.ASCII.GetBytes(
                ticks.ToString(CultureInfo.InvariantCulture) + TrustedClientToken);

            byte[] digest = Sha256Digest(data);
            uint[] cs64 = Cs64Digest(data, digest);

            return ToHexLittleEndian(cs64[0]) + ToHexLittleEndian(cs64[1]) + "-" + ToHex(digest);
        }

        private static uint Rotr(uint value, int bits)
        {
            return (value >> bits) | (value << (32 - bits));
        }

        private static uint[] Sha256Rounds(uint[] state, List<uint> words, int firstRound = 0)
        {
            uint a = state[0], b = state[1], c = state[2], d = state[3];
            uint e = state[4], f = state[5], g = state[6], h = state[7];

            uint[] constants = firstRound == 14 ? Sha256BlockConstants : Sha256RoundConstants;

            for (int index = firstRound; index < 64; index++)
            {
                uint sigma1 = Rotr(e, 6) ^ Rotr(e, 11) ^ Rotr(e, 25);
                uint choose = (e & f) ^ (~e & g);
                uint t1 = unchecked(h + sigma1 + choose + constants[index] + words[index]);

                uint sigma0 = Rotr(a, 2) ^ Rotr(a, 13) ^ Rotr(a, 22);
                uint majority = (a & b) ^ (a & c) ^ (b & c);
                uint t2 = unchecked(sigma0 + majority);

                h = g;
                g = f;
                f = e;
                e = unchecked(d + t1);
                d = c;
                c = b;
                b = a;
                a = unchecked(t1 + t2);
            }

            return new[] { a, b, c, d, e, f, g, h };
        }

        private static void ExpandSha256Schedule(List<uint> words, int firstWord = 16)
        {
            for (int index = firstWord; index < 64; index++)
            {
                uint x = words[index - 2];
                uint y = words[index - 15];

                uint sigma1 = (x >> 17) ^ (x << 15) ^ (x >> 19) ^ (x << 13) ^ (x >> 10);
                uint sigma0 = (y >> 7) ^ (y << 25) ^ (y >> 18) ^ (y << 14) ^ (y >> 3);

                words.Add(unchecked(sigma1 + words[index - 7] + sigma0 + words[index - 16]));
            }
        }

        private static uint[] Sha256Init(byte[] data)
        {
            var words = new List<uint>(new uint[14])
            {
                ReadBigEndianUInt32(data, 0, 3),
                ReadBigEndianUInt32(data, 3, 4)
            };

            for (int index = 16; index < 30; index++)
            {
                uint value = Sha256Schedule[index - 16];

                if (index - 2 >= 14)
                {
                    value = unchecked(value + (index == 16
                        ? (Sigma1(words[index - 2]) ^ Sha256Sigma1Adjustment)
                        : Sigma1(words[index - 2])));
                }
                if (index - 7 >= 14)
                {
                    value = unchecked(value + words[index - 7]);
                }
                if (index - 15 >= 14)
                {
                    value = unchecked(value + (index == 29
                        ? (Sigma0(words[index - 15]) ^ Sha256Sigma0Adjustment)
                        : Sigma0(words[index - 15])));
                }

                words.Add(value);
            }

            words.Add(unchecked(
                Sigma1(words[28])
                + words[23]
                + Sigma0(words[15])
                + words[14]
                + Sha256Adjustment
                - Sha256RoundConstants[14]));

            ExpandSha256Schedule(words, 31);

            uint[] working = Sha256Rounds(Sha256State, words, 14);
            var result = new uint[8];
            for (int index = 0; index < 8; index++)
            {
                result[index] = unchecked(Sha256Initial[index] + working[index]);
            }

            return result;
        }

        private static byte[] Sha256Digest(byte[] data)
        {
            if (data.Length < 7)
            {
                throw new ArgumentException("Token salt must contain at least seven bytes.", nameof(data));
            }

            int length = 57 + data.Length;
            var padded = new List<byte>(data.Length + 64);
            for (int index = 7; index < data.Length; index++)
            {
                padded.Add(data[index]);
            }

            padded.Add(0x80);
            int paddingLength = ((55 - length) % 64 + 64) % 64;
            for (int index = 0; index < paddingLength; index++)
            {
                padded.Add(0);
            }

            ulong bitLength = (ulong)length * 8;
            for (int shift = 56; shift >= 0; shift -= 8)
            {
                padded.Add((byte)(bitLength >> shift));
            }

            uint[] state = Sha256Init(data);
            for (int offset = 0; offset < padded.Count; offset += 64)
            {
                var words = new List<uint>(16);
                for (int index = 0; index < 16; index++)
                {
                    int position = offset + index * 4;
                    words.Add((uint)(
                        (padded[position] << 24)
                        | (padded[position + 1] << 16)
                        | (padded[position + 2] << 8)
                        | padded[position + 3]));
                }

                ExpandSha256Schedule(words);

                uint[] working = Sha256Rounds(state, words);
                for (int index = 0; index < 8; index++)
                {
                    state[index] = unchecked(state[index] + working[index]);
                }
            }

            var digest = new byte[32];
            for (int index = 0; index < 8; index++)
            {
                WriteBigEndianUInt32(digest, index * 4, state[index]);
            }

            return digest;
        }

        private static uint[] Cs64Digest(byte[] data, byte[] digest)
        {
            uint keyA = ReadLittleEndianUInt32(digest, 0) | 1;
            uint keyB = ReadLittleEndianUInt32(digest, 4) | 1;

            uint[] factors =
            {
                unchecked(keyA * Cs64Factor),
                unchecked(keyB * Cs64Factor)
            };

            var terms = new List<uint>(Cs64Coefficients.Length);
            for (int index = 0; index < Cs64Coefficients.Length; index++)
            {
                terms.Add(unchecked(Cs64Coefficients[index] * factors[index % 2]));
            }

            var offsets = new List<uint>(Cs64Offsets);

            var wordData = new byte[data.Length + 1];
            Array.Copy(data, 0, wordData, 1, data.Length);
            List<uint> words = WordsLittleEndianEven(wordData);

            for (int index = 0; index < words.Count; index++)
            {
                uint word = words[index];
                uint term = unchecked(word * (index % 2 == 0 ? keyA : keyB));
                uint offset = unchecked(Rol16(word) * (index % 2 == 0 ? 0xEF0569FB : 0xC31713DB));

                if (index == 0)
                {
                    term = unchecked(term + Cs64Adjustment0 * factors[0]);
                    offset = unchecked(offset + Cs64Adjustment1);
                }

                terms.Add(term);
                offsets.Add(offset);
            }

            uint swapAcc = 0, swapLast = 0, state = 0, acc = 0;

            for (int index = 0; index < terms.Count; index += 2)
            {
                uint x = terms[index];
                uint y = terms[index + 1];

                uint t = Rol16(unchecked(offsets[index] + x));
                uint q = unchecked(t * 0x76F50F34);
                swapAcc = unchecked(swapAcc + (unchecked(Rol16(q) * 0xC3EFEA97) + q));

                t = Rol16(unchecked(offsets[index + 1] + y));
                q = unchecked(t * 0x1881CE3C);
                swapLast = unchecked(Rol16(q) * 0x35BD1EC9 + q);
                swapAcc = unchecked(swapAcc + swapLast);

                state = unchecked(state * keyA + x);
                state = unchecked(Rol16(state) * 0xCF98B111);
                state = unchecked(Rol16(state) * 0x87085B9F);
                state = unchecked(Rol16(state) * 0x12CEB96D);
                state = unchecked(Rol16(state) * 0x257E1D83);
                acc = unchecked(acc + state);

                t = unchecked(state * keyB + y);
                t = unchecked(Rol16(t) * 0xA27416F5);
                t = unchecked(Rol16(t) * 0xD38396FF);
                state = unchecked(Rol16(unchecked(Rol16(t) * 0x7C932B89)) * 0xBFA49F69);
                acc = unchecked(acc + state);
            }

            return new[] { swapLast ^ state, swapAcc ^ acc };
        }

        private static uint Rol16(uint value)
        {
            return (value << 16) | (value >> 16);
        }

        private static List<uint> WordsLittleEndianEven(byte[] data)
        {
            int paddedLength = data.Length + ((4 - data.Length % 4) % 4);
            byte[] padded = data;
            if (paddedLength != data.Length)
            {
                padded = new byte[paddedLength];
                Array.Copy(data, padded, data.Length);
            }

            var words = new List<uint>(paddedLength / 4);
            for (int index = 0; index < paddedLength; index += 4)
            {
                words.Add(ReadLittleEndianUInt32(padded, index));
            }

            if (words.Count % 2 != 0)
            {
                words.Add(0);
            }

            return words;
        }

        private static uint Sigma0(uint value)
        {
            return (value >> 7) ^ (value << 25) ^ (value >> 18) ^ (value << 14) ^ (value >> 3);
        }

        private static uint Sigma1(uint value)
        {
            return (value >> 17) ^ (value << 15) ^ (value >> 19) ^ (value << 13) ^ (value >> 10);
        }

        private static uint ReadBigEndianUInt32(byte[] data, int offset, int count)
        {
            uint value = 0;
            for (int index = 0; index < count; index++)
            {
                value = (value << 8) | data[offset + index];
            }
            return value;
        }

        private static uint ReadLittleEndianUInt32(byte[] data, int offset)
        {
            return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
        }

        private static void WriteBigEndianUInt32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }

        private static string ToHex(byte[] data)
        {
            var builder = new StringBuilder(data.Length * 2);
            foreach (byte value in data)
            {
                builder.Append(value.ToString("X2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        private static string ToHexLittleEndian(uint value)
        {
            return ((byte)value).ToString("X2", CultureInfo.InvariantCulture)
                + ((byte)(value >> 8)).ToString("X2", CultureInfo.InvariantCulture)
                + ((byte)(value >> 16)).ToString("X2", CultureInfo.InvariantCulture)
                + ((byte)(value >> 24)).ToString("X2", CultureInfo.InvariantCulture);
        }
    }
}
