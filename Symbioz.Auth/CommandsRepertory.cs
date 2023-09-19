using Symbioz.Auth.Transition;
using Symbioz.Core;
using Symbioz.Core.Commands;
using Symbioz.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Symbioz.Protocol.Selfmade.Messages;
using System.Threading.Tasks;
using System.Threading;
using Symbioz.Auth.Records;
using Symbioz.Network.Servers;

namespace Symbioz.Auth
{
    class CommandsRepertory
    {
        static Logger logger = new Logger();

        [ConsoleCommand("infos")]
        public static void Infos(string input)
        {
            logger.White("Clients Connecteds: " + AuthServer.Instance.ClientsCount);
        }

        [ConsoleCommand("banip")]
        public static void BanIp(string input)
        {
            BanIpRecord record = new BanIpRecord(input);
            record.AddInstantElement();
        }

        [ConsoleCommand("addaccount")]
        public static void Account(string input)
        {
            AccountRecord accountRecord = AccountRecord.GetAccountRecord((int)DatabaseReader<AccountRecord>.Count("Id"));
            int id = accountRecord.Id;
            id++;

            Console.Write("username : ");
            var username = Console.ReadLine();

            Console.Write("password : ");
            var password = Console.ReadLine();

            Console.WriteLine("nickname : ");
            var nickname = Console.ReadLine();

            Console.WriteLine("role : ");
            var roleString = Console.ReadLine();
            sbyte role = Convert.ToSByte(roleString);

            var newAccount = new AccountRecord(id, username, password, nickname, role, false, 5, 30);
            DatabaseWriter<AccountRecord>.InstantInsert(newAccount);

            Console.WriteLine("Compte créer avec succès ! ");
        }
    }
}
