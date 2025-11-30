using System;
using System.Collections.Generic;
using System.Text;

namespace vNext
{
    public interface IState
    {
        Workflow Workflow { get; set; }
    }
}
