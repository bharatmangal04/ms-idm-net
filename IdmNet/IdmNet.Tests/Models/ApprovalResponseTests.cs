using System;
using System.Collections.Generic;
using IdmNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UseObjectOrCollectionInitializer

namespace IdmNet.Models.Tests
{
    [TestClass]
    public class ApprovalResponseTests
    {
        private ApprovalResponse _it;

        public ApprovalResponseTests()
        {
            _it = new ApprovalResponse();
        }

        [TestMethod]
        public void It_has_a_paremeterless_constructor()
        {
            Assert.AreEqual("ApprovalResponse", _it.ObjectType);
        }

        [TestMethod]
        public void It_has_a_constructor_that_takes_an_IdmResource()
        {
            var resource = new IdmResource
            {
                DisplayName = "My Display Name",
                Creator = new Person { DisplayName = "Creator Display Name", ObjectID = "Creator ObjectID"},
            };
            var it = new ApprovalResponse(resource);

            Assert.AreEqual("ApprovalResponse", it.ObjectType);
            Assert.AreEqual("My Display Name", it.DisplayName);
            Assert.AreEqual("Creator Display Name", it.Creator.DisplayName);
        }

        [TestMethod]
        public void It_has_a_constructor_that_takes_an_IdmResource_without_Creator()
        {
            var resource = new IdmResource
            {
                DisplayName = "My Display Name",
            };
            var it = new ApprovalResponse(resource);

            Assert.AreEqual("My Display Name", it.DisplayName);
            Assert.IsNull(it.Creator);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void It_throws_when_you_try_to_set_ObjectType_to_anything_other_than_its_primary_ObjectType()
        {
            _it.ObjectType = "Invalid Object Type";
        }

        [TestMethod]
        public void It_has_Approval_which_is_null_by_default()
        {
            // Assert
            Assert.IsNull(_it.Approval);
        }

        [TestMethod]
        public void It_has_Approval_which_can_be_set_back_to_null()
        {
            // Arrange
            var testApproval = new Approval { DisplayName = "Test Approval" };			
            _it.Approval = testApproval; 

            // Act
            _it.Approval = null;

            // Assert
            Assert.IsNull(_it.Approval);
        }

        [TestMethod]
        public void It_can_get_and_set_Approval()
        {
            // Act
			var testApproval = new Approval { DisplayName = "Test Approval" };			
            _it.Approval = testApproval; 

            // Assert
            Assert.AreEqual(testApproval.DisplayName, _it.Approval.DisplayName);
        }


        [TestMethod]
        public void It_has_ComputedActor_which_is_null_by_default()
        {
            // Assert
            Assert.IsNull(_it.ComputedActor);
        }

        [TestMethod]
        public void It_has_ComputedActor_which_can_be_set_back_to_null()
        {
            // Arrange
            var list = new List<IdmResource>
            {
                new IdmResource { DisplayName = "Test IdmResource1", ObjectID = "guid1" },
                new IdmResource { DisplayName = "Test IdmResource2", ObjectID = "guid2" }
            };
            _it.ComputedActor = list;

            // Act
            _it.ComputedActor = null;

            // Assert
            Assert.IsNull(_it.ComputedActor);
        }

        [TestMethod]
        public void It_can_get_and_set_ComputedActor()
        {
            // Arrange
            var list = new List<IdmResource>
            {
                new IdmResource { DisplayName = "Test IdmResource1", ObjectID = "guid1" },
                new IdmResource { DisplayName = "Test IdmResource2", ObjectID = "guid2" }
            };

            // Act
            _it.ComputedActor = list;

            // Assert
            Assert.AreEqual(list[0].DisplayName, _it.ComputedActor[0].DisplayName);
            Assert.AreEqual(list[1].DisplayName, _it.ComputedActor[1].DisplayName);
        }


        [TestMethod]
        public void It_can_get_and_set_Decision()
        {
            // Act
            _it.Decision = "A string";

            // Assert
            Assert.AreEqual("A string", _it.Decision);
        }


        [TestMethod]
        public void It_can_get_and_set_Reason()
        {
            // Act
            _it.Reason = "A string";

            // Assert
            Assert.AreEqual("A string", _it.Reason);
        }


        [TestMethod]
        public void It_has_Requestor_which_is_null_by_default()
        {
            // Assert
            Assert.IsNull(_it.Requestor);
        }

        [TestMethod]
        public void It_has_Requestor_which_can_be_set_back_to_null()
        {
            // Arrange
            var testIdmResource = new IdmResource { DisplayName = "Test IdmResource" };			
            _it.Requestor = testIdmResource; 

            // Act
            _it.Requestor = null;

            // Assert
            Assert.IsNull(_it.Requestor);
        }

        [TestMethod]
        public void It_can_get_and_set_Requestor()
        {
            // Act
			var testIdmResource = new IdmResource { DisplayName = "Test IdmResource" };			
            _it.Requestor = testIdmResource; 

            // Assert
            Assert.AreEqual(testIdmResource.DisplayName, _it.Requestor.DisplayName);
        }


    }
}
