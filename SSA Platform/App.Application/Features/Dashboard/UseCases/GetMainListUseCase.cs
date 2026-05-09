using App.Application.Features.Dashboard.DTOs;
using App.Application.Interfaces;
using App.Domain.Entities;
using Base.Contracts;

namespace App.Application.Features.Dashboard.UseCases;

public class GetMainListUseCase : BaseUseCase
{
    private readonly IBaseMapper<MainListSatDTO, Satellite> _satMapper;
    private readonly IBaseMapper<MainListDebrisDTO, DebrisObject> _debrisMapper;

    public GetMainListUseCase(
        IUnitOfWork uow,
        IBaseMapper<MainListSatDTO, Satellite> satMapper,
        IBaseMapper<MainListDebrisDTO, DebrisObject> debrisMapper) : base(uow)
    {
        _satMapper    = satMapper;
        _debrisMapper = debrisMapper;
    }

    public async Task<MainListDTO> ExecuteAsync()
    {
        var satellites = await Uow.Satellites.AllAsync();
        var debris     = await Uow.DebrisObjects.AllAsync();

        return new MainListDTO
        {
            Satellites = satellites.Select(_satMapper.Map).OfType<MainListSatDTO>().ToList(),
            Debris     = debris.Select(_debrisMapper.Map).OfType<MainListDebrisDTO>().ToList()
        };
    }
}
