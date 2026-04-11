using System.ComponentModel.DataAnnotations;
using App.Domain.Entities.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class OperatorIndexViewModel
{
    public IEnumerable<OperatorListItemViewModel> Operators { get; set; } = [];
    public int TotalCount { get; set; }
}

public class OperatorListItemViewModel
{
    public Guid   Id             { get; set; }
    public string Name           { get; set; } = string.Empty;
    public string CountryCode    { get; set; } = string.Empty;
    public string? OperatorType  { get; set; }
    public int    SatelliteCount { get; set; }
    public int    UserCount      { get; set; }
}

public class OperatorDetailViewModel
{
    public Guid   Id            { get; set; }
    public string Name          { get; set; } = string.Empty;
    public string CountryCode   { get; set; } = string.Empty;
    public string? OperatorType { get; set; }
    public int    SatelliteCount { get; set; }
    public int    UserCount      { get; set; }
    public string?   CreatedBy  { get; set; }
    public DateTime? CreatedAt  { get; set; }
    public string?   UpdatedBy  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
}

public class OperatorFormViewModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(124)]
    [Display(Name = "Organisation Name")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(2)]
    [Display(Name = "Country")]
    public string CountryCode { get; set; } = string.Empty;

    [Display(Name = "Operator Type")]
    public EOperatorType? OperatorType { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> CountryOptions   { get; set; } = [];
}

public class OperatorDeleteViewModel
{
    public Guid   Id           { get; set; }
    public string Name         { get; set; } = string.Empty;
    public string CountryCode  { get; set; } = string.Empty;
    public string? OperatorType { get; set; }
    public int    SatelliteCount { get; set; }
}
