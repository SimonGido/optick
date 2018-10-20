﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profiler.Data
{
	public class SysCallEntry : Entry
	{
		public UInt64 ThreadID { get; set; }
		public UInt64 SysCallID { get; set; }

		public SysCallEntry(BinaryReader reader, EventDescriptionBoard board)
		{
			ReadEntry(reader, board);
			ThreadID = reader.ReadUInt64();
			SysCallID = reader.ReadUInt64();
		}
	}

	public class SysCallBoard : IResponseHolder
	{
		public override DataResponse Response { get; set; }

		Dictionary<UInt64, List<SysCallEntry>> sysCallMap;

		public bool HasSysCall(UInt64 threadID, long timeStamp)
		{
			List<SysCallEntry> entries = null;
			if (sysCallMap.TryGetValue(threadID, out entries))
			{
				int index = Utils.BinarySearchClosestIndex(entries, timeStamp);
				if (index >= 0)
					return entries[index].Start == timeStamp;
			}

			return false;
		}

		public static SysCallBoard Create(DataResponse response, FrameGroup group)
		{
			SysCallBoard result = new SysCallBoard() { Response = response, sysCallMap = new Dictionary<UInt64, List<SysCallEntry>>() };

			ulong totalCount = response.Reader.ReadUInt32();
			for (ulong i = 0; i < totalCount; ++i)
			{
				SysCallEntry entry = new SysCallEntry(response.Reader, group.Board);

				List<SysCallEntry> entries = null;
				if (!result.sysCallMap.TryGetValue(entry.ThreadID, out entries))
				{
					entries = new List<SysCallEntry>();
					result.sysCallMap.Add(entry.ThreadID, entries);
				}
				entries.Add(entry);
			}

			return result;
		}
	}
}
