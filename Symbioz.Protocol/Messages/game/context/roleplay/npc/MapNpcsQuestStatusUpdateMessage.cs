


















using System;
using System.Collections.Generic;
using System.Linq;
using Symbioz.Protocol.Types;
using SSync.IO;
using SSync.Messages;

namespace Symbioz.Protocol.Messages
{

public class MapNpcsQuestStatusUpdateMessage : Message
{

public const ushort Id = 5642;
public override ushort MessageId
{
    get { return Id; }
}

public int mapId;
        public int[] npcsIdsWithQuest;
        public Types.GameRolePlayNpcQuestFlag[] questFlags;
        public int[] npcsIdsWithoutQuest;
        

public MapNpcsQuestStatusUpdateMessage()
{
}

public MapNpcsQuestStatusUpdateMessage(int mapId, int[] npcsIdsWithQuest, Types.GameRolePlayNpcQuestFlag[] questFlags, int[] npcsIdsWithoutQuest)
        {
            this.mapId = mapId;
            this.npcsIdsWithQuest = npcsIdsWithQuest;
            this.questFlags = questFlags;
            this.npcsIdsWithoutQuest = npcsIdsWithoutQuest;
        }
        

public override void Serialize(ICustomDataOutput writer)
{

writer.WriteInt(mapId);
            writer.WriteUShort((ushort)npcsIdsWithQuest.Length);
            foreach (var entry in npcsIdsWithQuest)
            {
                 writer.WriteInt(entry);
            }
            writer.WriteUShort((ushort)questFlags.Length);
            foreach (var entry in questFlags)
            {
                 entry.Serialize(writer);
            }
            writer.WriteUShort((ushort)npcsIdsWithoutQuest.Length);
            foreach (var entry in npcsIdsWithoutQuest)
            {
                 writer.WriteInt(entry);
            }
            

}

public override void Deserialize(ICustomDataInput reader)
{

mapId = reader.ReadInt();
            var limit = reader.ReadUShort();
            npcsIdsWithQuest = new int[limit];
            for (int i = 0; i < limit; i++)
            {
                 npcsIdsWithQuest[i] = reader.ReadInt();
            }
            limit = reader.ReadUShort();
            questFlags = new Types.GameRolePlayNpcQuestFlag[limit];
            for (int i = 0; i < limit; i++)
            {
                 questFlags[i] = new Types.GameRolePlayNpcQuestFlag();
                 questFlags[i].Deserialize(reader);
            }
            limit = reader.ReadUShort();
            npcsIdsWithoutQuest = new int[limit];
            for (int i = 0; i < limit; i++)
            {
                 npcsIdsWithoutQuest[i] = reader.ReadInt();
            }
            

}


}


}