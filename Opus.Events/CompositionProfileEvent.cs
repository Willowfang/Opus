using Opus.Services.Data.Composition;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Opus.Events
{
    public class CompositionProfileEvent : PubSubEvent<ICompositionProfile> { }
}
