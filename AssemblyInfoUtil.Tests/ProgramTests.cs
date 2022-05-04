using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace AssemblyInfoUtil.Tests
{
    [TestClass]
    public class ProgramTests
    {
        private string ASSEMBLY_INFO = @"AssemblyInfo.cs.test";
        private string ANDROID_MANIFEST = @"AndroidManifest.xml.test";

        private string[] AssemblyInfoLines = new string[]
        {
            "using System.Reflection;",
            "using System.Runtime.CompilerServices;",
            "using System.Runtime.InteropServices;",
            "",
            "[assembly: AssemblyTitle(\"AssemblyInfoUtil\")]",
            "[assembly: AssemblyDescription(\"\")]",
            "[assembly: AssemblyConfiguration(\"\")]",
            "[assembly: AssemblyCompany(\"\")]",
            "[assembly: AssemblyProduct(\"AssemblyInfoUtil\")]",
            "[assembly: AssemblyCopyright(\"Copyright ©  2018\")]",
            "[assembly: AssemblyTrademark(\"\")]",
            "[assembly: AssemblyCulture(\"\")]",
            "",
            "[assembly: AssemblyVersion(\"12.23.34.45\")]",
            "[assembly: AssemblyFileVersion(\"16.17.18.19\")]"
        };

        private string[] AndroidManifestLines = new string[]
        {
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" android:versionCode=\"2\" android:versionName=\"2.0.0.1\" package=\"cz.elgas\" android:installLocation=\"auto\">",
            "<uses-sdk android:minSdkVersion=\"23\" android:targetSdkVersion=\"26\" />",
            "<uses-permission android:name=\"android.permission.ACCESS_NETWORK_STATE\" />",
            "<uses-permission android:name=\"android.permission.BLUETOOTH\" />",
            "<uses-permission android:name=\"android.permission.BLUETOOTH_ADMIN\" />",
            "<uses-permission android:name=\"android.permission.DISABLE_KEYGUARD\" />",
            "<uses-permission android:name=\"android.permission.INTERNET\" />",
            "<uses-permission android:name=\"android.permission.WAKE_LOCK\" />",
            "<uses-permission android:name=\"android.permission.WRITE_EXTERNAL_STORAGE\" />",
            "<uses-permission android:name=\"android.permission.READ_EXTERNAL_STORAGE\" />",
            "<application android:allowBackup=\"true\" android:icon=\"@mipmap/ic_launcher\" android:label=\"@string/app_name\" android:supportsRtl=\"true\" android:theme=\"@style/AppTheme\"></application>",
            "</manifest>"
        };

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void Increment3Test()
        {
            File.WriteAllLines(ASSEMBLY_INFO + "1", AssemblyInfoLines);
            AssemblyInfoUtil.Main(new string[] { @"-inc:3", ASSEMBLY_INFO + "1", @"-save:version1.txt" });

            string[] output = File.ReadAllLines(ASSEMBLY_INFO + "1");

            for (int i = 0; i < 13; i++)
            {
                Assert.AreEqual(output[i], AssemblyInfoLines[i]);
            }
            Assert.AreEqual("[assembly: AssemblyVersion(\"12.23.35.45\")]", output[13]);
            Assert.AreEqual("[assembly: AssemblyFileVersion(\"16.17.19.19\")]", output[14]);
            Assert.AreEqual(15, output.Length);

            output = File.ReadAllLines("version1.txt");
            Assert.AreEqual("Release 16.17.19.19", output[0]);
            Assert.AreEqual(1, output.Length);

            File.Delete(ASSEMBLY_INFO + "1");
            File.Delete("version1.txt");
        }

        [TestMethod]
        public void Increment2Test()
        {
            File.WriteAllLines(ASSEMBLY_INFO + "2", AssemblyInfoLines);
            AssemblyInfoUtil.Main(new string[] { @"-inc:2", ASSEMBLY_INFO + "2", @"-save:version2.txt" });

            string[] output = File.ReadAllLines(ASSEMBLY_INFO + "2");

            for (int i = 0; i < 13; i++)
            {
                Assert.AreEqual(output[i], AssemblyInfoLines[i]);
            }
            Assert.AreEqual("[assembly: AssemblyVersion(\"12.24.34.45\")]", output[13]);
            Assert.AreEqual("[assembly: AssemblyFileVersion(\"16.18.18.19\")]", output[14]);
            Assert.AreEqual(15, output.Length);

            output = File.ReadAllLines("version2.txt");
            Assert.AreEqual("Release 16.18.18.19", output[0]);
            Assert.AreEqual(1, output.Length);

            File.Delete(ASSEMBLY_INFO + "2");
            File.Delete("version2.txt");
        }

        [TestMethod]
        public void IncrementNew1Test()
        {
            File.WriteAllLines(ASSEMBLY_INFO + "3", AssemblyInfoLines);
            AssemblyInfoUtil.Main(new string[] { @"-new", @"-inc:1", ASSEMBLY_INFO + "3" });

            string[] output = File.ReadAllLines(ASSEMBLY_INFO + "3");

            for (int i = 0; i < output.Length; i++)
            {
                Assert.AreEqual(output[i], AssemblyInfoLines[i]);
            }
            Assert.AreEqual(15, output.Length);
            File.Delete(ASSEMBLY_INFO + "3");
        }

        [TestMethod]
        public void VersionTest()
        {
            AssemblyInfoUtil.Main(new string[] { @"-inc:4", @"-save:version3.txt" });
            Assert.AreEqual(false, File.Exists("version3.txt"));
            File.Delete("version3.txt");
        }

        [TestMethod]
        public void AndroidManifestTest()
        {
            File.WriteAllLines(ASSEMBLY_INFO + "4", AssemblyInfoLines);
            File.WriteAllLines(ANDROID_MANIFEST, AndroidManifestLines);

            AssemblyInfoUtil.Main(new string[] { @"-inc:2", ASSEMBLY_INFO + "4", @"-droid:" + ANDROID_MANIFEST, @"-save:version4.txt" });

            string[] output = File.ReadAllLines(ASSEMBLY_INFO + "4");
            Assert.AreEqual("[assembly: AssemblyVersion(\"12.24.34.45\")]", output[13]);
            Assert.AreEqual("[assembly: AssemblyFileVersion(\"16.18.18.19\")]", output[14]);

            output = File.ReadAllLines(ANDROID_MANIFEST);
            Assert.AreEqual("<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" android:versionCode=\"16\" android:versionName=\"16.18.18.19\" package=\"cz.elgas\" android:installLocation=\"auto\">", output[1]);
            for (int i = 2; i < output.Length; i++)
            {
                Assert.AreEqual(output[i], AndroidManifestLines[i]);
            }

            output = File.ReadAllLines("version4.txt");
            Assert.AreEqual("Release 16.18.18.19", output[0]);

            File.Delete(ASSEMBLY_INFO + "4");
            File.Delete(ANDROID_MANIFEST);
            File.Delete("version4.txt");
        }
    }
}
