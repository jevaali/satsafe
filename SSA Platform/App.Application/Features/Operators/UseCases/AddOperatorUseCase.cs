using App.Application.Features.Operators.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.Features.Operators.UseCases;

public class AddOperatorUseCase
{
    private readonly IUnitOfWork _uow;

    public AddOperatorUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Execute(AddOperatorRequest request)
    {
        var operatorEntity = new Operator
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CountryCode = request.Country,
            OperatorType = request.OperatorType,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Operators.AddAsync(operatorEntity);

        await _uow.CommitAsync();

        return operatorEntity.Id;
    }
}
