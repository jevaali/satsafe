using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels;

public class UserOperatorIndexViewModel
{
    public IEnumerable<UserOperatorListItemViewModel> Links { get; set; } = [];
    public int TotalCount { get; set; }
}

public class UserOperatorListItemViewModel
{
    public Guid   Id           { get; set; }
    public string UserName     { get; set; } = string.Empty;
    public string UserEmail    { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public Guid   OperatorId   { get; set; }
}

public class UserOperatorDetailViewModel
{
    public Guid   Id           { get; set; }
    public string UserName     { get; set; } = string.Empty;
    public string UserEmail    { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public Guid   OperatorId   { get; set; }
    public Guid   AppUserId    { get; set; }
}

public class UserOperatorFormViewModel
{
    public Guid? Id { get; set; }

    [Required]
    [Display(Name = "User")]
    public Guid AppUserId { get; set; }

    [Required]
    [Display(Name = "Operator")]
    public Guid OperatorId { get; set; }

    // Populated server-side
    public IEnumerable<SelectListItem> UserOptions     { get; set; } = [];
    public IEnumerable<SelectListItem> OperatorOptions { get; set; } = [];
}

public class UserOperatorDeleteViewModel
{
    public Guid   Id           { get; set; }
    public string UserName     { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
}
