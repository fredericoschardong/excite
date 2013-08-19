using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TimelineSample
{
    public class Parser
    {
        public String rule;
        public String originalRule;
        public static String[] boolEvents = new String[] { "colliding", "pointing_at", "pointing_towards", "touching", "from", "towards", "parallel", "purpendicular"};
        public static String[] valEvents = new String[] { "distance", "velocity_difference" };
        public static String[] valOperators = new String[] { "<=", "<", ">=", ">", "==", "!=" };
        public static String[] valProperties = new String[] { "x", "y", "z" };
        public static String[] boolProperties = new String[] { "moving" };
        public static String[] functions = new String[] { "strict_order", "flexible_order" };

        List<String> entities = new List<String>();

        public Parser(List<String> entities)
        {
            this.entities = entities;
        }

        public void setRule(String rule_temp)
        {
            this.originalRule = rule_temp;
            this.rule = rule_temp.Replace(" ", String.Empty);

            this.rule = this.rule.Replace("\n", String.Empty);
            this.rule = this.rule.Replace("\r", String.Empty);
            this.rule = this.rule.Replace("\t", String.Empty);
        }

        private List<Relations> splitOrder()
        {
            //for special functions
            foreach (String action in functions)
            {
                if (this.rule.IndexOf(action) == 0)
                {
                    if (!(this.rule.Split('(').Length == this.rule.Split(')').Length))
                    {
                        MessageBoxResult result = MessageBox.Show("The number of opening and closing parentheses don't match", "Invalid rule");
                        return null;
                    }

                    if (this.rule.Split(',').Count() < 4)
                    {
                        MessageBoxResult result = MessageBox.Show("There is an error with your rule: not enough parameters", "Error");
                        return null;
                    }

                    String[] conds = this.rule.Substring(this.rule.IndexOf('(') + 1, this.rule.LastIndexOf(')') - this.rule.IndexOf('(') - 1).Split(',');

                    int time = 0;

                    try
                    {
                        time = Convert.ToInt32(conds[0]);
                    }
                    catch (FormatException)
                    {
                        MessageBoxResult result = MessageBox.Show("There is an error with your rule: could not convert first parameter to integer", "Error");
                        return null;
                    }

                    Relations.operations op;

                    if (conds[1] == "and")
                    {
                        op = Relations.operations.AND;
                    }
                    else
                    {
                        if (conds[1] == "or")
                        {
                            op = Relations.operations.OR;
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("There is an error with your rule: could not convert second parameter to boolean (use \"and\" or \"or\")", "Error");
                            return null;
                        }
                    }

                    List<Relations> relations = new List<Relations>();

                    for (int x = 2; x < conds.Count(); x++)
                    {
                        //Console.WriteLine("rule " + conds[x]);

                        conds[x] = conds[x].Trim();

                        if (conds[x] == "")
                        {
                            MessageBoxResult result = MessageBox.Show("There is an error with your rule: " + conds[x], "Error");
                            return null;
                        }

                        Relations temp = new Relations();
                        temp.rule = conds[x];
                        temp.operation = Relations.operations.NULL;

                        relations.Add(temp);

                        relations.Last().condition = this.validate(relations.Last().rule);

                        if (relations.Last().condition == null)
                        {
                            MessageBoxResult result = MessageBox.Show("There is an error with your rule: " + relations.Last().displayRule, "Error");
                            return null;
                        }

                        relations.Last().orderTime = time;
                        relations.Last().order = action == "strict_order" ? Relations.orderType.STRICT : Relations.orderType.FLEXIBLE;

                        if (x > 2)
                        {
                            relations.Add(new Relations());
                            relations.Last().operation = op;
                            relations.Last().relation1 = relations.Count() - 3;
                            relations.Last().relation2 = relations.Count() - 2;

                            relations.Last().orderTime = time;
                            relations.Last().order = action == "strict_order" ? Relations.orderType.STRICT : Relations.orderType.FLEXIBLE;
                        }
                    }

                    if (relations.Count > 0)
                    {
                        relations.First().displayRule = this.originalRule;
                    }

                    return relations;
                }
            }

            return null;
        }

        public List<Relations> splitConditions()
        {
            if (this.rule.IndexOf("_order") != -1)
            {
                return this.splitOrder();
            }
            
            //count if the number of braces is right
            if (this.rule.Split('{').Length - 1 != this.rule.Split('}').Length - 1)
            {
                MessageBoxResult result = MessageBox.Show("The number of opening and closing braces don't match", "Invalid rule");
                return null;
            }

            if (this.rule.Split('}').Length - 1 == 0)
            {
                MessageBoxResult result = MessageBox.Show("You have to use braces", "Invalid rule");
                return null;
            }

            List<Relations> relations = new List<Relations>();
            Relations.operations op = Relations.operations.NULL;
            
            //assume always using {}
            for (int x = 0; x < rule.Length; x++ )
            {
                if (rule[x] == '{')
                {
                    if(x + 1 < rule.Length && rule[x + 1] != '{')
                    {
                        relations.Add(new Relations());
                    }
                }
                else
                {
                    if (rule[x] == '}')
                    {
                        if (relations.Last().rule != null && relations.Last().rule.Trim() != "")
                        {
                            relations.Last().condition = this.validate(relations.Last().rule);

                            if (relations.Last().condition == null)
                            {
                                MessageBoxResult result = MessageBox.Show("There is an error with your rule: " + relations.Last().rule, "Error");
                                return null;
                            }
                        }

                        if (op != Relations.operations.NULL)
                        {
                            relations.Add(new Relations());
                            relations.Last().operation = op;
                            relations.Last().relation1 = relations.Count() - 3;
                            relations.Last().relation2 = relations.Count() - 2;

                            op = Relations.operations.NULL;
                        }

                        if (x + 2 < rule.Length)
                        {
                            if (rule[x + 1] == '&' && rule[x + 2] == '&')
                            {
                                op = Relations.operations.AND;
                            }

                            if (rule[x + 1] == '|' && rule[x + 2] == '|')
                            {
                                op = Relations.operations.OR;
                            }
                        }
                    }
                    else
                    {
                        if (rule[x] != '&' && rule[x] != '|')
                        {
                            relations.Last().rule += rule[x];
                        }
                    }
                }
            }

            relations.First().displayRule = this.originalRule;

            return relations;
        }

        public Conditions validate(String individualRule)
        {
            bool negation = false;

            if (individualRule[0] == '!')
            {
                negation = true;
                individualRule = individualRule.Substring(1);
            }

            foreach(String item in entities)
            {
                if (individualRule.IndexOf(item) == 0)
                {
                    //for boolean events
                    foreach (String action in boolEvents)
                    {
                        if (individualRule.IndexOf(item + "." + action + "(") == 0)
                        {
                            foreach (String item2 in entities)
                            {
                                if (individualRule.IndexOf(item + "." + action + "(" + item2 + ")") == 0)
                                {
                                    return this.createCondition(item, item2, action, individualRule, item + "." + action + "(" + item2 + ")", (!negation).ToString());
                                }
                            }

                            break;
                        }
                    }

                    //for valuable events
                    foreach (String action in valEvents)
                    {
                        if (individualRule.IndexOf(item + "." + action + "(") == 0)
                        {
                            foreach (String item2 in entities)
                            {
                                if (individualRule.IndexOf(item + "." + action + "(" + item2) == 0)
                                {
                                    foreach (String operators in valOperators)
                                    {
                                        if (individualRule.IndexOf(item + "." + action + "(" + item2 + ")" + operators) == 0)
                                        {
                                            return this.createCondition(item, item2, action, individualRule, item + "." + action + "(" + item2 + ")" + operators, operators);
                                        }
                                    }

                                    break;
                                }
                            }

                            break;
                        }
                    }

                    //for valuable properties
                    foreach (String action in valProperties)
                    {
                        if (individualRule.IndexOf(item + "." + action) == 0)
                        {
                            foreach (String operators in valOperators)
                            {
                                if (individualRule.IndexOf(item + "." + action + operators) == 0)
                                {
                                    return this.createCondition(item, null, action, individualRule, item + "." + action + operators, operators);
                                }
                            }

                            break;
                        }
                    }

                    //for bool properties
                    foreach (String action in boolProperties)
                    {
                        if (individualRule.IndexOf(item + "." + action) == 0)
                        {
                            if (individualRule.IndexOf(item + "." + action) == 0)
                            {
                                return this.createCondition(item, null, action, individualRule, item + "." + action, (!negation).ToString());
                            }

                            break;
                        }
                    }

                    break;
                }
            }

            return null;
        }

        private Conditions createCondition(String entityA, String entityB, String action, String originalRule, String rules, String operators)
        {
            int val = 0;
            int boolValue = -1;

            try
            {
                boolValue = Convert.ToBoolean(operators) ? 1 : 0;
            }
            catch (FormatException)
            {
                boolValue = -1;
            }

            if (boolValue == -1)
            {
                try
                {
                    val = Convert.ToInt32(originalRule.Substring(rules.Length));
                }
                catch (FormatException)
                {
                    return null;
                }
            }

            Conditions temp = new Conditions(entityA, entityB);
            PropertyInfo propertyInfo = temp.GetType().GetProperty(action);

            if(boolValue != -1)
            {
                propertyInfo.SetValue(temp, (boolValue == 1 ? true : false), null);
            }
            else
            {
                propertyInfo.SetValue(temp, val, null);
            }

            if (boolValue == -1)
            {
                temp.operators = operators;
            }

            return temp;
        }

        public static bool evaluate(Conditions cond, Package package)
        {
            if (cond == null)
            {
                MainWindow.error("Some internal error occurred when evaluating the conditions", true);
            }

            //if the event is about boolEvents
            if (cond.colliding || cond.pointing_at || cond.pointing_towards || cond.touching || cond.from || cond.towards || cond.parallel || cond.perpendicular)
            {
                if (package.getEntityA() == cond.entityA && package.getEntityB() == cond.entityB)
                {
                    if (cond.colliding && package is PackageCollision)
                    {
                        return true;
                    }

                    if (cond.pointing_at && package is PackagePointing && ((PackagePointing)package).getIsPointingAt())
                    {
                        return true;
                    }

                    if (cond.pointing_towards && package is PackagePointing && ((PackagePointing)package).getIsPointingToward())
                    {
                        return true;
                    }

                    if (cond.touching && package is PackagePointing && ((PackagePointing)package).getIsTouching())
                    {
                        return true;
                    }

                    if (cond.from && package is PackageDirection && ((PackageDirection)package).getIsAFromB())
                    {
                        return true;
                    }

                    if (cond.towards && ((package is PackageDirection && ((PackageDirection)package).getIsATowardsB())/* ||
                                         (package is PackageLocation && ((PackageLocation)package).getIsATowardsB())*/))
                    {
                        return true;
                    }

                    if (cond.parallel && ((package is PackageDirection && ((PackageDirection)package).getIsAParallelB())/* ||
                                         (package is PackageLocation && ((PackageLocation)package).getParallel())*/))
                    {
                        return true;
                    }

                    if (cond.perpendicular && package is PackageLocation && ((PackageLocation)package).getPerpendicular())
                    {
                        return true;
                    }
                }

                if (package.getEntityA() == cond.entityB && package.getEntityB() == cond.entityA)
                {
                    if (cond.towards)
                    {
                        //Console.WriteLine("towards " + cond.entityA + " " + package.getEntityB() + " - " + cond.entityB + " " + package.getEntityA() + " " + ((PackageDirection)package).getIsBTowardsA() + " " + ((PackageDirection)package).getIsATowardsB());
                    }

                    if (cond.from && package is PackageDirection && ((PackageDirection)package).getIsBFromA())
                    {
                        return true;
                    }

                    if (cond.towards && ((package is PackageDirection && ((PackageDirection)package).getIsBTowardsA()) /*||
                                         (package is PackageLocation && ((PackageLocation)package).getIsBTowardsA())*/))
                    {
                        return true;
                    }

                    if (cond.parallel && ((package is PackageDirection && ((PackageDirection)package).getIsBParallelA()) /*||
                                         (package is PackageLocation && ((PackageLocation)package).getParallel())*/))
                    {
                        return true;
                    }
                }
            }

            //if the event is about distance
            if (cond.distance != int.MinValue && cond.operators != null)
            {
                if (package.getEntityA() == cond.entityA && package.getEntityB() == cond.entityB)
                {
                    if (package is PackageLocation)
                    {
                        if (Parser.compare(((PackageLocation)package).getDistance(), cond.distance, cond.operators))
                        {
                            return true;
                        }
                    }

                    if (package is PackagePointing)
                    {
                        if (Parser.compare(((PackagePointing)package).getDistance(), cond.distance, cond.operators))
                        {
                            return true;
                        }
                    }
                }

                if (package.getEntityA() == cond.entityB && package.getEntityB() == cond.entityB)
                {
                    if (package is PackageLocation)
                    {
                        if (Parser.compare(((PackageLocation)package).getDistance(), cond.distance, cond.operators))
                        {
                            return true;
                        }
                    }

                    if (package is PackagePointing)
                    {
                        if (Parser.compare(((PackagePointing)package).getDistance(), cond.distance, cond.operators))
                        {
                            return true;
                        }
                    }
                }
            }

            //if the event is about velocityDifference
            if (cond.velocity_difference != int.MinValue && cond.operators != null)
            {
                if (package.getEntityA() == cond.entityA && package.getEntityB() == cond.entityB)
                {
                    if (package is PackageMotion)
                    {
                        if (Parser.compare(((PackageMotion)package).getVelocityDifference(), cond.velocity_difference, cond.operators))
                        {
                            return true;
                        }
                    }
                }
            }

            //if the event is about some non-boolean property
            if (cond.x != int.MinValue || cond.y != int.MinValue || cond.z != int.MinValue)
            {
                if (package.getEntityA() == cond.entityA)
                {
                    if (package is PackageLocation)
                    {
                        if (cond.x != int.MinValue && Parser.compare(((PackageLocation)package).getEntityAPosition().Item1, cond.x, cond.operators))
                        {
                            return true;
                        }

                        if (cond.y != int.MinValue && Parser.compare(((PackageLocation)package).getEntityAPosition().Item2, cond.y, cond.operators))
                        {
                            return true;
                        }

                        if (cond.z != int.MinValue && Parser.compare(((PackageLocation)package).getEntityAPosition().Item3, cond.z, cond.operators))
                        {
                            return true;
                        }
                    }

                    if (package is PackageDirection)
                    {
                        if (cond.x != int.MinValue && Parser.compare(((PackageDirection)package).getEntityAPosition().Item1, cond.x, cond.operators))
                        {
                            return true;
                        }

                        if (cond.y != int.MinValue && Parser.compare(((PackageDirection)package).getEntityAPosition().Item2, cond.y, cond.operators))
                        {
                            return true;
                        }

                        if (cond.z != int.MinValue && Parser.compare(((PackageDirection)package).getEntityAPosition().Item3, cond.z, cond.operators))
                        {
                            return true;
                        }
                    }
                }

                if (package.getEntityB() == cond.entityA)
                {
                    if (package is PackageLocation)
                    {
                        if (cond.x != int.MinValue && Parser.compare(((PackageLocation)package).getEntityBPosition().Item1, cond.x, cond.operators))
                        {
                            return true;
                        }

                        if (cond.y != int.MinValue && Parser.compare(((PackageLocation)package).getEntityBPosition().Item2, cond.y, cond.operators))
                        {
                            return true;
                        }

                        if (cond.z != int.MinValue && Parser.compare(((PackageLocation)package).getEntityBPosition().Item3, cond.z, cond.operators))
                        {
                            return true;
                        }
                    }

                    if (package is PackageDirection)
                    {
                        if (cond.x != int.MinValue && Parser.compare(((PackageDirection)package).getEntityBPosition().Item1, cond.x, cond.operators))
                        {
                            return true;
                        }

                        if (cond.y != int.MinValue && Parser.compare(((PackageDirection)package).getEntityBPosition().Item2, cond.y, cond.operators))
                        {
                            return true;
                        }

                        if (cond.z != int.MinValue && Parser.compare(((PackageDirection)package).getEntityBPosition().Item3, cond.z, cond.operators))
                        {
                            return true;
                        }
                    }
                }
            }

            if (cond.moving)
            {
                if (package is PackageMotion)
                {
                    if (package.getEntityA() == cond.entityA)
                    {
                        if (((PackageMotion)package).getIsAMoving())
                        {
                            return true;
                        }
                    }

                    if (package.getEntityB() == cond.entityA)
                    {
                        if (((PackageMotion)package).getIsBMoving())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool compare(double a, double b, String operators)
        {
            if (Array.IndexOf<String>(Parser.valOperators, operators) == -1)
            {
                return false;
            }

            if (operators == "<")
            {
                return a < b;
            }

            if (operators == "<=")
            {
                return a <= b;
            }

            if (operators == ">")
            {
                return a > b;
            }

            if (operators == ">=")
            {
                return a >= b;
            }

            if (operators == "==")
            {
                return a == b;
            }

            if (operators == "!=")
            {
                return a != b;
            }

            return false;
        }
    }
}