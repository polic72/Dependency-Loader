using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependency_Loader
{
    /// <summary>
    /// Represents the criteria used when resolving assemblies in <see cref="Dependency_Loader.DependedncyLoader"/>.
    /// </summary>
    [Flags]
    public enum ResolveCriteria
    {
        /// <summary>
        /// Checks to see if the assemblies share the same name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="System.Reflection.AssemblyName.Name"/> to check.
        /// </remarks>
        Name = 1,


        /// <summary>
        /// Checks to see if the assemblies share the same version.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="System.Reflection.AssemblyName.Version"/> to check.
        /// </remarks>
        Version = 2,


        /// <summary>
        /// Checks to see if the assemblies share the same culture name.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="System.Reflection.AssemblyName.CultureName"/> to check.
        /// </remarks>
        Culture = 4,


        /// <summary>
        /// Checks to see if the assemblies share the same public key.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="System.Reflection.AssemblyName.GetPublicKey()"/> to check.
        /// </remarks>
        PublicKey = 8,



        /// <summary>
        /// Checks to see if the assemblies share all info in every other flag.
        /// </summary>
        /// <remarks>
        /// See <see cref="Dependency_Loader.ResolveCriteria.Name"/>, <see cref="Dependency_Loader.ResolveCriteria.Version"/>, 
        /// <see cref="Dependency_Loader.ResolveCriteria.Culture"/>, and <see cref="Dependency_Loader.ResolveCriteria.PublicKeyToken"/>.
        /// </remarks>
        All = 15
    }
}
