//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    public abstract class LinearMemoryManager : MemoryManager
    {
        //
        // Helper Methods
        //

        public override void InitializeMemoryManager()
        {
            base.InitializeMemoryManager();

            Memory.Range[] ranges = Memory.Instance.AvailableMemory;

            foreach(Memory.Range rng in ranges)
            {
                if((rng.Usage      & MemoryUsage     .Heap     ) != 0 &&
                   (rng.Attributes & MemoryAttributes.Allocated) == 0  )
                {
                    AddLinearSection( rng.Start, rng.End, rng.Attributes );
                }
            }
        }

        public override void InitializationComplete()
        {
            if(MemoryManager.Configuration.TrashFreeMemory)
            {
                DirtyFreeMemory();
            }
            else
            {
                ZeroFreeMemory();
            }
        }

        //--//

        public override unsafe UIntPtr Allocate( uint size )
        {
            BugCheck.Assert( MemoryManager.Lock.IsHeldByCurrentThread( ), BugCheck.StopCode.HeapCorruptionDetected );

            MemorySegment* ptr = m_active;

            if(ptr != null)
            {
                UIntPtr res = ptr->Allocate( size );

                if(res != UIntPtr.Zero)
                {
                    GarbageCollectionManager.Instance.NotifyNewObject( res, size );
                    return res;
                }
            }

            ptr = m_heapHead;
            while(ptr != null)
            {
                UIntPtr res = ptr->Allocate( size );

                if(res != UIntPtr.Zero)
                {
                    m_active = ptr;

                    GarbageCollectionManager.Instance.NotifyNewObject( res, size );
                    return res;
                }

                ptr = ptr->Next;
            }

            return UIntPtr.Zero;
        }

        public override unsafe void Release(UIntPtr address)
        {
            if (address != UIntPtr.Zero)
            {
                using (SmartHandles.YieldLockHolder hnd = new SmartHandles.YieldLockHolder(MemoryManager.Lock))
                {
                    for (MemorySegment* heap = m_heapHead; heap != null; heap = heap->Next)
                    {
                        if (AddressMath.IsInRange(address, heap->Beginning, heap->End))
                        {
                            heap->Release(address);
                        }
                    }
                }
            }
        }

        public override bool RefersToMemory( UIntPtr address )
        {
            foreach(Memory.Range rng in Memory.Instance.AvailableMemory)
            {
                if(AddressMath.IsInRange( address, rng.Start, rng.End ))
                {
                    return true;
                }
            }

            return false;
        }

        internal override void ConsistencyCheck()
        {
            ConsistencyCheck(dumpMemory:false);
        }

        internal override void DumpMemory()
        {
            ConsistencyCheck(dumpMemory:true);
        }

        private unsafe void ConsistencyCheck(bool dumpMemory)
        {
            for (MemorySegment* heap = m_heapHead; heap != null; heap = heap->Next)
            {
                heap->ConsistencyCheck(dumpMemory);
            }
        }

        internal override unsafe bool IsObjectAlive( UIntPtr ptr )
        {
            for(MemorySegment* heap = m_heapHead; heap != null; heap = heap->Next)
            {
                if(AddressMath.IsInRange( ptr, heap->Beginning, heap->End ))
                {
                    return heap->IsObjectAlive( ptr );
                }
            }

            return false;
        }
    }
}
