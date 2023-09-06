﻿using SSync.IO;
using SSync.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbioz.Protocol.Selfmade.Messages
{
    public class ResetDatabaseMessage : TransitionMessage
    {
        public const ushort Id = 6707;

        public override ushort MessageId
        {
            get { return Id; }
        }

        public override void Serialize(ICustomDataOutput writer)
        {
          
        }

        public override void Deserialize(ICustomDataInput reader)
        {
            
        }
    }
}
