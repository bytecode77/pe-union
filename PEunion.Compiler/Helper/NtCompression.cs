using BytecodeApi.Extensions;
using System;
using System.Runtime.InteropServices;

namespace PEunion.Compiler.Helper
{
	/// <summary>
	/// Helper class for data compression using the NT api.
	/// </summary>
	public static class NtCompression
	{
		/// <summary>
		/// Compresses <paramref name="data" /> using RtlCompressBuffer.
		/// Because the stub is written in FASM, a simple, small and accessible algorithm should be used.
		/// </summary>
		/// <param name="data">The <see cref="byte" />[] to compress.</param>
		/// <returns>
		/// A new <see cref="byte" />[] with the compressed data.
		/// </returns>
		public static byte[] Compress(byte[] data)
		{
			// Fortunately, WinAPI comes with handy functions for data compression.
			// In the assembly stub, RtlDecompressBuffer is called to decompress the data.

			if (RtlGetCompressionWorkSpaceSize(2, out uint workSpaceSize, out _) == 0)
			{
				IntPtr workSpace = LocalAlloc(0, new IntPtr(workSpaceSize));
				byte[] compressed = new byte[data.Length + 1024 * 16];

				uint result = RtlCompressBuffer(0x102, data, data.Length, compressed, compressed.Length, 0, out int compressedSize, workSpace);
				LocalFree(workSpace);
				if (result == 0) return compressed.GetBytes(0, compressedSize);
			}

			return null;
		}

		[DllImport("ntdll.dll")]
		private static extern uint RtlGetCompressionWorkSpaceSize(ushort compressionFormat, out uint workSpaceSize, out uint fragmentWorkSpaceSize);
		[DllImport("ntdll.dll")]
		private static extern uint RtlCompressBuffer(ushort compressionFormat, byte[] buffer, int bufferSize, byte[] compressedBuffer, int compressedBufferSize, uint chunkSize, out int finalCompressedSize, IntPtr workSpace);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LocalAlloc(int flags, IntPtr size);
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LocalFree(IntPtr buffer);
	}
}