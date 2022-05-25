using CX.TaskManagerLib;
using Opus.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Services.Implementation.Configuration
{
    public class TaskConnection : ITaskConnection
    {
        public string TaskDataStorageLocation => FilePaths.TASKMANAGEMENTDATABASEPATH;
    }
}
