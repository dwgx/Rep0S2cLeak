using System;
using System.Runtime.InteropServices;

namespace LeastSquares.Overtone;

public class FixedPointerToHeapAllocatedMem
{
	private GCHandle _handle;

	public IntPtr Address { get; private set; }

	public uint SizeInBytes { get; private set; }

	public void Free()
	{
		_handle.Free();
		Address = IntPtr.Zero;
	}

	public static FixedPointerToHeapAllocatedMem Create<T>(T Object, uint SizeInBytes)
	{
		FixedPointerToHeapAllocatedMem obj = new FixedPointerToHeapAllocatedMem
		{
			_handle = GCHandle.Alloc(Object, GCHandleType.Pinned),
			SizeInBytes = SizeInBytes
		};
		obj.Address = obj._handle.AddrOfPinnedObject();
		return obj;
	}
}
