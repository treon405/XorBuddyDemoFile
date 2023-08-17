//
// XorBuddy.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace XorBuddy {
	public class XorBuddy : IEnumerator, IEnumerable {

		private const ConsoleColor HIGHLIGHT_COLD = ConsoleColor.DarkBlue;
		private const ConsoleColor HIGHLIGHT_HOT = ConsoleColor.DarkRed;
		private const char UNPRINTABLE_CHAR = '.';
		private const string FREQ_STRING = " etaonrishdlfcmugpywbvkxjqz";
		private readonly int[] FREQUENCIES = 
			new int[] {1350, 1300, 1050, 810, 790, 710, 680, 630, 610, 520, 380, 340, 290, 270, 
			250, 240, 200, 190, 190, 150, 140, 90, 40, 15, 13, 11, 7};

		private const int SHOW_NONE = 0;
		private const int SHOW_END = 1;
		private const int SHOW_LETTER = 2;
		private const int SHOW_ATTEMPTS = 3;

		List<byte[]> samples;
		int row, col, wd, ht;
		bool running, insert;
		byte[] key;
		int position = -1;
		int trim = 0;

		public XorBuddy() {
			samples = new List<byte[]>();
			row = 0;
			col = 0;
			key = new byte[0];
			insert = false;
		}

		public string this[int ind] {
			get { return this.GetString(ind); }
		}

		public void AddSample(byte[] sample) {
			samples.Add(sample);
		}

		public void AddFoldedSample(byte[] sample, int blocklength) {
			if (sample.Length < 1)
				return;
			samples = new List<byte[]>();
			int blocks = ((sample.Length - 1) / blocklength); 
			for (int i = 0; i <= blocks; i++) 
				AddSample(Utility.SubBuffer(sample, i * blocklength, i == blocks ? -1 : blocklength));
		}

		public byte[] GetSample(int ind) {
			return samples[ind];
		}

		public string GetString(int ind) {
			byte[] buf = new byte[samples[ind].Length];
			Buffer.BlockCopy(key, 0, buf, 0, key.Length < buf.Length ? key.Length : buf.Length);
			return Encoding.ASCII.GetString(Utility.XorBytes(samples[ind], buf));
		}

		private char GetUChar(byte b) {
			if ((b > 31) && (b < 127))
				return ((char) b);
			else return UNPRINTABLE_CHAR;
		}

		private string GetKeyedString(int ind) {
			var sb = new StringBuilder();
			for (int i = 0; i < samples[ind].Length; i++) {
				if ((i >= col) && (i < (col + key.Length))) {
					sb.Append(GetUChar((byte) (samples[ind][i] ^ key[i - col])));
				} else sb.Append(GetUChar(samples[ind][i]));
			}
			return sb.ToString();
		}

		private void WriteLineColorString(string s, ConsoleColor color, int ind, int count) {
			if (ind >= s.Length) {
				Console.WriteLine(s);
				return;
			}
			Console.Write(s.Substring(0, ind));
			Console.BackgroundColor = color;

			if ((ind + count) >= s.Length) {
				Console.WriteLine(s.Substring(ind));
				Console.ResetColor();
				return;
			}
			Console.Write(s.Substring(ind, count));
			Console.ResetColor();
			Console.WriteLine(s.Substring(ind+count));
		}

		private void ClearToEOL() {
			int line = Console.CursorTop;
			if ((line < 0) || (line > Console.WindowHeight))
				return;
			Console.SetCursorPosition(0, line);
			Console.Write(new string(' ', Console.WindowWidth)); 
			Console.SetCursorPosition(0, line);
		}

		private string GetSelectedText() {
			return Encoding.ASCII.GetString(GetSelectedBytes());
		}

		private byte[] GetSelectedBytes() {
			byte[] block;
			if (col > samples[row].Length)
				return new byte[0];
			if ((col + key.Length) > samples[row].Length)
				block = Utility.SubBuffer(samples[row], col);
			else
				block = Utility.SubBuffer(samples[row], col, key.Length);
			return Utility.XorBytes(block, key);
		}

		private long CountKeyed() {
			long counter = 0;
			for (int i = 0; i < samples.Count; i++) {
				var sample = samples[i];
				for (int j = col; j < (col + key.Length); j++) 
					if (j < sample.Length) {
						byte b = (byte) (sample[j] ^ key[j - col]);
						int ind = FREQ_STRING.IndexOf(Char.ToLower((char) b));
						if (ind >= 0)
							counter += FREQUENCIES[ind];
					}
			}
			return counter;
		}

		private long CountColumn(int column) {
			long counter = 0;
			for (int i = 0; i < samples.Count; i++) {
				var sample = samples[i];
				if (column < sample.Length) {
					byte b = (byte) (sample[column] ^ key[column - col]);
					int ind = FREQ_STRING.IndexOf(Char.ToLower((char) b));
					if (ind >= 0)
						counter += FREQUENCIES[ind];
				}
			}
			return counter;
		}

		private void FindFirstOccurance(int show = SHOW_NONE) {
			int longest = 0;
			foreach (var sample in samples)
				if (sample.Length > longest)
					longest = sample.Length;
			for (int i = 0; i < samples.Count; i++) {
				MoveSelectionToRow(i);
				for (int j = 0; j < Math.Min(samples[i].Length - key.Length + 1, longest - key.Length - trim); j++) {
					MoveSelectionToColumn(j);
					if (show == SHOW_ATTEMPTS)
						Display();
					if (Detect())
						return;
				}
			}
			if (show == SHOW_END)
				Display();
		}

		public byte[] AutoDetect(bool forceDetect = false, int show = SHOW_NONE) {
			key = new byte[0];
			col = 0;
			int length = 0;
			foreach (byte[] sample in samples)
				if (sample.Length > length)
					length = sample.Length;

			for (int i = 0; i < length; i++) {
				long bestCount = 0;
				byte bestByte = 0;
				key = Utility.ConcatBuffer(key, new byte[1]);
				for (int j = 0; j < 256; j++) {
					key[i] = (byte) j;
					long c = CountColumn(key.Length - 1);
					if (forceDetect)
					if (!Detect())
						c = -1;
					if (c > bestCount) {
						bestCount = c;
						bestByte = (byte) j;
					}
					if (show == SHOW_ATTEMPTS)
						Display();
				}
				key[i] = bestByte;
				if (show == SHOW_LETTER)
					Display();
			}
			if (show == SHOW_END)
				Display();
			return key;
		}

		private bool Detect() {
			if (key.Length == 0)
				return false;
			for (int i = 0; i < samples.Count; i++) {
				var sample = samples[i];
				for (int j = col; j < (col + key.Length); j++) 
					if (j < sample.Length) {
						byte b = (byte) (sample[j] ^ key[j - col]);
						if (!Char.IsLetterOrDigit((char) b) && 
							!Char.IsPunctuation((char) b) && 
							!((char) b).Equals(' ')	&& !(b == 0x0A) && !(b == 0x0D))
							return false;
					}
			}
			return true;
		}

		private void Display() {
			ConsoleColor color = HIGHLIGHT_COLD;
			if (Detect())
				color = HIGHLIGHT_HOT;
			if ((wd != Console.WindowWidth) || (ht != Console.WindowHeight)) {
				wd = Console.WindowWidth;
				ht = Console.WindowHeight;
				Console.Clear();				
			} else 
				Console.SetCursorPosition(0, 0);
			if (Console.WindowHeight < (samples.Count + 3))
				Console.Clear();
			for (int i = 0; i < samples.Count; i++) {
				var sample = samples[i];
				if (row == i) {
					Console.BackgroundColor = color;
					Console.Write("{0:d2}: ", i);
					Console.WriteLine(GetKeyedString(i));
					Console.ResetColor();			
				}
				else {
					Console.Write("{0:d2}: ", i);
					WriteLineColorString(GetKeyedString(i), color, col, key.Length);
				}
			}
			Console.WriteLine();
			Console.WriteLine("Type to enter test plain text. Use Page and Arrow Keys to move.");
			ClearToEOL();
			Console.WriteLine("Press F9 to search current selection. F2/F3 to adjust trim ({0}).", trim);
			ClearToEOL();
			Console.WriteLine("Press Insert to toggle insertion ({0}), End to Stop and F10 to guess.", insert ? "on" : "off");
			ClearToEOL();
			Console.Write("({0}): ", Utility.BytesToEscapedStr(GetSelectedBytes(), true));
		}


		private void Command() {
			var ki = Console.ReadKey(true);
			switch (ki.Key) {
				case ConsoleKey.PageUp:
					MoveToPrevRow();
					break;	
				case ConsoleKey.PageDown:
					MoveToNextRow();
					break;	
				case ConsoleKey.UpArrow:
					MoveSelectionUp();
					break;	
				case ConsoleKey.DownArrow:
					MoveSelectionDown();
					break;	
				case ConsoleKey.LeftArrow:
					MoveSelectionLeft();
					break;	
				case ConsoleKey.RightArrow:
					MoveSelectionRight();
					break;	
				case ConsoleKey.Backspace:
				case ConsoleKey.Delete:
					if (key.Length > 0) {
						if (insert) {
							col++;
							key = Utility.SubBuffer(key, 1, key.Length-1);
						} else key = Utility.SubBuffer(key, 0, key.Length-1);
					}
					break;
				case ConsoleKey.F2:
					trim ++;
					break;
				case ConsoleKey.F3:
					trim = Math.Max(0, trim - 1);
					break;
				case ConsoleKey.Insert:
					insert = !insert;
					break;
				case ConsoleKey.F9:
					FindFirstOccurance(SHOW_ATTEMPTS);
					break;
				case ConsoleKey.F10:
					AutoDetect(true, SHOW_LETTER);
					break;
				case ConsoleKey.End:
					running = false;
					break;
			}
			if (insert) {
				if ((((int) ki.KeyChar) != 0) && (col > 0)) {
					col--;
					var b = new byte[] {(byte) (samples[row][col] ^ ki.KeyChar)};
					key = Utility.ConcatBuffer(b, key);
				}
			} else if ((((int) ki.KeyChar) != 0) && ((col + key.Length) < samples[row].Length)) {
				var b = new byte[] {(byte) (samples[row][col + key.Length] ^ ki.KeyChar)};
				key = Utility.ConcatBuffer(key, b);
			}
		}

		private bool MoveToNextRow() {
			if (!(row < (samples.Count - 1)))
				return false;
			row++;
			return true;
		}

		private bool MoveToPrevRow() {
			if (row <= 0)
				return false;
			row--;
			return true;
		}

		private bool MoveSelectionUp() {
			if (row > 0) {
				byte[] b = GetSelectedBytes();
				row--;
				if (col >= (samples[row].Length - key.Length))
					col = Math.Max(0, samples[row].Length - key.Length);
				byte[] p;
				if ((col + key.Length) > samples[row].Length)
					p = Utility.SubBuffer(samples[row], col);
				else
					p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		private bool MoveSelectionDown() {
			if (row < (samples.Count - 1)) {
				byte[] b = GetSelectedBytes();
				row++;
				if (col >= (samples[row].Length - key.Length))
					col = Math.Max(0, samples[row].Length - key.Length);
				byte[] p;
				if ((col + key.Length) > samples[row].Length)
					p = Utility.SubBuffer(samples[row], col);
				else
					p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		private bool MoveSelectionLeft() {
			if (col > 0) {
				byte[] b = GetSelectedBytes();
				col--;
				byte[] p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		private bool MoveSelectionRight() {
			if (col < (samples[row].Length - key.Length)) {
				byte[] b = GetSelectedBytes();
				col++;
				byte[] p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		private bool MoveSelectionToColumn(int toColumn) {
			if ((toColumn <= (samples[row].Length - key.Length)) && (toColumn >= 0)){
				byte[] b = GetSelectedBytes();
				col = toColumn;
				byte[] p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		private bool MoveSelectionToRow(int toRow) {
			if ((toRow <= (samples.Count - 1)) && (toRow >= 0)){
				byte[] b = GetSelectedBytes();
				row = toRow;
				if (col >= (samples[row].Length - key.Length))
					col = Math.Max(0, samples[row].Length - key.Length);
				byte[] p;
				if ((col + key.Length) > samples[row].Length)
					p = Utility.SubBuffer(samples[row], col);
				else
					p = Utility.SubBuffer(samples[row], col, key.Length);
				key = Utility.XorBytes(b, p);
				return true;
			}
			return false;
		}

		public void RunUI() {
			Console.CursorVisible = false;
			running = true;
			while(running) {
				Display();
				Command();
			}
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Your key is:");
			Console.WriteLine(Utility.BytesToHexStrF(key, 8, 4));
			Console.CursorVisible = true;
		}

		public IEnumerator GetEnumerator() {
			return (IEnumerator) this;
		}

		public bool MoveNext() {
			position++;
			return (position < samples.Count);
		}

		public void Reset() {
			position = 0;
		}

		public object Current {
			get {
				return GetString(position);
			}
		}
	}

}

