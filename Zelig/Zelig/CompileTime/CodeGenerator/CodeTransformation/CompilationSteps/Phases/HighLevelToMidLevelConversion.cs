//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//


namespace Microsoft.Zelig.CodeGeneration.IR.CompilationSteps.Phases
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Zelig.Runtime.TypeSystem;

    //[PhaseDisabled()]
    [PhaseOrdering( ExecuteAfter = typeof( LayoutTypes ) )]
    public sealed class HighLevelToMidLevelConversion : PhaseDriver
    {
        //
        // Constructor Methods
        //

        public HighLevelToMidLevelConversion( Controller context ) : base ( context )
        {
        }

        //
        // Helper Methods
        //

        public override PhaseDriver Run()
        {
            PerformParallelExecutionOfPhase( true, false );

            return this.NextPhase;
        }
    }
}
