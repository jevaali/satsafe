using App.Application.DTO;
using App.Application.Interfaces;

namespace App.Application.UseCases;

public class GetMainListUseCase
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMainListUseCase(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MainListDTO> ExecuteAsync()
    {
        var satellites = await _unitOfWork.Satellites.GetAllAsync();
        var satelliteDtos = satellites.Select(s => new MainListSatDTO
        {
            Name = s.Name,
            Operator = s.OperatorName ?? "Unknown"
        }).ToList();
        
        var debris = await _unitOfWork.DebrisObjects.GetAllAsync();
        var debrisDtos = debris.Select(d => new MainListDebrisDTO
        {
            NoradId = d.NoradId,
            SizeEstimate = d.SizeEstimate
        }).ToList();

        return new MainListDTO
        {
            Satellites = satelliteDtos,
            Debris = debrisDtos
        };
    }
}