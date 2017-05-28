using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TripToPrint.Core;
using TripToPrint.Core.Models;
using TripToPrint.Presenters;
using TripToPrint.ViewModels;

namespace TripToPrint.Tests
{
    [TestClass]
    public class KmlObjectsTreePresenterTests
    {
        private readonly Mock<IKmlCalculator> _kmlCalculatorMock = new Mock<IKmlCalculator>();

        private Mock<KmlObjectsTreePresenter> _presenter;

        [TestInitialize]
        public void TestInitialize()
        {
            _presenter = new Mock<KmlObjectsTreePresenter>(
                _kmlCalculatorMock.Object) {
                CallBase = true
            };
        }

        [TestMethod]
        public void When_handling_activation_the_folderstoinclude_are_populated_correctly()
        {
            // Arrange
            var kmlDocument = new KmlDocument {
                Folders = {
                    new KmlFolder("folder-1", new[] {
                        new KmlPlacemark { Name = "pm-1" },
                        new KmlPlacemark()
                    }),
                    new KmlFolder(new[] {
                        new KmlPlacemark()
                    })
                }
            };
            _kmlCalculatorMock.Setup(x => x.CompleteFolderIsRoute(kmlDocument.Folders[1])).Returns(true);
            var vm = new KmlObjectsTreeViewModel();
            _presenter.SetupGet(x => x.ViewModel).Returns(vm);

            // Act
            _presenter.Object.HandleActivated(kmlDocument, new CancellationToken());

            // Verify
            Assert.AreEqual(2, vm.FoldersToInclude.Count);
            AssertKmlFolderNodeViewModel(kmlDocument.Folders[0], vm.FoldersToInclude[0], false);
            AssertKmlFolderNodeViewModel(kmlDocument.Folders[1], vm.FoldersToInclude[1], true);
        }

        private void AssertKmlFolderNodeViewModel(KmlFolder kmlFolder, KmlObjectTreeNodeViewModel viewModel, bool isPartOfRoute)
        {
            Assert.AreEqual(kmlFolder, viewModel.Element);
            Assert.AreEqual(kmlFolder.Name, viewModel.Name);
            Assert.AreEqual(isPartOfRoute, viewModel.IsPartOfRoute);
            Assert.AreEqual(kmlFolder.Placemarks.Count, viewModel.Children.Count);
            for (var i = 0; i < viewModel.Children.Count; i++)
            {
                Assert.AreEqual(kmlFolder.Placemarks[i], viewModel.Children[i].Element);
                Assert.AreEqual(kmlFolder.Placemarks[i].Name, viewModel.Children[i].Name);
                Assert.AreEqual(isPartOfRoute, viewModel.Children[i].IsPartOfRoute);
                Assert.IsTrue(viewModel.Children[i].Enabled);
            }

            Assert.IsTrue(viewModel.Enabled);
        }
    }
}
