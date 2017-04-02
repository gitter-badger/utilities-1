﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using FizzWare.NBuilder.FunctionalTests.Model;
using FizzWare.NBuilder.FunctionalTests.Support;
using NUnit.Framework;

namespace FizzWare.NBuilder.FunctionalTests
{
    [TestFixture]
    public class RepositoryPersistenceTests
    {
        [OneTimeSetUp]
        public void BeforeEach()
        {

        }

        [Test]
        public void PersistingASingleObject()
        {
            var builderSetup =new RepositoryBuilderSetup().SetUp();
            new Builder(builderSetup).CreateNew< Product>().Persist();

            // Go directly to the database to do some asserts
            var dataTable = Database.GetContentsOf(Database.Tables.Product);

            Assert.That(dataTable.Rows.Count, Is.EqualTo(1));

            Assert.That(dataTable.Rows[0]["Title"], Is.EqualTo("Title1"));
            Assert.That(dataTable.Rows[0]["Description"], Is.EqualTo("Description1"));
            Assert.That(dataTable.Rows[0]["PriceBeforeTax"], Is.EqualTo(1m));
            Assert.That(dataTable.Rows[0]["QuantityInStock"], Is.EqualTo(1));
        }

        [Test]
        public void PersistingASingleTaxTypeAndAListOf100Products()
        {
            var builderSetup = new RepositoryBuilderSetup().SetUp();
            var taxType = new Builder(builderSetup).CreateNew< TaxType>().Persist();

            new Builder(builderSetup).CreateListOfSize< Product>(100)
                .All()
                    .With(x => x.TaxType = taxType)
                .Persist(); // NB: Persistence is setup in the RepositoryBuilderSetup class

            var dbProducts = Database.GetContentsOf(Database.Tables.Product);

            Assert.That(dbProducts.Rows.Count, Is.EqualTo(100));
        }

        [Test]
        [Description("Instead of assigning directly to a category")]
        public void PersistingUsingPick_UpTo_AndDoForEach()
        {
            var builderSetup = new RepositoryBuilderSetup().SetUp();
            var categories = new Builder(builderSetup).CreateListOfSize< Category>(10).Persist();

            new Builder(builderSetup)
                .CreateListOfSize< Product>(50)
                .All()
                .DoForEach((x, y) => x.AddToCategory(y), Pick<Category>.UniqueRandomList(With.UpTo(4).Elements).From(categories))
                .Persist();

            DataTable productCategoriesTable = Database.GetContentsOf(Database.Tables.ProductCategory);

            Assert.That(productCategoriesTable.Rows.Count, Is.LessThanOrEqualTo(50 * 4));
        }

        [Test]
        public void PersistingAListOfProductsAndCategories()
        {
            var builderSetup = new RepositoryBuilderSetup().SetUp();
            const int numProducts = 500;
            const int numCategories = 50;
            const int numCategoriesForEachProduct = 5;

            var categories = new Builder(builderSetup)
                .CreateListOfSize< Category>(numCategories)
                .Persist();

            new Builder(builderSetup)
                 .CreateListOfSize< Product>(numProducts)
                .All()
                    .With(x => x.Categories = Pick<Category>.
                        UniqueRandomList(With.Exactly(numCategoriesForEachProduct).Elements)
                        .From(categories)
                        .ToList())
                .Persist(); // NB: Persistence is setup in the RepositoryBuilderSetup class

            DataTable productsTable = Database.GetContentsOf(Database.Tables.Product);
            DataTable categoriesTable = Database.GetContentsOf(Database.Tables.Category);
            DataTable productCategoriesTable = Database.GetContentsOf(Database.Tables.ProductCategory);

            Assert.That(productsTable.Rows.Count, Is.EqualTo(numProducts), "products");
            Assert.That(categoriesTable.Rows.Count, Is.EqualTo(numCategories), "categories");
            Assert.That(productCategoriesTable.Rows.Count, Is.EqualTo(numCategoriesForEachProduct * numProducts));
        }

        // TODO: Add CreatingAHierarchyOfCategoriesAndAddingProducts
    }
}