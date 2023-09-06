﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Symbioz.Core.DesignPattern.StartupEngine
{
    /// <summary>
    /// This class let you invoke 'public static' or 'public' (Singleton) methods at program startup by using the [StartupInvoke] Attribute.
    /// </summary>
    public class StartupManager : Singleton<StartupManager>
    {
        /// <summary>
        /// Singleton.cs
        /// </summary>
        public static string SingletonInstancePropretyName = "Instance";

        Logger logger = new Logger();

        public void Initialize(Assembly startupAssembly)
        {
            logger.Color1("-- Initialisation --");
            Stopwatch watch = Stopwatch.StartNew();

            foreach (var pass in Enum.GetValues(typeof(StartupInvokePriority)))
            {
                foreach (var item in startupAssembly.GetTypes())
                {
                    var methods = item.GetMethods().ToList().FindAll(x => x.GetCustomAttribute(typeof(StartupInvoke), false) != null);
                    var attributes = methods.ConvertAll<KeyValuePair<StartupInvoke, MethodInfo>>(x => new KeyValuePair<StartupInvoke, MethodInfo>(x.GetCustomAttribute(typeof(StartupInvoke), false) as StartupInvoke, x)).FindAll(x => x.Key.Type == (StartupInvokePriority)pass); ;

                    foreach (var data in attributes)
                    {
                        if (!data.Key.Hided)
                        {
                            logger.White("(" + pass + ") Loading " + data.Key.Name + " ...");
                        }

                        Delegate del = null;

                        if (data.Value.IsStatic)
                        {
                            del = Delegate.CreateDelegate(typeof(Action), data.Value);
                        }
                        else
                        {
                            PropertyInfo field = data.Value.DeclaringType.BaseType.GetProperty(SingletonInstancePropretyName);
                            Object singletonInstance = field.GetValue(null);
                            del = Delegate.CreateDelegate(typeof(Action), singletonInstance, data.Value.Name);
                        }
                        try
                        {
                            del.DynamicInvoke();
                        }
                        catch (Exception ex)
                        {
                            logger.Error(ex.ToString());
                            Console.ReadKey();
                            Environment.Exit(0);
                            return;
                        }
                    }


                }
            }
            watch.Stop();
            logger.Color1("-- Initialisation Complete (" + watch.Elapsed.Minutes + "min" + watch.Elapsed.Seconds + "s) --");
        }
    }
}
