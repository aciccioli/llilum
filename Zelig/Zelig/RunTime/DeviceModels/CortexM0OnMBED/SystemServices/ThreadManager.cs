//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.CortexM0OnMBED
{
    using RT            = Microsoft.Zelig.Runtime;
    using ChipsetModel  = Microsoft.CortexM0OnCMSISCore;


    public abstract class ThreadManager : ChipsetModel.ThreadManager
    {
        //
        // State
        //

        //
        // BUGBUG: we need to Dispose this object on shutdown !!!
        //
        Drivers.SystemTimer.Timer m_timerForWaits;

        //--//
       
        //
        // Helper Methods
        //

        public override void Activate()
        {
            base.Activate( ); 

            m_timerForWaits = Drivers.SystemTimer.Instance.CreateTimer( WaitExpired );

            DeviceModels.Chipset.CortexM0.Drivers.InterruptController.Instance.Activate();
        }

        public override void SetNextWaitTimer( RT.SchedulerTime nextTimeout )
        {
            if(nextTimeout != RT.SchedulerTime.MaxValue)
            {
                m_timerForWaits.Timeout = nextTimeout.Units;
            }
            else
            {
                m_timerForWaits.Cancel( );
            }
        }

        //--//

        private void WaitExpired( Drivers.SystemTimer.Timer sysTickTimer, ulong currentTime )
        {
            WaitExpired( RT.SchedulerTime.FromUnits( currentTime ) );
        }
    }
}
