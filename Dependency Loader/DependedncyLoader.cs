using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Dependency_Loader
{
    /// <summary>
    /// Represents a dependency loader that automatically searches folders for dependencies.
    /// </summary>
    /// <remarks>
    /// Note that the loader will use the first assembly it finds that fits the criteria.
    /// </remarks>
    public class DependedncyLoader
    {
        #region Properties

        /// <summary>
        /// The <see cref="System.AppDomain"/> this <see cref="Dependency_Loader.DependedncyLoader"/> is working under.
        /// </summary>
        public AppDomain AppDomain { get; }


        /// <summary>
        /// The root directory to search for dependencies in.
        /// </summary>
        /// <remarks>
        /// All child directories are searched too.
        /// </remarks>
        public string Root { get; }


        /// <summary>
        /// Whether or not to ignore all information but the name when resolving a dependency.
        /// </summary>
        public bool OnlyUseName { get; }

        #endregion Properties


        /// <summary>
        /// Whether or not this dependency loader was started.
        /// </summary>
        private bool started;


        /// <summary>
        /// The regex used to parse assembly name information.
        /// </summary>
        protected static readonly Regex assemblyName_regex = new Regex(@"(.+), Version=(.+), Culture=(.+), PublicKeyToken=(.+)", RegexOptions.Compiled);


        #region Constructors

        /// <summary>
        /// Constructs a <see cref="Dependency_Loader.DependedncyLoader"/> for the given <paramref name="appDomain"/> in the given 
        /// <paramref name="root"/> directory.
        /// </summary>
        /// <param name="appDomain">The appdomain to load the dependency assemblies into.</param>
        /// <param name="root">The root directory to search for dependencies in. All child directories are searched too.</param>
        /// <exception cref="System.ArgumentNullException">When any of the given parameters are null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">When the given <paramref name="root"/> directory does not exist.</exception>
        public DependedncyLoader(AppDomain appDomain, string root)
        {
            #region Error Checking

            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(appDomain), "The given " + nameof(appDomain) + " is null.");
            }


            if (appDomain == null)
            {
                throw new ArgumentNullException(nameof(root), "The given " + nameof(root) + " is null.");
            }


            if (!Directory.Exists(root))
            {
                throw new DirectoryNotFoundException("The given " + nameof(root) + " directory (" + root + ") does not exist.");
            }

            #endregion Error Checking


            AppDomain = appDomain;

            Root = root;
        }


        /// <summary>
        /// Constructs a <see cref="Dependency_Loader.DependedncyLoader"/> for the <see cref="System.AppDomain.CurrentDomain"/> in the given 
        /// <paramref name="root"/> directory.
        /// </summary>
        /// <param name="root">The root directory to search for dependencies in. All child directories are searched too.</param>
        /// <exception cref="System.ArgumentNullException">When any of the given parameters are null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">When the given <paramref name="root"/> directory does not exist.</exception>
        public DependedncyLoader(string root)
            : this(AppDomain.CurrentDomain, root)
        {
            //Do nothing.
        }

        #endregion Constructors


        #region Start/Stop

        /// <summary>
        /// Starts this dependency loader.
        /// </summary>
        /// <returns>True if the loader was successfully started. False if it was already started.</returns>
        /// <remarks>
        /// Now whenever the <see cref="System.AppDomain.AssemblyResolve"/> event is fired, the 
        /// <see cref="Dependency_Loader.DependedncyLoader.Root"/> directory is searched.
        /// </remarks>
        public bool Start()
        {
            if (started)
            {
                return false;
            }


            AppDomain.AssemblyResolve += AssemblyResolve;

            return true;
        }


        /// <summary>
        /// Stops this dependency loader.
        /// </summary>
        /// <returns>True if the loader was successfully stopped. False if it wasn't started.</returns>
        /// <remarks>
        /// Now whenever the <see cref="System.AppDomain.AssemblyResolve"/> event is fired, the 
        /// <see cref="Dependency_Loader.DependedncyLoader.Root"/> directory is no longer searched.
        /// </remarks>
        public bool Stop()
        {
            if (!started)
            {
                return false;
            }


            AppDomain.AssemblyResolve -= AssemblyResolve;

            return true;
        }

        #endregion Start/Stop


        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            foreach (string file in Directory.GetFiles(Root))
            {
                AssemblyName name = AssemblyName.GetAssemblyName(file);

                if (AssemblyName.ReferenceMatchesDefinition(name, args.Name))
                {
                    //Working on it.
                }
            }
        }
    }
}
