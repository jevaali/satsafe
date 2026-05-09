using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Operators.UseCases;

public class AddOperatorUseCase : BaseUseCase
{
    public AddOperatorUseCase(IUnitOfWork uow) : base(uow) { }

    public async Task<Guid> Execute(AddOperatorRequest request)
    {
        var operatorEntity = new Operator
        {
            Id           = Guid.NewGuid(),
            Name         = request.Name,
            CountryCode  = request.Country,
            OperatorType = request.OperatorType,
            CreatedBy    = "system",
            CreatedAt    = DateTime.UtcNow
        };

        Uow.Operators.Add(operatorEntity);

        await Uow.CommitAsync();

        return operatorEntity.Id;
    }
}
