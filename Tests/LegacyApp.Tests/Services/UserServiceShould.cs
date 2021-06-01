namespace LegacyApp.Tests.Services
{
    using LegacyApp.Repositories;

    using Moq;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Xunit;

    public class UserServiceShould
    {
        private Mock<IClientRepository> clientRepositoryMock;
        private Mock<IUserCreditService> userCreditService;
        private Mock<IUserRepository> userRepositoryMock;
        private UserService userService;

        public UserServiceShould()
        {
            this.clientRepositoryMock = new Mock<IClientRepository>();
            this.userCreditService = new Mock<IUserCreditService>();
            this.userRepositoryMock = new Mock<IUserRepository>();
            this.userService = new UserService(clientRepositoryMock.Object, userCreditService.Object, userRepositoryMock.Object);
        }

        [Fact]
        public void ShouldRejectDueToEmail()
        {
            var client = new Client() { Name = "VeryImportantClient" };
            clientRepositoryMock.Setup(c => c.GetById(It.IsAny<int>())).Returns(client);

            var date = new DateTime(1993, 1, 1);
            var addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail", date, 4);
            Assert.False(addResult);

            addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail@.com", date, 4);
            Assert.False(addResult);

            addResult = this.userService.AddUser("John", "Doe", "@gmail.com", date, 4);
            Assert.False(addResult);

            addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail.com", date, 4);
            Assert.True(addResult);
        }

        [Fact]
        public void ShouldRejectDueToAge()
        {
            var client = new Client() { Name = "VeryImportantClient" };
            clientRepositoryMock.Setup(c => c.GetById(It.IsAny<int>())).Returns(client);

            var date = new DateTime(2010, 1, 1);
            var addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail.com", date, 4);
            Assert.False(addResult);

            addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail.com", DateTime.Today.AddYears(-21), 4);
            Assert.True(addResult);
        }

        [Fact]
        public void ShouldRejectDueToFirstName()
        {

            var date = new DateTime(1990, 1, 1);
            var addResult = this.userService.AddUser(string.Empty, "Doe", "John.doe@gmail.com", date, 4);
            Assert.False(addResult);
        }

        [Fact]
        public void ShouldRejectDueToSurname()
        {

            var date = new DateTime(1990, 1, 1);
            var addResult = this.userService.AddUser("John", string.Empty, "John.doe@gmail.com", date, 4);
            Assert.False(addResult);
        }

        [Fact]
        public void ShouldRejectDueToCreditLimit()
        {
            var client = new Client { };
            clientRepositoryMock.Setup(c => c.GetById(It.IsAny<int>())).Returns(client);
            var date = new DateTime(1990, 1, 1);
            var addResult = this.userService.AddUser("John", string.Empty, "John.doe@gmail.com", date, 4);
            Assert.False(addResult);
        }

        [Fact]
        public void ShouldDoubleCreditLimit()
        {
            var client = new Client { Name = "ImportantClient" };
            this.clientRepositoryMock.Setup(c => c.GetById(It.IsAny<int>())).Returns(client);

            this.userCreditService.Setup(c => c.GetCreditLimit(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                                  .Returns(500);

            this.userRepositoryMock
                .Setup(c => c.AddUser(It.Is<User>(u => u.HasCreditLimit == true && u.CreditLimit == 1000)))
                .Verifiable();


            var date = new DateTime(1990, 1, 1);
            var addResult = this.userService.AddUser("John", "Doe", "John.doe@gmail.com", date, 4);
            Assert.True(addResult);
            this.userRepositoryMock.Verify();
        }
    }
}
