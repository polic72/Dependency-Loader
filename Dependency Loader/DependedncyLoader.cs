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
        /// The criteria used to resolve assembies. 
        /// </summary>
        public ResolveCriteria Criteria { get; }

        #endregion Properties


        /// <summary>
        /// Whether or not this dependency loader was started.
        /// </summary>
        private bool started;


        /// <summary>
        /// The assembly names cached based on file-path. This will dramatically improve performance.
        /// </summary>
        protected Dictionary<string, AssemblyName> cached_assemblyNames;


        #region Constructors

        /// <summary>
        /// Constructs a <see cref="Dependency_Loader.DependedncyLoader"/> for the given <paramref name="appDomain"/> in the given 
        /// <paramref name="root"/> directory.
        /// </summary>
        /// <param name="appDomain">The appdomain to load the dependency assemblies into.</param>
        /// <param name="root">The root directory to search for dependencies in. All child directories are searched too.</param>
        /// <param name="criteria">The criteria used to resolve assembies.</param>
        /// <exception cref="System.ArgumentNullException">When any of the given parameters are null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">When the given <paramref name="root"/> directory does not exist.</exception>
        public DependedncyLoader(AppDomain appDomain, string root, ResolveCriteria criteria)
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
            Criteria = criteria;


            started = false;

            cached_assemblyNames = new Dictionary<string, AssemblyName>();
        }


        /// <summary>
        /// Constructs a <see cref="Dependency_Loader.DependedncyLoader"/> for the <see cref="System.AppDomain.CurrentDomain"/> in the given 
        /// <paramref name="root"/> directory.
        /// </summary>
        /// <param name="root">The root directory to search for dependencies in. All child directories are searched too.</param>
        /// <param name="criteria">The criteria used to resolve assembies.</param>
        /// <exception cref="System.ArgumentNullException">When any of the given parameters are null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">When the given <paramref name="root"/> directory does not exist.</exception>
        public DependedncyLoader(string root, ResolveCriteria criteria)
            : this(AppDomain.CurrentDomain, root, criteria)
        {
            //Do nothing.
        }

        #endregion Constructors


        #region Start/Stop

        /// <summary>
        /// Starts this dependency loader. Caches the info of all assemblies in root after the first run.
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


            started = true;

            return true;
        }


        /// <summary>
        /// Stops this dependency loader. Clears the cached assembly information.
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


            cached_assemblyNames.Clear();


            started = false;

            return true;
        }

        #endregion Start/Stop


        private Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            AssemblyName target_name = new AssemblyName(args.Name);


            return SearchDirectory(target_name, Root);
        }


        /// <summary>
        /// Searches the given <paramref name="directory_path"/> for the given <paramref name="target_name"/>.
        /// </summary>
        /// <param name="target_name">The target assembly name to search for.</param>
        /// <param name="directory_path">The path of the directory to search through.</param>
        /// <returns>The found assembly. Null if it could not be found.</returns>
        /// <exception cref="System.ArgumentNullException">When any of the given parameters are null.</exception>
        /// <remarks>
        /// Will recursively look through child directories too.
        /// </remarks>
        private Assembly SearchDirectory(AssemblyName target_name, string directory_path)
        {
            #region Error Checking

            if (target_name == null)
            {
                throw new ArgumentNullException(nameof(target_name), "The given " + nameof(target_name) + " is null.");
            }


            if (directory_path == null)
            {
                throw new ArgumentNullException(nameof(directory_path), "The given " + nameof(directory_path) + " is null.");
            }

            #endregion Error Checking


            #region Files

            string[] files = Directory.GetFiles(directory_path);

            foreach (string file in files)
            {
                AssemblyName assemblyName = null;


                if (cached_assemblyNames.ContainsKey(file))
                {
                    assemblyName = cached_assemblyNames[file];
                }
                else
                {
                    try
                    {
                        assemblyName = AssemblyName.GetAssemblyName(file);
                    }
                    catch
                    {
                        continue;
                    }
                    finally
                    {
                        cached_assemblyNames.Add(file, assemblyName);
                    }
                }


                if (assemblyName == null)
                {
                    continue;
                }


                #region Check Criteria

                //Name:
                if (Criteria.HasFlag(ResolveCriteria.Name))
                {
                    if (assemblyName.Name != target_name.Name)
                    {
                        continue;
                    }
                }


                //Version:
                if (Criteria.HasFlag(ResolveCriteria.Version))
                {
                    if (assemblyName.Version != target_name.Version)
                    {
                        continue;
                    }
                }


                //Culture:
                if (Criteria.HasFlag(ResolveCriteria.Culture))
                {
                    if (assemblyName.CultureName != target_name.CultureName)
                    {
                        continue;
                    }
                }


                //PublicKey:
                if (Criteria.HasFlag(ResolveCriteria.PublicKey))
                {
                    byte[] possible_token = assemblyName.GetPublicKey();
                    byte[] target_token = target_name.GetPublicKey();

                    if (possible_token.Length != target_token.Length)
                    {
                        continue;
                    }


                    bool same_token = true;

                    for (int i = 0; i < possible_token.Length; ++i)
                    {
                        if (possible_token[i] != target_token[i])
                        {
                            same_token = false;
                            break;
                        }
                    }

                    if (!same_token) continue;
                }

                #endregion Check Criteria


                return Assembly.LoadFrom(file);
            }

            #endregion Files


            #region Directories

            string[] directories = Directory.GetDirectories(directory_path);

            foreach (string directory in directories)
            {
                Assembly assembly = SearchDirectory(target_name, directory);

                if (assembly != null)
                {
                    return assembly;
                }
            }

            #endregion Directories

            return null;
        }
    }
}
