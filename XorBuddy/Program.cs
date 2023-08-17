//
// Program.cs
//
// Copyright (c) 2015 Heath Leach
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
using System.Text;

namespace XorBuddy {
	class MainClass {
		public static void Main(string[] args) {
			string keystring = "VnJMZjhORTJTb0VpTEJOeXNtR0V2ZWc1TmpFYkNtZW9pQTRuWm5NNGNtbHFlNktGCg==";
			string[] strings = { 
				"VGhlIGNhdCB3ZW50IGhlcmUgYW5kIHRoZXJl",
				"QW5kIHRoZSBtb29uIHNwdW4gcm91bmQgbGlrZSBhIHRvcCw=",
				"QW5kIHRoZSBuZWFyZXN0IGtpbiBvZiB0aGUgbW9vbiw=",
				"VGhlIGNyZWVwaW5nIGNhdCwgbG9va2VkIHVwLg==",
				"QmxhY2sgTWlubmFsb3VzaGUgc3RhcmVkIGF0IHRoZSBtb29uLA==",
				"Rm9yLCB3YW5kZXIgYW5kIHdhaWwgYXMgaGUgd291bGQs",
				"VGhlIHB1cmUgY29sZCBsaWdodCBpbiB0aGUgc2t5",
				"VHJvdWJsZWQgaGlzIGFuaW1hbCBibG9vZC4=",
				"TWlubmFsb3VzaGUgcnVucyBpbiB0aGUgZ3Jhc3M=",
				"TGlmdGluZyBoaXMgZGVsaWNhdGUgZmVldC4=",
				"RG8geW91IGRhbmNlLCBNaW5uYWxvdXNoZSwgZG8geW91IGRhbmNlPw==",
				"V2hlbiB0d28gY2xvc2Uga2luZHJlZCBtZWV0LA==",
				"V2hhdCBiZXR0ZXIgdGhhbiBjYWxsIGEgZGFuY2U/",
				"TWF5YmUgdGhlIG1vb24gbWF5IGxlYXJuLA==",
				"VGlyZWQgb2YgdGhhdCBjb3VydGx5IGZhc2hpb24s",
				"QSBuZXcgZGFuY2UgdHVybi4=",
				"TWlubmFsb3VzaGUgY3JlZXBzIHRocm91Z2ggdGhlIGdyYXNz",
				"RnJvbSBtb29ubGl0IHBsYWNlIHRvIHBsYWNlLA==",
				"VGhlIHNhY3JlZCBtb29uIG92ZXJoZWFk",
				"SGFzIHRha2VuIGEgbmV3IHBoYXNlLg==",
				"RG9lcyBNaW5uYWxvdXNoZSBrbm93IHRoYXQgaGlzIHB1cGlscw==",
				"V2lsbCBwYXNzIGZyb20gY2hhbmdlIHRvIGNoYW5nZSw=",
				"QW5kIHRoYXQgZnJvbSByb3VuZCB0byBjcmVzY2VudCw=",
				"RnJvbSBjcmVzY2VudCB0byByb3VuZCB0aGV5IHJhbmdlPw==",
				"TWlubmFsb3VzaGUgY3JlZXBzIHRocm91Z2ggdGhlIGdyYXNz",
				"QWxvbmUsIGltcG9ydGFudCBhbmQgd2lzZSw=",
				"QW5kIGxpZnRzIHRvIHRoZSBjaGFuZ2luZyBtb29u",
				"SGlzIGNoYW5naW5nIGV5ZXMu" };

			var xb = new XorBuddy();
			byte[] key = Convert.FromBase64String(keystring);
			foreach (string s in strings) {
				var sample = Utility.XorBytes(Convert.FromBase64String(s), key);
				xb.AddSample(sample);
			}
			xb.RunUI();			


		}
		
	}
}
