


















using System;
using System.Collections.Generic;
using System.Linq;
using Symbioz.Protocol.Types;
using SSync.IO;
using SSync.Messages;

namespace Symbioz.Protocol.Messages
{

public class QuestValidatedMessage : Message
{

public const ushort Id = 6097;
public override ushort MessageId
{
    get { return Id; }
}

public ushort questId;
        

public QuestValidatedMessage()
{
}

public QuestValidatedMessage(ushort questId)
        {
            this.questId = questId;
        }
        

public override void Serialize(ICustomDataOutput writer)
{

writer.WriteVarUhShort(questId);
            

}

public override void Deserialize(ICustomDataInput reader)
{

questId = reader.ReadVarUhShort();
            if (questId < 0)
                throw new Exception("Forbidden value on questId = " + questId + ", it doesn't respect the following condition : questId < 0");
            

}


}


}