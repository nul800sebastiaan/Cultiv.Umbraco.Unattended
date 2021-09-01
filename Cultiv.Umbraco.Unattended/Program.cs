using Microsoft.Web.XmlTransform;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace Cultiv.Umbraco.Unattended
{
    class Program
    {
        static void Main(string[] args)
        {
			// Get current directory to find required config / transform (/database) files
			var appBaseDirectory = System.Reflection.Assembly.GetExecutingAssembly()
				// This path is returned with a leading "file:///" string, so removing that
				.CodeBase.Replace("file:///", "");
			
			appBaseDirectory = Path.GetDirectoryName(appBaseDirectory).EnsureTrailingBackSlash() + @"\dependencies\";
			
			// This is where the root of the Umbraco site is, from the first argument of the command line
			var targetBaseDirectory = args[0].EnsureTrailingBackSlash();
			
			// Config transform file needs to exist in the "dependencies" directory relative to the console's exe
			var transformFile = File.ReadAllText(appBaseDirectory + "Web.config.xdt");

			// Find the Umbraco.Core.dll in the target site and get the Umbraco version that we're going to install
            var umbracoTargetVersion = FileVersionInfo.GetVersionInfo(targetBaseDirectory + @"bin\Umbraco.Core.dll");

			// In the transform file, replace the token with the actual version we're targeting
            transformFile = transformFile.Replace("[[UmbracoVersion]]", 
	            umbracoTargetVersion.ProductVersion);

			// Transform the target Web.config as specified in the Web.config.xdt file that's in the "dependencies"
			// directory relative to the console's exe. Adapted from the well-tested AppHarbor Transform tester
			// https://webconfigtransformationtester.apphb.com/
			// Source code of that site is on GitHub https://github.com/appharbor/appharbor-transformtester/
			try
			{
				using (var document = new XmlTransformableDocument())
				{
					document.PreserveWhitespace = true;
					document.Load(targetBaseDirectory + "Web.config");

					using (var transform = new XmlTransformation(transformFile, false, null))
					{
						if (transform.Apply(document))
						{
							var xmlWriterSettings = new XmlWriterSettings
                            {
                                Indent = true,
                                IndentChars = "    "
                            };

							var stringBuilder = new StringBuilder();
							using (var xmlTextWriter = XmlTextWriter.Create(stringBuilder, xmlWriterSettings))
							{
								document.WriteTo(xmlTextWriter);
							}
							File.WriteAllText(targetBaseDirectory + "Web.config", stringBuilder.ToString());
						}
						else
						{
							Console.WriteLine("Couldn't transform " + targetBaseDirectory + "Web.config"
							                  + " with transform file " + appBaseDirectory
							                  + "Web.config.xdt because of an unknown reason");
						}
					}
				}
			}
			catch (XmlTransformationException xmlTransformationException)
			{
				Console.WriteLine("Couldn't transform " + targetBaseDirectory + "Web.config" 
				                  + " with transform file " + appBaseDirectory + "Web.config.xdt because of: " 
				                  + xmlTransformationException.Message);
			}
			catch (XmlException xmlException)
			{
				Console.WriteLine("Couldn't transform " + targetBaseDirectory + "Web.config" 
				                  + " with transform file " + appBaseDirectory + "Web.config.xdt because of: " 
				                  + xmlException.Message);
			}

			// Copy the pre-configured user config
			File.Copy(appBaseDirectory + "unattended.user.json", 
				targetBaseDirectory + @"App_Data\unattended.user.json", true);

			// For versions before 8.17 we need to copy an empty SQL CE database in App_Data to be able to
			// complete the unattended install
			if (umbracoTargetVersion.ProductMajorPart == 8 && umbracoTargetVersion.ProductMinorPart < 17)
            {
				File.Copy(appBaseDirectory + "Umbraco.sdf", 
					targetBaseDirectory + @"App_Data\Umbraco.sdf", true);
			}

			Console.WriteLine("The Umbraco site in directory " + targetBaseDirectory 
			                                                   + " will now install Umbraco version " 
			                                                   + umbracoTargetVersion.ProductVersion 
			                                                   + " in unattended mode as soon as you start the site.");
        }
    }

	public static class StringExtensions
    {
		public static string EnsureTrailingBackSlash(this string filePath)
		{
			if (filePath.EndsWith(@"\") == false)
			{
				filePath += @"\";
			}

			return filePath;
		}
	}
}
