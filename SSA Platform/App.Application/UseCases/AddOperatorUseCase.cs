using App.Application.DTO;
using App.Application.Interfaces;
using App.Domain.Entities;

namespace App.Application.UseCases;

public class AddOperatorUseCase
{
    private readonly IUnitOfWork _uow;

    public AddOperatorUseCase(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Execute(AddOperatorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required");

        if (string.IsNullOrWhiteSpace(request.Country))
            throw new ArgumentException("Country is required");

        var operatorEntity = new Operator
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Country = request.Country,
            OperatorType = request.OperatorType,
            CreatedBy = "system",
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Operators.AddAsync(operatorEntity);

        await _uow.CommitAsync();

        return operatorEntity.Id;
    }
}