﻿using Symbioz.World.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Symbioz.World.Providers.Criterias.Repertory
{
    [Criteria("PP")]
    public class GradeCriteria : AbstractCriteria
    {
        public override bool Eval(WorldClient client)
        {
           return BasicEval(CriteriaValue, ComparaisonSymbol, client.Character.Record.Alignment.Grade);
        }
    }
}
