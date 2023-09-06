


















using System;
using System.Collections.Generic;
using System.Linq;
using Symbioz.Protocol.Types;
using SSync.IO;
using SSync.Messages;

namespace Symbioz.Protocol.Messages
{

public class ExchangeIsReadyMessage : Message
{

public const ushort Id = 5509;
public override ushort MessageId
{
    get { return Id; }
}

public double id;
        public bool ready;
        

public ExchangeIsReadyMessage()
{
}

public ExchangeIsReadyMessage(double id, bool ready)
        {
            this.id = id;
            this.ready = ready;
        }
        

public override void Serialize(ICustomDataOutput writer)
{

writer.WriteDouble(id);
            writer.WriteBoolean(ready);
            

}

public override void Deserialize(ICustomDataInput reader)
{

id = reader.ReadDouble();
            if (id < -9007199254740990 || id > 9007199254740990)
                throw new Exception("Forbidden value on id = " + id + ", it doesn't respect the following condition : id < -9007199254740990 || id > 9007199254740990");
            ready = reader.ReadBoolean();
            

}


}


}