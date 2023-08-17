//
// Utility.cs
//
// Copyright (c) 2014 Heath Leach
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace XorBuddy {
	public class Utility {
		// Like SubString, but for a byte[]
		// Adjust to end of line if not specified
		public static byte[] SubBuffer(byte[] buf, int offset, int count = -1) {
			if (count == -1)
				count = buf.Length - offset;
			byte[] b = new byte[count];
			Buffer.BlockCopy(buf, offset, b, 0, count);
			return b;
		}

		// Concatinate multiple byte[]s
		public static byte[] ConcatBuffer(params byte[][] buffers) {
			var output = new System.IO.MemoryStream();
			foreach (byte[] b in buffers)
				output.Write(b, 0, b.Length);
			return output.ToArray();
		}

		public static string ByteToEscapedStr(byte b, bool nolinefeed = false) {
			if (((b == 0x0a) && !nolinefeed) | ((b > 31) & (b < 127)))
				return String.Format("{0}", (char) b);
			else if (b == (byte) '\\')
				return (@"\\");
			else return (String.Format("\\0x{0:x2}", b));
		}

		// Convert a byte array to a string with unprintables in escaped hex
		public static string BytesToEscapedStr(byte[] buf, bool nolinefeed = false) {
			var sb = new System.Text.StringBuilder();
			foreach (var b in buf)
				sb.Append(ByteToEscapedStr(b, nolinefeed));	
			return sb.ToString();
		}

		// Convert a byte array to a string of hex digits, Formatted to # of columns, count bytes wide
		public static string BytesToHexStrF(byte[] buf, int count = 16, int columns = 1, bool nolinefeed = false) {
			const string HEX_DIGITS = "0123456789abcdef";
			var ret = new System.Text.StringBuilder();
			int i = 1;
			bool appended = false;
			foreach (var b in buf) {
				appended = false;
				ret.Append(HEX_DIGITS[(b & 0xF0) >> 4]);
				ret.Append(HEX_DIGITS[b & 0x0F]);
				if ((i % (count*columns)) == 0) {
					if (!nolinefeed)
						ret.Append("\n");
					appended = true;
				}
				else if ((i % count) == 0)
					ret.Append(" ");
				i++;
			}
			if ((!appended) && (!nolinefeed))
				ret.Append("\n");
			return ret.ToString();
		}

		// Xor bytes of b1 with b2, circling through b2 as many times as necessary
		public static byte[] XorBytes(byte[] b1, byte[] b2) {
			byte[] rb = new byte[b1.Length];
			for (int i = 0; i < b1.Length; i++)
				rb[i] = (byte) (b1[i] ^ b2[i % b2.Length]);
			return rb;
		}
	}
}

