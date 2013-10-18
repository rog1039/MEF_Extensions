using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MefExtensions;

namespace MefExtensions.Tests
{
    public class TestMefExtensionHelpers
    {
        [Fact]
        public void StandardMefBehavior_ShouldWork()
        {
            //Arrange
            var catalog = new AggregateCatalog(
                new DirectoryCatalog("."),
                new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(new AssemblyCatalog(Assembly.GetAssembly(typeof(TestMefExtensionHelpers))));
            
            //Act
            var myObjects = container.GetExports<MyObjectBaseClass>();

            //Assert
            Assert.Equal(3, myObjects.Count());
        }

        [Fact]
        public void GetExports_With_A_Predicate_Should_Return_Filtered_Results()
        {
            //Arrange
            var catalog = new AggregateCatalog(
                new DirectoryCatalog("."),
                new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(new AssemblyCatalog(Assembly.GetAssembly(typeof(TestMefExtensionHelpers))));

            //Act
            var myAObjects =
                container.GetExports<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A");
            var myBObjects =
                container.GetExports<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "B");
            var myAObjectWithNameTwo =
                container.GetExports<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A" && attribute.Name == "Two");

            //Assert
            Assert.Equal(2, myAObjects.Count());
            Assert.Equal(1, myBObjects.Count());
            Assert.Equal("B", myBObjects.First().Metadata.Category);
            Assert.Equal("One", myBObjects.First().Metadata.Name);
            Assert.Equal(1, myAObjectWithNameTwo.Count());

            //And let's make sure we can create a value and access metadata.
            Assert.Equal("B", myBObjects.First().Metadata.Category);
            Assert.NotNull(myBObjects.First().Value);
        }

        [Fact]
        public void GetExportValues_With_A_Predicate_Should_Return_Filtered_Results()
        {
            //Arrange
            var catalog = new AggregateCatalog(
                new DirectoryCatalog("."),
                new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(new AssemblyCatalog(Assembly.GetAssembly(typeof(TestMefExtensionHelpers))));

            //Act
            var myAObjects =
                container.GetExportValues<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A");
            var myBObjects =
                container.GetExportValues<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "B");
            var myAObjectWithNameTwo =
                container.GetExportValues<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A" && attribute.Name == "Two");

            //Assert
            Assert.Equal(2, myAObjects.Count());
            Assert.Equal(1, myBObjects.Count());
            Assert.Equal(1, myAObjectWithNameTwo.Count());
        }

        [Fact]
        public void GetExportFactories_With_A_Predicate_Should_Return_Filtered_Results()
        {
            //Arrange
            var catalog = new AggregateCatalog(
                new DirectoryCatalog("."),
                new AssemblyCatalog(Assembly.GetExecutingAssembly()));

            var container = new CompositionContainer(new AssemblyCatalog(Assembly.GetAssembly(typeof(TestMefExtensionHelpers))));

            //Act
            var myAObjects =
                container.GetExportFactories<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A");
            var myBObjects =
                container.GetExportFactories<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "B");
            var myAObjectWithNameTwo =
                container.GetExportFactories<MyObjectBaseClass, MyMetadataAttribute>(attribute => attribute.Category == "A" && attribute.Name == "Two");

            //Assert
            Assert.Equal(2, myAObjects.Count());
            Assert.Equal(1, myBObjects.Count());
            Assert.Equal(1, myAObjectWithNameTwo.Count());

            //And let's check that we can create a bunch of objects from a factory
            Assert.NotNull(myBObjects.First().Create());
            Assert.NotNull(myBObjects.First().Create());
            Assert.NotNull(myBObjects.First().Create());
            Assert.NotNull(myBObjects.First().Create());

            //A different change.
        }
    }

    public class MyObjectBaseClass
    {
        
    }

    [MetadataAttribute]
    public class MyMetadataAttribute : InheritedExportAttribute
    {
        public MyMetadataAttribute()
            : base(typeof(MyObjectBaseClass)) 
        {
            
        }

        public string Category { get; set; }
        public string Name { get; set; }
    }

    [MyMetadata(Category = "A", Name = "One")]
    public class CategoryAObjectOneObject : MyObjectBaseClass
    {
        
    }

    [MyMetadata(Category = "A", Name = "Two")]
    public class CategoryAObjectTwoObject : MyObjectBaseClass
    {

    }

    [MyMetadata(Category = "B", Name = "One")]
    public class CategoryBObjectOneObject : MyObjectBaseClass
    {

    }
}
