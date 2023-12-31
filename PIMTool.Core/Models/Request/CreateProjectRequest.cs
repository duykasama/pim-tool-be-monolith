﻿using System.ComponentModel.DataAnnotations;
using PIMTool.Core.Attributes;

namespace PIMTool.Core.Models.Request;

public class CreateProjectRequest
{
    
    [RequiredGuid]
    public Guid GroupId { get; set; }
    
    [Range(typeof(int), "1", "9999")]
    public int ProjectNumber { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Customer { get; set; }
    
    [Required]
    [MaxLength(3)]
    public string Status { get; set; }

    public IList<Guid> MemberIds { get; set; } = new List<Guid>();
    
    [RequiredDate]
    [DateIsBefore(nameof(EndDate), "End Date must be after Start Date")]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; } 
}