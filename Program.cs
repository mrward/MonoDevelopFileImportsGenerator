//
// MyClass.cs
//
// Author:
//       Matt Ward <ward.matt@gmail.com>
//
// Copyright (c) 2016 Matthew Ward
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
//

using System;
using System.IO;
using System.Xml;

namespace MonoDevelopFileImportsGenerator
{
	public class Program
	{
		string rootDirectory;

		static void Main (string[] args)
		{
			try {
				var program = new Program ();
				program.Run (args);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		void Run (string[] args)
		{
			if (!ParseArguments (args)) {
				ShowUsage ();
				return;
			}

			if (!Directory.Exists (rootDirectory)) {
				Console.WriteLine ("AddIn directory does not exist. '{0}'", rootDirectory);
				return;
			}

			GenerateFileImports ();
		}

		bool ParseArguments(string[] args)
		{
			if (args.Length != 1) {
				return false;
			}

			rootDirectory = Path.GetFullPath (args[0]);

			return true;
		}

		void ShowUsage ()
		{
			Console.WriteLine ("Usage: FileImportsGenerator AddInDirectory");
		}

		void GenerateFileImports ()
		{
			var settings = new XmlWriterSettings {
				IndentChars = "\t",
				Indent = true,
				OmitXmlDeclaration = true
			};

			using (var writer = XmlWriter.Create ("generated.addin.xml", settings)) {

				writer.WriteStartElement ("ExtensionModel");
				writer.WriteStartElement ("Runtime");

				foreach (string file in Directory.EnumerateFiles (rootDirectory, "*.*", SearchOption.AllDirectories)) {

					writer.WriteStartElement ("Import");

					string relativeFilePath = GetRelativeFilePath (file);
					WriteImportAttribute (writer, relativeFilePath);

					writer.WriteEndElement ();
				}

				writer.WriteEndElement ();
				writer.WriteEndElement ();
			}
		}

		string GetRelativeFilePath (string file)
		{
			string relativeFilePath = file.Substring (rootDirectory.Length);
			if (relativeFilePath[0] == Path.DirectorySeparatorChar) {
				return relativeFilePath.Substring (1);
			}
			return relativeFilePath;
		}

		static bool IsAssembly (string extension)
		{
			return ".dll".Equals (extension, StringComparison.OrdinalIgnoreCase);
		}

		static void WriteImportAttribute (XmlWriter writer, string file)
		{
			string attributeName = GetAttributeName (file);

			file = file.Replace ('\\', '/');
			writer.WriteAttributeString (attributeName, file);
		}

		static string GetAttributeName (string file)
		{
			string extension = Path.GetExtension (file);
			if (IsAssembly (extension)) {
				return "assembly";
			}

			return "file";
		}
	}
}

