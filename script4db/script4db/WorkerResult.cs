using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db
{
    enum WorkerResultStatuses
    {
        Error,
        Cancel,
        Success
    }

    class WorkerResult
    {

        private WorkerResultStatuses status;
        private ArrayList logMsgs;

        public WorkerResult(WorkerResultStatuses _status, ArrayList _logMsgs)
        {
            this.status = _status;
            this.logMsgs = _logMsgs;
        }

        public WorkerResultStatuses Status
        {
            get { return status; }
        }

        public ArrayList LogMsgs
        {
            get { return logMsgs; }
        }
    }
}
