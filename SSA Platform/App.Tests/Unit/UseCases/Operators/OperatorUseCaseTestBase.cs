using App.Application.Interfaces;
using App.Application.Interfaces.IRepositories;
using App.Domain.Entities;
using App.Domain.Entities.Enums;
using App.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace App.Tests.Unit.UseCases.Operators;

public abstract class OperatorUseCaseTestBase
{
    protected readonly Mock<IUnitOfWork>                  Uow;
    protected readonly Mock<IOperatorRepository>          OperatorRepo;
    protected readonly Mock<IUserOperatorRepository>      UserOperatorRepo;
    protected readonly Mock<INotificationSettingRepository> NotificationRepo;
    protected readonly Mock<UserManager<AppUser>>         UserManager;

    protected OperatorUseCaseTestBase()
    {
        Uow              = new Mock<IUnitOfWork>();
        OperatorRepo     = new Mock<IOperatorRepository>();
        UserOperatorRepo = new Mock<IUserOperatorRepository>();
        NotificationRepo = new Mock<INotificationSettingRepository>();

        UserManager = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        Uow.Setup(u => u.Operators).Returns(OperatorRepo.Object);
        Uow.Setup(u => u.UserOperators).Returns(UserOperatorRepo.Object);
        Uow.Setup(u => u.NotificationSettings).Returns(NotificationRepo.Object);
    }

    protected static AppUser MakeUser(string email = "user@example.com") =>
        new AppUser
        {
            Id        = Guid.NewGuid(),
            UserName  = email,
            Email     = email,
            FirstName = "John",
            LastName  = "Doe"
        };

    protected static Operator MakeOperator(Guid? id = null) =>
        new Operator
        {
            Id          = id ?? Guid.NewGuid(),
            Name        = "TestCorp",
            CountryCode = "US",
            OperatorType = EOperatorType.Commercial,
            CreatedBy   = "system",
            CreatedAt   = DateTime.UtcNow
        };
}
