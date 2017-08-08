using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Aims.IISAgent.Module.Pipes
{
	internal sealed class NamedPipeClient : IDisposable
	{
		#region PInvoke

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
			[MarshalAs(UnmanagedType.U4)] FileShare dwShareMode, IntPtr attr,
			[MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FlushFileBuffers(IntPtr hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetLastError();

		[DllImport("kernel32.dll")]
		private static extern bool GetNamedPipeHandleState(IntPtr hHandle, uint lpState, ref uint lpCurInstances,
			uint lpMaxCollectionCount, uint lpCollectDataTimeout, uint lpUserName, uint nMaxUserNameSize);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern unsafe bool PeekNamedPipe(IntPtr hHandle, byte* lpBuffer, uint nBufferSize, ref uint lpBytesRead,
			ref uint lpTotalBytesAvail, ref uint lpBytesLeftThisMessage);

		[DllImport(@"kernel32.dll", SetLastError = true)]
		private static extern unsafe bool ReadFile(IntPtr hHandle, byte* lpBuffer, int nNumberOfBytesToRead,
			ref uint pNumberOfBytesRead, uint lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool SetNamedPipeHandleState(IntPtr hHandle, ref uint mode, IntPtr cc, IntPtr cd);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern unsafe bool TransactNamedPipe(IntPtr hNamedPipe, byte* lpInBuffer, uint nInBufferSize,
			[Out] byte* lpOutBuffer, uint nOutBufferSize, ref int lpBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WaitNamedPipe(string name, int timeout);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern unsafe bool WriteFile(IntPtr hHandle, byte* lpBuffer, uint nNumberOfBytesToWrite,
			ref uint lpNumberOfBytesWritten, uint lpOverlapped);

		#endregion PInvoke

		private readonly string _name;
		private bool _disposed;
		private IntPtr _handle;

		public NamedPipeClient(string name)
		{
			_name = name;
		}

		~NamedPipeClient()
		{
			Dispose(false);
		}

		public bool IsConnected
		{
			get
			{
				uint count = 0;
				GetNamedPipeHandleState(_handle, 0, ref count, 0, 0, 0, 0);
				return count != 0;
			}
		}

		public unsafe bool IsEmpty
		{
			get
			{
				uint avail = 0, readed = 0, left = 0;
				byte[] buffer = new byte[1];
				fixed (byte* ptr = buffer)
				{
					PeekNamedPipe(_handle, ptr, 1, ref readed, ref avail, ref left);
				}
				return avail == 0 || readed == 0;
			}
		}

		public void Close()
		{
			CheckIfDisposed();
			CloseHandle(_handle);
		}

		public void Connect(int timeout = -1)
		{
			CheckIfDisposed();

			string name = @"\\.\pipe\" + _name;
			var startTime = DateTime.UtcNow;

			int fHigh = 1;
			int fLow = 1;

			while (true)
			{
				uint lastError;

				if (!WaitNamedPipe(name, timeout))
				{
					lastError = GetLastError();
					switch (lastError)
					{
						case 0x02:
							Thread.Sleep(fHigh - 1);
							if (fHigh < 50)
							{
								fHigh += fLow;
								fLow = fHigh - fLow;
							}

							if (timeout != -1 && (DateTime.UtcNow - startTime).TotalMilliseconds >= timeout)
								throw new TimeoutException();

							continue;
						case 0x79:
							throw new TimeoutException();
						default:
							throw new Win32Exception((int)lastError, "Failed connect to pipe.");
					}
				}

				_handle = CreateFile(name, 0xC0000000, FileShare.None, IntPtr.Zero, FileMode.Open, 0, 0);

				if (_handle.ToInt32() != -1)
				{
					// Set the read mode of the pipe channel
					uint mode = 0x02; // PIPE_READMODE_MESSAGE;
					if (SetNamedPipeHandleState(_handle, ref mode, IntPtr.Zero, IntPtr.Zero))
						break;

					lastError = GetLastError();
					CloseHandle(_handle);

					throw new Win32Exception((int)lastError, "Failed set pipe mode.");
				}

				throw new Win32Exception((int)GetLastError(), "Failed establish connection.");
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Flush()
		{
			FlushFileBuffers(_handle);
		}

		public unsafe bool Transact(byte[] bytes)
		{
			CheckIfDisposed();

			if (bytes == null || bytes.Length == 0)
				return false;

			var len = (uint)bytes.Length;
			var response = new byte[1];
			bool result;
			int readed = 0;

			fixed (byte* inPtr = bytes)
			fixed (byte* outPtr = response)
			{
				result = TransactNamedPipe(_handle, inPtr, len, outPtr, 1, ref readed, IntPtr.Zero);
				Flush();
			}

			return result && readed == 1 && response[0] != 0;
		}

		public unsafe int Read(byte[] buffer, int offset, int length)
		{
			fixed (byte* ptr = buffer)
			{
				uint numBytesToRead = 0;
				var result = ReadFile(_handle, ptr, length, ref numBytesToRead, 0);

				var lastError = (int)GetLastError();
				if (!result && lastError != 0x000000EA) // ERROR_MORE_DATA
					throw new Exception();

				return (int)numBytesToRead;
			}
		}

		public unsafe void Write(byte[] bytes)
		{
			CheckIfDisposed();

			if (bytes == null || bytes.Length == 0)
				return;

			// Get the message length
			var len = (uint)bytes.Length;

			fixed (byte* ptr = bytes)
			{
				uint offset = 0;
				do
				{
					uint numReadWritten = 0;
					if (!WriteFile(_handle, ptr + offset, len, ref numReadWritten, 0))
						throw new Win32Exception((int)GetLastError(), "Error writing to pipe.");
					offset += numReadWritten;
					len -= numReadWritten;
				}
				while (len > 0);
			}
			Flush();
		}

		private void CheckIfDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("The pipe connection is disposed.");
			}
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				Close();
			}
			_disposed = true;
		}
	}
}