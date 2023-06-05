using System;
using System.Text;
using SRQ;

namespace SRQ
{

    public class Reader
    {
        private int i = 0;
        private readonly byte[] data;

        public Reader(byte[] data)
        {
            this.data = data;
        }

        public int Int32()
        {
            int result = BitConverter.ToInt32(data, i);
            i += 4;
            return result;
        }

        public string ReadString(bool canBeUndefined = false)
        {
            int ifString = Int32();
            if (ifString != 0)
            {
                int strLen = Int32();
                string str = Encoding.Unicode.GetString(data, i, strLen * 2);
                i += strLen * 2;
                return str;
            }
            else
            {
                return canBeUndefined ? null : string.Empty;
            }
        }

        public byte Byte()
        {
            return data[i++];
        }

        public void DWordFlag(int? expected = null)
        {
            int val = Int32();
            if (val != expected)
            {
                throw new Exception($"Expecting {expected}, but got {val} at position {i - 4}");
            }
        }

        public double Float64()
        {
            double val = BitConverter.ToDouble(data, i);
            i += 8;
            return val;
        }

        public void Seek(int n)
        {
            i += n;
        }

        public string IsNotEnd()
        {
            if (data.Length == i)
            {
                return null;
            }
            else
            {
                return $"Not an end! We are at 0x{i:x}, file length=0x{data.Length:x}, left=0x{data.Length - i:x}";
            }
        }

        public void DebugShowHex(int n = 300)
        {
            Console.WriteLine($"Data at 0x{i:x}\n");
            string s = string.Empty;
            for (int j = 0; j < n; j++)
            {
                s += $"{data[i + j]:x2}:";
                if (j % 16 == 15)
                {
                    s += "\n";
                }
            }
            Console.WriteLine(s);
        }
    }
}