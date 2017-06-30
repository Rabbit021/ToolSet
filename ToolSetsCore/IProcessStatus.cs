using System;

namespace ToolSetsCore
{
    public interface IProcessStatus
    {
        int State { get; set; }
        string Message { get; set; }

        void SetState(int state, Exception exp);
        void SetState(int state, string msg);
        void SetState(int state);
    }
}