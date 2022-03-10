using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Dependency_Loader;


namespace Dependency_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    Console.Error.WriteLine("A path to an assembly to test must be passed, as well as the directory to for dependencies.");

                    return;
                }


                Console.WriteLine("Running test...\n\n");


                DependedncyLoader loader = new DependedncyLoader(args[1], ResolveCriteria.Name | ResolveCriteria.Version);

                loader.Start();

                Assembly assembly = Assembly.LoadFrom(args[0]);


                Type[] types = assembly.GetTypes();


                loader.Stop();


                Console.WriteLine("Test successful! Press any key to exit...");

                Console.ReadKey(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Test failed! Here's why:\n");


                if (e is ReflectionTypeLoadException loadException)
                {
                    foreach (Exception exception in loadException.LoaderExceptions)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        if (exception is FileNotFoundException foundException)
                        {
                            Console.WriteLine("Cannot find " + foundException.FileName);
                        }
                        else
                        {
                            Console.WriteLine(exception.StackTrace);
                        }

                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine(e.StackTrace);

                    Console.ForegroundColor = ConsoleColor.White;
                }


                Console.WriteLine("\n\nPress any key to exit...");

                Console.ReadKey(true);
            }
        }
    }
}
