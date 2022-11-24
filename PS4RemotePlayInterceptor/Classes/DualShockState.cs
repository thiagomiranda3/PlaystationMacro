// PS4RemotePlayInterceptor (File: Classes/DualShockState.cs)
//
// Copyright (c) 2018 Komefai
//
// Visit http://komefai.com for more information
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PS4RemotePlayInterceptor
{
    // References
    // https://github.com/Jays2Kings/DS4Windows/blob/jay/DS4Windows/DS4Library/DS4Device.cs
    // https://github.com/Jays2Kings/DS4Windows/blob/jay/DS4Windows/DS4Library/DS4Sixaxis.cs
    // https://github.com/Jays2Kings/DS4Windows/blob/jay/DS4Windows/DS4Library/DS4Touchpad.cs
    // http://www.psdevwiki.com/ps4/DS4-USB

    public class DualShockState
    {
        /// <summary>
        /// Serialize a list of DualShockState to xml file
        /// </summary>
        public static void Serialize(string path, List<byte[]> list)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<byte[]>));
            using (TextWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, list);
            }
        }

        /// <summary>
        /// Deserialize a list of DualShockState from xml file
        /// </summary>
        public static List<byte[]> Deserialize(string path)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<byte[]>));
            using (TextReader reader = new StreamReader(path))
            {
                object obj = deserializer.Deserialize(reader);
                List<byte[]> list = obj as List<byte[]>;
                return list;
            }
        }
    }
}
