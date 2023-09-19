﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Symbioz.ProtocolBuilder.Parsing;
using Symbioz.ProtocolBuilder.Parsing.Elements;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Symbioz.ProtocolBuilder.XmlPatterns
{
    public class XmlTypesBuilder : XmlPatternBuilder
    {
        public XmlTypesBuilder(Parser parser)
            : base(parser)
        {
        }

        public override void WriteToXml(XmlWriter writer)
        {
            var xmlType = new XmlType
            {
                Name = Parser.Class.Name,
                Id = Parser.Fields.Find(entry => entry.Name == "protocolId").Value,
                Heritage = Parser.Class.Heritage,
            };

            if (Parser.Class.Name.Contains("CommonIn"))
            {

            }
            var xmlFields = new List<XmlField>();

            MethodInfo deserializeAsMethod = Parser.Methods.Find(entry => entry.Name.Contains("deserializeAs"));
            string type = null;
            int limit = 0;

            for (int i = 0; i < deserializeAsMethod.Statements.Count; i++)
            {
                if (deserializeAsMethod.Statements[i] is AssignationStatement &&
                    ((AssignationStatement)deserializeAsMethod.Statements[i]).Value.Contains("Read"))
                {
                    var statement = ((AssignationStatement)deserializeAsMethod.Statements[i]);
                    type = Regex.Match(statement.Value, @"Read([\w\d_]+)\(").Groups[1].Value.ToLower();
                    var name = statement.Name;

                    if (type.Contains("var"))
                    {
                        char[] letters = type.ToCharArray();

                        letters[3] = char.ToUpper(letters[3]);
                        if (char.ToLower(letters[3]) == 'u')
                            letters[5] = char.ToUpper(letters[5]);

                        type = new string(letters);
                    }
                    if (type == "bytes")
                        type = "byte[]";

                    Match match = Regex.Match(name, @"^([\w\d]+)\[.+\]$");
                    if (match.Success)
                    {
                        IEnumerable<string> limitLinq = from entry in Parser.Constructors[0].Statements
                                                        where
                                                            entry is AssignationStatement &&
                                                            ((AssignationStatement)entry).Name == match.Groups[1].Value
                                                        let entryMatch =
                                                            Regex.Match(((AssignationStatement)entry).Value,
                                                                        @"new List<[\d\w\._]+>\(([\d]+)\)")
                                                        where entryMatch.Success
                                                        select entryMatch.Groups[1].Value;

                        if (limitLinq.Count() == 1)
                            limit = int.Parse(limitLinq.Single());

                        type += "[]";
                        name = name.Split('[')[0];

                    }

                    FieldInfo field = Parser.Fields.Find(entry => entry.Name == name);

                    if (field != null)
                    {
                        string condition = null;

                        if (i + 1 < deserializeAsMethod.Statements.Count &&
                            deserializeAsMethod.Statements[i + 1] is ControlStatement &&
                            ((ControlStatement)deserializeAsMethod.Statements[i + 1]).ControlType == ControlType.If)
                            condition = ((ControlStatement)deserializeAsMethod.Statements[i + 1]).Condition;

                        xmlFields.Add(new XmlField
                        {
                            Name = field.Name,
                            Type = type,
                            Limit = limit > 0 ? limit.ToString() : null,
                            Condition = condition,
                        });

                        limit = 0;
                        type = null;
                    }
                }

                if (deserializeAsMethod.Statements[i] is InvokeExpression &&
                    ((InvokeExpression)deserializeAsMethod.Statements[i]).Name == "deserialize")
                {
                    var statement = ((InvokeExpression)deserializeAsMethod.Statements[i]);
                    FieldInfo field = Parser.Fields.Find(entry => entry.Name == statement.Target);

                    if (field != null && xmlFields.Count(entry => entry.Name == field.Name) <= 0)
                    {
                        type = "Types." + field.Type;

                        string condition = null;

                        if (i + 1 < deserializeAsMethod.Statements.Count &&
                            deserializeAsMethod.Statements[i + 1] is ControlStatement &&
                            ((ControlStatement)deserializeAsMethod.Statements[i + 1]).ControlType == ControlType.If)
                            condition = ((ControlStatement)deserializeAsMethod.Statements[i + 1]).Condition;

                        xmlFields.Add(new XmlField
                        {
                            Name = field.Name,
                            Type = type,
                            Limit = limit > 0 ? limit.ToString() : null,
                            Condition = condition,
                        });

                        limit = 0;
                        type = null;
                    }
                    else if (i > 0 &&
                             deserializeAsMethod.Statements[i - 1] is AssignationStatement)
                    {
                        var substatement = ((AssignationStatement)deserializeAsMethod.Statements[i - 1]);
                        var name = substatement.Name;
                        Match match = Regex.Match(substatement.Value, @"new ([\d\w]+)");
                        if (match.Success)
                        {
                            type = "Types." + match.Groups[1].Value;

                            Match arrayMatch = Regex.Match(name, @"^([\w\d]+)\[.+\]$");
                            if (arrayMatch.Success)
                            {
                                IEnumerable<string> limitLinq = from entry in Parser.Constructors[0].Statements
                                                                where
                                                                    entry is AssignationStatement &&
                                                                    ((AssignationStatement)entry).Name == arrayMatch.Groups[1].Value
                                                                let entryMatch =
                                                                    Regex.Match(((AssignationStatement)entry).Value,
                                                                                @"new List<[\d\w\._]+>\(([\d]+)\)")
                                                                where entryMatch.Success
                                                                select entryMatch.Groups[1].Value;

                                if (limitLinq.Count() == 1)
                                    limit = int.Parse(limitLinq.Single());

                                type += "[]";
                                name = name.Split('[')[0];

                            }
                        }

                        field = Parser.Fields.Find(entry => entry.Name == name);

                        if (field != null && xmlFields.Count(entry => entry.Name == field.Name) <= 0)
                        {
                            string condition = null;

                            if (i + 1 < deserializeAsMethod.Statements.Count &&
                                deserializeAsMethod.Statements[i + 1] is ControlStatement &&
                                ((ControlStatement)deserializeAsMethod.Statements[i + 1]).ControlType == ControlType.If)
                                condition = ((ControlStatement)deserializeAsMethod.Statements[i + 1]).Condition;

                            xmlFields.Add(new XmlField
                            {
                                Name = field.Name,
                                Type = type,
                                Limit = limit > 0 ? limit.ToString() : null,
                                Condition = condition,
                            });

                            limit = 0;
                            type = null;
                        }
                    }
                }

                if (deserializeAsMethod.Statements[i] is AssignationStatement &&
                    ((AssignationStatement)deserializeAsMethod.Statements[i]).Value.Contains("getFlag"))
                {
                    var statement = ((AssignationStatement)deserializeAsMethod.Statements[i]);
                    FieldInfo field = Parser.Fields.Find(entry => entry.Name == statement.Name);

                    var match = Regex.Match(statement.Value, @"getFlag\([\w\d]+, (\d+)\)");

                    if (match.Success)
                    {
                        type = "flag(" + match.Groups[1].Value + ")";

                        if (field != null)
                        {
                            xmlFields.Add(new XmlField
                            {
                                Name = field.Name,
                                Type = type,
                            });

                            type = null;
                        }
                    }
                }

                if (deserializeAsMethod.Statements[i] is AssignationStatement &&
                    ((AssignationStatement)deserializeAsMethod.Statements[i]).Value.Contains("getInstance"))
                {
                    var statement = ((AssignationStatement)deserializeAsMethod.Statements[i]);
                    FieldInfo field = Parser.Fields.Find(entry => entry.Name == statement.Name);

                    type = "instance of Types." + Regex.Match(statement.Value, @"getInstance\(([\w\d_\.]+),").Groups[1].Value;

                    if (field != null)
                    {
                        xmlFields.Add(new XmlField
                        {
                            Name = field.Name,
                            Type = type,
                        });

                        type = null;
                    }
                }

                if (deserializeAsMethod.Statements[i] is InvokeExpression &&
                    ((InvokeExpression)deserializeAsMethod.Statements[i]).Name == "Add" &&
                    type != null)
                {
                    var statement = ((InvokeExpression)deserializeAsMethod.Statements[i]);

                    FieldInfo field = Parser.Fields.Find(entry => entry.Name == statement.Target);

                    string condition = null;

                    if (i + 1 < deserializeAsMethod.Statements.Count &&
                        deserializeAsMethod.Statements[i + 1] is ControlStatement &&
                        ((ControlStatement)deserializeAsMethod.Statements[i + 1]).ControlType == ControlType.If)
                        condition = ((ControlStatement)deserializeAsMethod.Statements[i + 1]).Condition;

                    xmlFields.Add(new XmlField
                    {
                        Name = field.Name,
                        Type = type + "[]",
                        Limit = limit > 0 ? limit.ToString() : null,
                        Condition = condition,
                    });

                    limit = 0;
                    type = null;
                }
            }

            xmlType.Fields = xmlFields.ToArray();

            var serializer = new XmlSerializer(typeof(XmlType));
            serializer.Serialize(writer, xmlType);
        }
    }
}
