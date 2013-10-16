using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;

namespace MefExtensions
{
    public static class MefExtensionHelper
    {
        public static IEnumerable<Lazy<TtoExport, Tmetadata>> GetExports<TtoExport, Tmetadata>(
            this CompositionContainer container,
            Predicate<Tmetadata> predicate)
            where Tmetadata : class
            where TtoExport : class
        {
            var re =
                from cpd in container.Catalog.Parts
                from ed in cpd.ExportDefinitions
                where ed.ContractName == typeof (TtoExport).FullName
                let attribute = GetAttribute<TtoExport, Tmetadata>(ed)
                where attribute != null && predicate(attribute)
                select new Lazy<TtoExport, Tmetadata>(
                    () =>
                        (TtoExport) cpd.CreatePart().GetExportedValue(ed),
                    attribute);

            return re.AsEnumerable();
        }

        public static IEnumerable<LazyFactory<TtoExport, Tmetadata>> GetExportFactories<TtoExport, Tmetadata>(
            this CompositionContainer container,
            Predicate<Tmetadata> predicate)
            where Tmetadata : class
            where TtoExport : class
        {
            var re =
                from cpd in container.Catalog.Parts
                from ed in cpd.ExportDefinitions
                where ed.ContractName == typeof (TtoExport).FullName
                let attribute = GetAttribute<TtoExport, Tmetadata>(ed)
                where attribute != null && predicate(attribute)
                select new LazyFactory<TtoExport, Tmetadata>(
                    () =>
                        (TtoExport) cpd.CreatePart().GetExportedValue(ed),
                    attribute);

            return re.AsEnumerable();
        }

        public static IEnumerable<TtoExport> GetExportValues<TtoExport, Tmetadata>(
            this CompositionContainer container,
            Predicate<Tmetadata> predicate)
            where Tmetadata : class
            where TtoExport : class
        {
            var re =
                from cpd in container.Catalog.Parts
                from ed in cpd.ExportDefinitions
                where ed.ContractName == typeof (TtoExport).FullName
                let attribute = GetAttribute<TtoExport, Tmetadata>(ed)
                where attribute != null && predicate(attribute)
                select (TtoExport) cpd.CreatePart().GetExportedValue(ed);

            return re.AsEnumerable();
        }

        /// <summary>
        ///     From the provided export definition, returns the attribute of
        ///     the specified type attached. (If such an attribute is defined on
        ///     the part).
        /// </summary>
        /// <typeparam name="TtoExport">The type of to export.</typeparam>
        /// <typeparam name="Tmetadata">
        ///     The type of the metadata.
        /// </typeparam>
        /// <param name="exportDefinition">The export definition.</param>
        /// <returns>
        /// </returns>
        private static Tmetadata GetAttribute<TtoExport, Tmetadata>(ExportDefinition exportDefinition)
            where Tmetadata : class
            where TtoExport : class
        {
            //At this point we need type information for the type related to this export definition
            //Through reflection and debugging it appears a private property called _origin has this info.
            //_origin is a member of the ReflectionMemberExportDefinition class
            var reflectedType = exportDefinition.GetType();
            var fields = reflectedType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var originField = fields.FirstOrDefault(x => x.Name.Contains("origin"));
            var originValue = originField.GetValue(exportDefinition);
            //The _origin value itself has a field called _origin which is an AssemblyCatalog
            var reflectedType2 = originValue.GetType();
            var fields2 = reflectedType2.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var originField2 = fields2.FirstOrDefault(x => x.Name.Contains("origin"));
            var originValue2 = (AssemblyCatalog) originField2.GetValue(originValue);
            //Now we have the assembly which contains the part we are looking for.
            //So let's combine the assembly and the type name and then we can get reflection on that type.
            var metadataType = typeof (Tmetadata);
            var name = exportDefinition.ToString();
            var indexOfSpace = name.IndexOf(' ');
            var fullTypeName = name.Substring(0, indexOfSpace);
            var assemblyTypes = originValue2.Assembly.GetTypes().ToList();
            assemblyTypes.ForEach(x => Console.WriteLine(x.FullName));
            var typeInfo = assemblyTypes.FirstOrDefault(x => x.FullName == fullTypeName);
            //Now we have the type Info for the metadata we wish to examine so let's retrieve it.
            var attribute =
                (Tmetadata) typeInfo.GetCustomAttributes(typeof (Tmetadata), true).FirstOrDefault();
            return attribute;
        }
    }
}